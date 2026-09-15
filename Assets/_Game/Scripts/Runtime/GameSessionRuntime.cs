using System;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Gameplay.Spawn;
using RainbowBlockSaga.Presentation.Contracts;
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
        [FormerlySerializedAs("deckManager")]
        [SerializeField] MonoBehaviour trayPresentationSource;

        [FormerlySerializedAs("itemFactory")]
        [SerializeField] MonoBehaviour shapeCatalogSource;

        IBlockTrayPresentation trayPresentation;
        IShapeCatalog shapeCatalog;
        IGameplaySessionPresentation sessionPresentation;

        SpawnProfileData runtimeSpawnProfile;
        ScoreRuleData runtimeScoreRule;
        bool presentationScoreInitialized;
        bool freshRestartPending;

        public static GameSessionRuntime Current { get; private set; }

        public GameSession Session { get; private set; }
        public IBlockTrayPresentation TrayPresentation => trayPresentation;
        public IShapeCatalog ShapeCatalog => shapeCatalog;
        public GameplaySessionState CurrentPresentationState =>
            sessionPresentation != null
                ? sessionPresentation.CurrentState
                : GameplaySessionState.Unknown;

        public event Action<GameSession> SessionCreated;
        public event Action SessionReset;
        public event Action<GameSessionResult> SessionEnded;

        void OnEnable()
        {
            if (sessionPresentation != null)
            {
                sessionPresentation.RestartRequested += OnRestartLevel;
                sessionPresentation.StateChanged += OnGameStateChanged;
            }

            if (boardRuntime != null)
                boardRuntime.ModelReplaced += OnBoardModelReplaced;
        }

        void OnDisable()
        {
            if (sessionPresentation != null)
            {
                sessionPresentation.RestartRequested -= OnRestartLevel;
                sessionPresentation.StateChanged -= OnGameStateChanged;
            }

            if (boardRuntime != null)
                boardRuntime.ModelReplaced -= OnBoardModelReplaced;
        }

        void Awake()
        {
            Current = this;

            trayPresentation =
                trayPresentationSource as IBlockTrayPresentation;
            shapeCatalog =
                shapeCatalogSource as IShapeCatalog;
            sessionPresentation =
                shapeCatalogSource as IGameplaySessionPresentation;

            if (trayPresentation == null)
                throw new InvalidOperationException(
                    "GameSessionRuntime requires an IBlockTrayPresentation source.");

            if (shapeCatalog == null)
                throw new InvalidOperationException(
                    "GameSessionRuntime requires an IShapeCatalog source.");

            if (sessionPresentation == null)
                throw new InvalidOperationException(
                    "GameSessionRuntime requires the shape catalog source to also implement IGameplaySessionPresentation.");

            runtimeSpawnProfile =
                ScriptableObject.CreateInstance<SpawnProfileData>();
            runtimeSpawnProfile.name =
                "Runtime_SessionSpawnProfile";
            runtimeSpawnProfile.BatchSize =
                trayPresentation.SlotCount;
            runtimeSpawnProfile.EnsureAtLeastOnePlayable = true;
            runtimeSpawnProfile.MinPlayablePerBatch =
                Mathf.Min(2, trayPresentation.SlotCount);
            runtimeSpawnProfile.PreferDistinctShapes = true;

            runtimeScoreRule =
                ScriptableObject.CreateInstance<ScoreRuleData>();
            runtimeScoreRule.name =
                "Runtime_SessionScoreRule";
        }

        void RefreshScoreRule()
        {
            runtimeScoreRule.PlacementScorePerCell = sessionPresentation.ScorePerCell;
            runtimeScoreRule.ClearScorePerCell = sessionPresentation.ScorePerCell;
            runtimeScoreRule.UseComboStreak = true;
            runtimeScoreRule.IsEndless = sessionPresentation.IsEndlessScoring;
            runtimeScoreRule.Endless = sessionPresentation.EndlessScoring ?? new EndlessScoringSettings();
            runtimeScoreRule.ResetComboAfterMisses =
                sessionPresentation.ResetComboAfterMoves;
        }

        public GameSession GetOrCreateSession()
        {
            if (Session != null && runtimeScoreRule.IsEndless != sessionPresentation.IsEndlessScoring)
                ResetSession();
            if (Session != null)
                return Session;

            if (boardRuntime == null ||
                boardRuntime.Model == null)
                return null;

            RefreshSpawnProfile();
            RefreshScoreRule();

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

            InitializePresentationScore();

            if (!freshRestartPending)
                SynchronizeQueueFromVisualDecks();

            freshRestartPending = false;

            ApplyPresentationGameState(sessionPresentation.CurrentState);

            SessionCreated?.Invoke(Session);
            return Session;
        }

        void OnRestartLevel()
        {
            // Restart means a brand-new Classic/Timed run.
            // Do not let stale visual deck contents become the new Session.Queue.
            freshRestartPending = true;

            ResetSession();

            trayPresentation?.Clear();

            ResetPresentationScore();
        }

        void ResetPresentationScore()
        {
            sessionPresentation.ResetCurrentScore();
        }

        void OnBoardModelReplaced()
        {
            // A different board layout invalidates the board reference owned by
            // the current session. Recreate the session around the new model.
            ResetSession();
        }

        void OnGameStateChanged(GameplaySessionState state)
        {
            ApplyPresentationGameState(state);
        }

        void ApplyPresentationGameState(GameplaySessionState state)
        {
            if (Session == null)
                return;

            if (state == GameplaySessionState.Paused ||
                state == GameplaySessionState.PreFailed ||
                state == GameplaySessionState.Failed ||
                state == GameplaySessionState.PreWin ||
                state == GameplaySessionState.Win ||
                state == GameplaySessionState.WinWaiting)
            {
                Session.Pause();
                return;
            }

            if (state == GameplaySessionState.Playing ||
                state == GameplaySessionState.Tutorial)
            {
                Session.Resume();
            }
        }

        public void ResetSession()
        {
            SessionReset?.Invoke();
            if (Session == null)
            {
                presentationScoreInitialized = false;
                return;
            }

            Session.Ended -= OnSessionEnded;

            if (!Session.IsEnded)
                Session.Cancel();

            Session = null;
            presentationScoreInitialized = false;
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

            var visibleHandles =
                trayPresentation.GetVisibleShapeHandles();

            var batch = new System.Collections.Generic.List<BlockShapeData>(
                visibleHandles.Length);

            foreach (var handle in visibleHandles)
            {
                var data =
                    ShapeDataAdapter.GetOrCreate(
                        handle,
                        shapeCatalog);

                if (data != null)
                    batch.Add(data);
            }

            // Visible tray contents are presentation truth during normal play/resume.
            // Fresh restart deliberately skips this path so stale shapes cannot seed the new queue.
            if (batch.Count > 0)
                session.SetExternalBatch(batch);
        }

        void InitializePresentationScore()
        {
            if (presentationScoreInitialized ||
                Session == null)
                return;

            presentationScoreInitialized = true;
            Session.Score.SetScore(sessionPresentation.CurrentScore);
        }

        public void RefreshSpawnProfile()
        {
            runtimeSpawnProfile.BatchSize =
                trayPresentation.SlotCount;
            runtimeSpawnProfile.MinPlayablePerBatch =
                Mathf.Min(2, trayPresentation.SlotCount);
            runtimeSpawnProfile.Shapes.Clear();

            var eligible =
                shapeCatalog.GetEligibleShapes();

            foreach (var descriptor in eligible)
            {
                var data =
                    ShapeDataAdapter.GetOrCreate(descriptor);

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
