using System;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;

namespace RainbowBlockSaga.Integration.Toolkit
{
    /// <summary>
    /// Step 6 resolve presentation bridge.
    /// GameSession now owns queue consumption, resolve, scoring and end-state evaluation.
    /// LevelManager is only used to reproduce the original toolkit FX / UI.
    /// </summary>
    public class ToolkitResolveScoreMigrationBridge : MonoBehaviour
    {
        [SerializeField] LevelManager levelManager;
        [SerializeField] ToolkitBoardMigrationBridge boardBridge;
        [SerializeField] ToolkitGameSessionMigrationBridge sessionBridge;

        public static ToolkitResolveScoreMigrationBridge Current { get; private set; }

        public bool IsPresenting { get; private set; }
        public event Action PresentationCompleted;

        public GameSession Session =>
            sessionBridge.GetOrCreateSession();

        public RainbowBlockSaga.Gameplay.Score.ScoreSystem Score =>
            Session?.Score;

        void Awake()
        {
            Current = this;
        }

        void OnEnable()
        {
            LevelManager.ExternalResolveEnabled = true;
        }

        void OnDisable()
        {
            LevelManager.ExternalResolveEnabled = false;
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;

            LevelManager.ExternalResolveEnabled = false;
        }

        public bool TryResolvePlacement(
            Shape legacyShape,
            IReadOnlyList<BoardCoord> placedCoords)
        {
            var session = Session;

            if (legacyShape == null ||
                session == null ||
                session.IsEnded)
                return false;

            // CellDeck visuals can be populated before GameSession is lazily created.
            // Reconcile the new queue from the actual visible slots before consuming this shape.
            sessionBridge.SynchronizeQueueFromVisualDecks();

            var shapeData =
                ToolkitShapeDataAdapter.GetOrCreate(
                    legacyShape.shapeTemplate);

            var outcome =
                session.ResolveExternalPlacement(
                    shapeData,
                    placedCoords);

            if (outcome == null)
                return false;

            var legacyLines =
                BuildLegacyLines(outcome.Resolve);

            if (outcome.Resolve.ClearedLines > 0)
                boardBridge.SuspendToolkitSync();

            IsPresenting = true;

            levelManager.PresentExternalResolve(
                legacyShape,
                legacyLines,
                outcome.ScoreGain,
                session.Score.Combo,
                () => OnPresentationCompleted(
                    outcome.Resolve.ClearedLines > 0));

            return true;
        }

        List<List<Cell>> BuildLegacyLines(
            BoardResolveResult resolve)
        {
            var lines = new List<List<Cell>>();

            foreach (int y in resolve.Rows)
            {
                var line = new List<Cell>();

                for (int x = 0;
                     x < boardBridge.Model.Width;
                     x++)
                {
                    var coord = new BoardCoord(x, y);

                    if (!boardBridge.Model.IsPlayable(coord))
                        continue;

                    if (boardBridge.TryGetCell(
                            coord,
                            out var cell))
                        line.Add(cell);
                }

                if (line.Count > 0)
                    lines.Add(line);
            }

            foreach (int x in resolve.Columns)
            {
                var line = new List<Cell>();

                for (int y = 0;
                     y < boardBridge.Model.Height;
                     y++)
                {
                    var coord = new BoardCoord(x, y);

                    if (!boardBridge.Model.IsPlayable(coord))
                        continue;

                    if (boardBridge.TryGetCell(
                            coord,
                            out var cell))
                        line.Add(cell);
                }

                if (line.Count > 0)
                    lines.Add(line);
            }

            return lines;
        }

        void OnPresentationCompleted(bool hadClear)
        {
            if (hadClear)
                boardBridge.ResumeToolkitSync();

            IsPresenting = false;
            PresentationCompleted?.Invoke();
        }
    }
}
