using System;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Gameplay.Spawn;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Gameplay composition root.
    /// Creates and owns the GameSession used by the gameplay scene. Presentation managers remain responsible for UI and visual flow.
    /// </summary>
    public class GameSessionRuntime : MonoBehaviour
    {
        [FormerlySerializedAs("boardBridge")]
        [SerializeField] BoardRuntime boardRuntime;
        [SerializeField] CellDeckManager deckManager;
        [SerializeField] ItemFactory itemFactory;

        SpawnProfileData runtimeSpawnProfile;
        ScoreRuleData runtimeScoreRule;
        bool legacyScoreInitialized;

        public static GameSessionRuntime Current { get; private set; }

        public GameSession Session { get; private set; }
        public ItemFactory ItemFactory => itemFactory;
        public CellDeckManager DeckManager => deckManager;

        public event Action<GameSession> SessionCreated;
        public event Action<GameSessionResult> SessionEnded;

        void OnEnable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel)
                .Subscribe(OnRestartLevel);
            EventManager.OnGameStateChanged += OnGameStateChanged;
        }

        void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel)
                .Unsubscribe(OnRestartLevel);
            EventManager.OnGameStateChanged -= OnGameStateChanged;
        }

        void Awake()
        {
            Current = this;

            runtimeSpawnProfile =
                ScriptableObject.CreateInstance<SpawnProfileData>();
            runtimeSpawnProfile.name =
                "Runtime_SessionSpawnProfile";
            runtimeSpawnProfile.BatchSize =
                deckManager.cellDecks.Length;
            runtimeSpawnProfile.EnsureAtLeastOnePlayable = true;
            runtimeSpawnProfile.MinPlayablePerBatch =
                Mathf.Min(2, deckManager.cellDecks.Length);
            runtimeSpawnProfile.PreferDistinctShapes = true;

            runtimeScoreRule =
                ScriptableObject.CreateInstance<ScoreRuleData>();
            runtimeScoreRule.name =
                "Runtime_SessionScoreRule";
            runtimeScoreRule.PlacementScorePerCell = 0;
            runtimeScoreRule.BaseLineScore =
                GameManager.instance.GameSettings.ScorePerLine;
            runtimeScoreRule.UseComboStreak = true;
            runtimeScoreRule.ResetComboAfterMisses =
                GameManager.instance.GameSettings.ResetComboAfterMoves;
        }

        public GameSession GetOrCreateSession()
        {
            if (Session != null)
                return Session;

            if (boardRuntime == null ||
                boardRuntime.Model == null)
                return null;

            RefreshSpawnProfile();

            Session = new GameSession(
                boardRuntime.Model,
                boardRuntime.Placement,
                new BoardResolver(),
                new ScoreSystem(runtimeScoreRule),
                new BlockQueue(),
                null,
                runtimeSpawnProfile,
                int.MaxValue);

            Session.Ended += OnSessionEnded;
            Session.StartFromCurrentState();

            InitializeLegacyScore();
            SynchronizeQueueFromVisualDecks();
            ApplyLegacyGameState(EventManager.GameStatus);

            SessionCreated?.Invoke(Session);
            return Session;
        }

        void OnRestartLevel()
        {
            ResetSession();
        }

        void OnGameStateChanged(EGameState state)
        {
            ApplyLegacyGameState(state);
        }

        void ApplyLegacyGameState(EGameState state)
        {
            if (Session == null)
                return;

            if (state == EGameState.Pause ||
                state == EGameState.Paused ||
                state == EGameState.PreFailed ||
                state == EGameState.Failed ||
                state == EGameState.PreWin ||
                state == EGameState.Win ||
                state == EGameState.WinWaiting)
            {
                Session.Pause();
                return;
            }

            if (state == EGameState.Playing ||
                state == EGameState.Tutorial)
            {
                Session.Resume();
            }
        }

        public void ResetSession()
        {
            if (Session == null)
            {
                legacyScoreInitialized = false;
                return;
            }

            Session.Ended -= OnSessionEnded;

            if (!Session.IsEnded)
                Session.Cancel();

            Session = null;
            legacyScoreInitialized = false;
        }

        public void RecoverFromNoMoves()
        {
            int score = Session != null ? Session.Score.Score : 0;

            ResetSession();

            // The board remains untouched: PreFailed Continue is a deck recovery,
            // not a level restart.
            var recovered = GetOrCreateSession();
            if (recovered != null)
            {
                recovered.Score.SetScore(score);
                recovered.Resume();
            }
        }

        public void SynchronizeQueueFromVisualDecks()
        {
            var session = Session;
            if (session == null || session.IsEnded)
                return;

            var visualShapes = deckManager.GetShapes();
            var batch = new System.Collections.Generic.List<BlockShapeData>(
                visualShapes.Length);

            foreach (var visualShape in visualShapes)
            {
                if (visualShape == null ||
                    visualShape.shapeTemplate == null)
                    continue;

                var data =
                    ShapeDataAdapter.GetOrCreate(
                        visualShape.shapeTemplate);

                if (data != null)
                    batch.Add(data);
            }

            // If visual decks already contain shapes, they are the migration boundary's
            // current truth. This prevents lifecycle ordering from leaving Session.Queue stale.
            if (batch.Count > 0)
                session.SetExternalBatch(batch);
        }

        void InitializeLegacyScore()
        {
            if (legacyScoreInitialized ||
                Session == null)
                return;

            legacyScoreInitialized = true;

            var classic =
                FindFirstObjectByType<ClassicModeHandler>(
                    FindObjectsInactive.Include);

            if (classic != null)
            {
                Session.Score.SetScore(classic.score);
                return;
            }

            var timed =
                FindFirstObjectByType<TimedModeHandler>(
                    FindObjectsInactive.Include);

            if (timed != null)
                Session.Score.SetScore(timed.score);
        }

        public void RefreshSpawnProfile()
        {
            runtimeSpawnProfile.BatchSize =
                deckManager.cellDecks.Length;
            runtimeSpawnProfile.Shapes.Clear();

            var eligible =
                itemFactory.GetEligibleShapeTemplates();

            foreach (var template in eligible)
            {
                var data =
                    ShapeDataAdapter.GetOrCreate(template);

                if (data != null)
                    runtimeSpawnProfile.Shapes.Add(data);
            }
        }

        void OnSessionEnded(GameSessionResult result)
        {
            SessionEnded?.Invoke(result);
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;

            ResetSession();

            if (runtimeSpawnProfile != null)
                Destroy(runtimeSpawnProfile);

            if (runtimeScoreRule != null)
                Destroy(runtimeScoreRule);
        }
    }
}
