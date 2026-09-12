using System;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Runtime resolve/score presentation controller.
    /// GameSession owns queue consumption, resolve, scoring and end-state evaluation. LevelManager presents FX and UI.
    /// </summary>
    public class ResolveScoreRuntime : MonoBehaviour
    {
        [SerializeField] LevelManager levelManager;
        [FormerlySerializedAs("boardBridge")]
        [SerializeField] BoardRuntime boardRuntime;
        [FormerlySerializedAs("sessionBridge")]
        [SerializeField] GameSessionRuntime sessionRuntime;

        public static ResolveScoreRuntime Current { get; private set; }

        public bool IsPresenting { get; private set; }
        public event Action PresentationCompleted;

        public GameSession Session =>
            sessionRuntime.GetOrCreateSession();

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
            sessionRuntime.SynchronizeQueueFromVisualDecks();

            var shapeData =
                ShapeDataAdapter.GetOrCreate(
                    legacyShape.shapeTemplate,
                    sessionRuntime.ShapeCatalog);

            var outcome =
                session.ResolveExternalPlacement(
                    shapeData,
                    placedCoords);

            if (outcome == null)
                return false;

            var legacyLines =
                BuildLegacyLines(outcome.Resolve);

            if (outcome.Resolve.ClearedLines > 0)
                boardRuntime.SuspendPresentationSync();

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
                     x < boardRuntime.Model.Width;
                     x++)
                {
                    var coord = new BoardCoord(x, y);

                    if (!boardRuntime.Model.IsPlayable(coord))
                        continue;

                    if (boardRuntime.TryGetCellHandle(
                            coord,
                            out var handle) &&
                        handle is Cell cell)
                    {
                        line.Add(cell);
                    }
                }

                if (line.Count > 0)
                    lines.Add(line);
            }

            foreach (int x in resolve.Columns)
            {
                var line = new List<Cell>();

                for (int y = 0;
                     y < boardRuntime.Model.Height;
                     y++)
                {
                    var coord = new BoardCoord(x, y);

                    if (!boardRuntime.Model.IsPlayable(coord))
                        continue;

                    if (boardRuntime.TryGetCellHandle(
                            coord,
                            out var handle) &&
                        handle is Cell cell)
                    {
                        line.Add(cell);
                    }
                }

                if (line.Count > 0)
                    lines.Add(line);
            }

            return lines;
        }

        void OnPresentationCompleted(bool hadClear)
        {
            if (hadClear)
                boardRuntime.ResumePresentationSync();

            IsPresenting = false;
            PresentationCompleted?.Invoke();
        }
    }
}
