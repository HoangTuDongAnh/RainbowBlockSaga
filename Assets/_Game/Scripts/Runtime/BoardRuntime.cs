using System;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Owns the gameplay board state for the active scene.
    /// It depends only on IBoardPresentation, never on presentation implementation types.
    /// </summary>
    public class BoardRuntime : MonoBehaviour
    {
        [FormerlySerializedAs("legacyField")]
        [SerializeField] MonoBehaviour fieldManager;

        IBoardPresentation presentation;
        BoardData runtimeData;
        bool presentationSyncSuspended;

        public static BoardRuntime Current { get; private set; }

        public event Action ModelReplaced;

        public BoardModel Model { get; private set; }
        public BoardData Data => runtimeData;
        public PlacementService Placement { get; } = new();

        /// <summary>
        /// Exposed only for presentation-side controllers that still need their concrete
        /// component during the next migration phases.
        /// BoardRuntime itself never casts this back to a toolkit type.
        /// </summary>
        public MonoBehaviour PresentationSource => fieldManager;
        public IBoardPresentation Presentation => presentation;

        void Awake()
        {
            Current = this;

            presentation = fieldManager as IBoardPresentation;
            if (presentation == null)
                throw new InvalidOperationException(
                    "BoardRuntime requires a component implementing IBoardPresentation.");

            presentation.BoardChanged += OnBoardChanged;

            if (presentation.IsReady)
                RebuildFromPresentation();
        }

        void LateUpdate()
        {
            if (!presentation.IsReady)
                return;

            if (Model == null ||
                Model.Width != presentation.ColumnCount ||
                Model.Height != presentation.RowCount)
            {
                RebuildFromPresentation();
                return;
            }

            if (!presentationSyncSuspended)
                SyncStateFromPresentation();
        }

        void OnBoardChanged()
        {
            RebuildFromPresentation();
        }

        void RebuildFromPresentation()
        {
            if (!presentation.IsReady ||
                presentation.RowCount <= 0 ||
                presentation.ColumnCount <= 0)
                return;

            // A normal Classic restart / restore recreates the presentation Cells,
            // but it is still the same logical board layout. Keep the same BoardModel
            // instance so an active GameSession never becomes detached from the board
            // that is currently visible.
            if (CanReuseCurrentModel())
            {
                SyncStateFromPresentation();
                return;
            }

            if (runtimeData)
                Destroy(runtimeData);

            runtimeData = ScriptableObject.CreateInstance<BoardData>();
            runtimeData.name =
                "RuntimeBoard_" +
                presentation.RowCount +
                "x" +
                presentation.ColumnCount;

            runtimeData.Width = presentation.ColumnCount;
            runtimeData.Height = presentation.RowCount;
            runtimeData.PlayableCells = BuildPlayableCells();

            Model = new BoardModel(runtimeData);
            SyncStateFromPresentation();
            ModelReplaced?.Invoke();
        }

        bool CanReuseCurrentModel()
        {
            if (Model == null ||
                Model.Width != presentation.ColumnCount ||
                Model.Height != presentation.RowCount)
                return false;

            for (int row = 0; row < presentation.RowCount; row++)
            {
                for (int column = 0;
                     column < presentation.ColumnCount;
                     column++)
                {
                    var coord = ToBoardCoord(row, column);

                    if (Model.IsPlayable(coord) !=
                        presentation.IsPlayable(row, column))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        List<BoardCoord> BuildPlayableCells()
        {
            var playable = new List<BoardCoord>(
                presentation.RowCount * presentation.ColumnCount);

            for (int row = 0; row < presentation.RowCount; row++)
            {
                for (int column = 0;
                     column < presentation.ColumnCount;
                     column++)
                {
                    if (presentation.IsPlayable(row, column))
                        playable.Add(ToBoardCoord(row, column));
                }
            }

            return playable;
        }

        public void SyncNow()
        {
            if (!presentationSyncSuspended &&
                Model != null &&
                presentation.IsReady)
            {
                SyncStateFromPresentation();
            }
        }

        public void SuspendPresentationSync()
        {
            presentationSyncSuspended = true;
        }

        public void ResumePresentationSync()
        {
            presentationSyncSuspended = false;
            SyncNow();
        }

        public bool TryGetCoord(
            UnityEngine.Object cellHandle,
            out BoardCoord coord)
        {
            if (presentation.TryGetCellPosition(
                    cellHandle,
                    out int row,
                    out int column))
            {
                coord = ToBoardCoord(row, column);
                return true;
            }

            coord = default;
            return false;
        }

        public bool TryGetCellHandle(
            BoardCoord coord,
            out UnityEngine.Object cellHandle)
        {
            cellHandle = null;

            if (Model == null || !Model.Contains(coord))
                return false;

            int row = Model.Height - 1 - coord.Y;
            int column = coord.X;

            return presentation.TryGetCellHandle(
                row,
                column,
                out cellHandle);
        }

        void SyncStateFromPresentation()
        {
            if (Model == null ||
                Model.Width != presentation.ColumnCount ||
                Model.Height != presentation.RowCount)
                return;

            for (int row = 0; row < presentation.RowCount; row++)
            {
                for (int column = 0;
                     column < presentation.ColumnCount;
                     column++)
                {
                    var coord = ToBoardCoord(row, column);

                    if (!Model.IsPlayable(coord))
                        continue;

                    if (presentation.IsOccupied(row, column))
                        Model.SetOccupied(coord);
                    else
                        Model.SetEmpty(coord);
                }
            }
        }

        BoardCoord ToBoardCoord(int row, int column)
        {
            return new BoardCoord(
                column,
                presentation.RowCount - 1 - row);
        }

        void OnDestroy()
        {
            if (presentation != null)
                presentation.BoardChanged -= OnBoardChanged;

            if (Current == this)
                Current = null;

            if (runtimeData)
                Destroy(runtimeData);
        }
    }
}
