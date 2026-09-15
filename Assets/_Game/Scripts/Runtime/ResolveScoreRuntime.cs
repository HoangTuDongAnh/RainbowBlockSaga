using System;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Resolve;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Owns the resolve/score runtime flow.
    /// Visual effects are delegated through IResolvePresentation.
    /// </summary>
    public class ResolveScoreRuntime : MonoBehaviour
    {
        [FormerlySerializedAs("levelManager")]
        [SerializeField] MonoBehaviour resolvePresentationSource;

        [FormerlySerializedAs("boardBridge")]
        [SerializeField] BoardRuntime boardRuntime;

        [FormerlySerializedAs("sessionBridge")]
        [SerializeField] GameSessionRuntime sessionRuntime;

        IResolvePresentation resolvePresentation;

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

            resolvePresentation =
                resolvePresentationSource as IResolvePresentation;

            if (resolvePresentation == null)
                throw new InvalidOperationException(
                    "ResolveScoreRuntime requires an IResolvePresentation source.");
        }

        void OnEnable()
        {
            resolvePresentation?.SetRuntimeResolveOwnership(true);
            sessionRuntime.SessionReset += ResetPresentation;
        }

        void OnDisable()
        {
            sessionRuntime.SessionReset -= ResetPresentation;
            IsPresenting = false;
            resolvePresentation?.SetRuntimeResolveOwnership(false);
        }

        void ResetPresentation()
        {
            IsPresenting = false;
            boardRuntime.ResumePresentationSync();
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;

            resolvePresentation?.SetRuntimeResolveOwnership(false);
        }

        public bool TryResolvePlacement(
            RainbowBlockSaga.Gameplay.Block.BlockShapeData shapeData,
            UnityEngine.Object presentationShapeHandle,
            IReadOnlyList<BoardCoord> placedCoords)
        {
            var session = Session;

            if (shapeData == null ||
                presentationShapeHandle == null ||
                session == null ||
                session.IsEnded)
                return false;

            // Visible tray is presentation truth immediately before consumption.
            sessionRuntime.SynchronizeQueueFromVisualDecks();

            var outcome =
                session.ResolveExternalPlacement(
                    shapeData,
                    placedCoords);

            if (outcome == null)
                return false;

            var presentationLines =
                BuildPresentationLines(outcome.Resolve);

            if (outcome.Resolve.ClearedLines > 0)
                boardRuntime.SuspendPresentationSync();

            IsPresenting = true;

            resolvePresentation.PresentResolve(
                presentationShapeHandle,
                presentationLines,
                outcome.ScoreGain,
                session.Score.Combo,
                session.Score.LastTurn,
                () => OnPresentationCompleted(
                    outcome.Resolve.ClearedLines > 0));

            return true;
        }

        List<IReadOnlyList<UnityEngine.Object>> BuildPresentationLines(
            BoardResolveResult resolve)
        {
            var lines =
                new List<IReadOnlyList<UnityEngine.Object>>();

            foreach (int y in resolve.Rows)
            {
                var line = new List<UnityEngine.Object>();

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
                        handle != null)
                    {
                        line.Add(handle);
                    }
                }

                if (line.Count > 0)
                    lines.Add(line);
            }

            foreach (int x in resolve.Columns)
            {
                var line = new List<UnityEngine.Object>();

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
                        handle != null)
                    {
                        line.Add(handle);
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
