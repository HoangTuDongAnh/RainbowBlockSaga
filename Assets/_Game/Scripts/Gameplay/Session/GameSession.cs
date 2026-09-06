using System;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Gameplay.Spawn;
using RainbowBlockSaga.Modes.Objectives;

namespace RainbowBlockSaga.Gameplay.Session
{
    public class GameSession
    {
        public BoardModel Board { get; }
        public PlacementService Placement { get; }
        public BoardResolver Resolver { get; }
        public ScoreSystem Score { get; }
        public BlockQueue Queue { get; }
        public IObjective Objective { get; }

        public bool IsStarted { get; private set; }
        public bool IsEnded { get; private set; }
        public bool IsPaused { get; private set; }
        public GameSessionResult Result { get; private set; }

        readonly SpawnStrategy spawnStrategy;
        readonly SpawnProfileData spawnProfile;
        readonly int adventureLevel;

        public event Action Started;
        public event Action<PlacementResult, BoardResolveResult, int> Resolved;
        public event Action<GameSessionResult> Ended;

        /// <summary>
        /// Clean-session constructor used by the final Adventure / Endless runtime.
        /// </summary>
        public GameSession(GameSessionConfig config)
            : this(
                new BoardModel(config.Board),
                new PlacementService(),
                new BoardResolver(),
                new ScoreSystem(config.ScoreRule),
                new BlockQueue(),
                config.Objective ? config.Objective.CreateRuntime() : null,
                config.Spawn,
                config.AdventureLevel)
        {
        }

        /// <summary>
        /// Migration constructor.
        /// Allows the toolkit presentation bridge to compose one GameSession around the
        /// already-migrated BoardModel while the original scene is still providing visuals.
        /// </summary>
        public GameSession(
            BoardModel board,
            PlacementService placement,
            BoardResolver resolver,
            ScoreSystem score,
            BlockQueue queue,
            IObjective objective,
            SpawnProfileData spawnProfile,
            int adventureLevel = 1)
        {
            Board = board;
            Placement = placement;
            Resolver = resolver;
            Score = score;
            Queue = queue;
            Objective = objective;

            this.spawnProfile = spawnProfile;
            this.adventureLevel = adventureLevel;
            spawnStrategy = new SpawnStrategy(Placement);
        }

        public void Start()
        {
            if (IsStarted && !IsEnded)
                return;

            IsStarted = true;
            IsEnded = false;
            IsPaused = false;
            Result = null;

            Board.ClearAll();
            Score.Reset();
            Objective?.Reset();

            EnsureBatch();
            Started?.Invoke();

            EvaluateEndState();
        }

        /// <summary>
        /// Starts ownership without resetting the current board.
        /// Used only while migrating the original BlockBlast scene.
        /// </summary>
        public void StartFromCurrentState()
        {
            if (IsStarted && !IsEnded)
                return;

            IsStarted = true;
            IsEnded = false;
            IsPaused = false;
            Result = null;
            Objective?.Reset();

            Started?.Invoke();
        }

        public bool TryPlace(BlockShapeData shape, BoardCoord anchor)
        {
            if (!CanInteractWith(shape))
                return false;

            var placement = Placement.Place(Board, shape, anchor);
            if (!placement.Success)
                return false;

            ResolvePlacement(shape, placement);
            return true;
        }

        /// <summary>
        /// Migration entry point. The toolkit drag presentation has already filled the
        /// BoardModel cells; this method performs the remaining session transaction.
        /// </summary>
        public GameSessionPlacementOutcome ResolveExternalPlacement(
            BlockShapeData shape,
            IReadOnlyList<BoardCoord> placedCoords)
        {
            if (!IsStarted ||
                IsEnded ||
                IsPaused ||
                shape == null)
                return null;

            var placement = new PlacementResult(
                true,
                new List<BoardCoord>(placedCoords));

            return ResolvePlacement(shape, placement);
        }

        GameSessionPlacementOutcome ResolvePlacement(
            BlockShapeData shape,
            PlacementResult placement)
        {
            Queue.Consume(shape);

            var resolve = Resolver.Resolve(Board);
            int gain = Score.Apply(placement, resolve);

            Objective?.OnGameplayResolved(
                Score.Score,
                placement,
                resolve);

            var outcome = new GameSessionPlacementOutcome(
                placement,
                resolve,
                gain);

            Resolved?.Invoke(placement, resolve, gain);

            if (Objective != null && Objective.IsCompleted)
            {
                Finish(GameSessionEndReason.ObjectiveCompleted);
                return outcome;
            }

            EnsureBatch();
            EvaluateEndState();

            return outcome;
        }

        public void EnsureBatch()
        {
            if (IsEnded || !Queue.IsEmpty || spawnProfile == null)
                return;

            Queue.SetBatch(
                spawnStrategy.CreateBatch(
                    Board,
                    spawnProfile,
                    adventureLevel));
        }

        public void SetExternalBatch(IEnumerable<BlockShapeData> shapes)
        {
            if (IsEnded)
                return;

            Queue.SetBatch(shapes);
        }

        public void AddExternalShape(BlockShapeData shape)
        {
            if (IsEnded || shape == null)
                return;

            Queue.Add(shape);
        }

        public bool HasAnyMove()
        {
            foreach (var shape in Queue.Shapes)
                if (Placement.HasAnyValidPlacement(Board, shape))
                    return true;

            return false;
        }

        public void EvaluateEndState()
        {
            if (IsEnded || Queue.IsEmpty)
                return;

            if (!HasAnyMove())
                Finish(GameSessionEndReason.NoValidMoves);
        }

        public void Pause()
        {
            if (IsStarted && !IsEnded)
                IsPaused = true;
        }

        public void Resume()
        {
            if (IsStarted && !IsEnded)
                IsPaused = false;
        }

        public void Cancel()
        {
            Finish(GameSessionEndReason.Cancelled);
        }

        bool CanInteractWith(BlockShapeData shape)
        {
            return IsStarted &&
                   !IsEnded &&
                   !IsPaused &&
                   shape != null &&
                   ContainsShape(shape);
        }

        bool ContainsShape(BlockShapeData shape)
        {
            foreach (var queued in Queue.Shapes)
                if (queued == shape)
                    return true;

            return false;
        }

        void Finish(GameSessionEndReason reason)
        {
            if (IsEnded)
                return;

            IsEnded = true;
            IsPaused = true;
            Result = new GameSessionResult(
                reason,
                Score.Score);

            Ended?.Invoke(Result);
        }
    }
}
