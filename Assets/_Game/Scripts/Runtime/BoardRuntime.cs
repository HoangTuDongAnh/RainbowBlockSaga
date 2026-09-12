using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Runtime board owner for the current gameplay scene.
    /// FieldManager provides board presentation while BoardModel + PlacementService own gameplay state.
    /// </summary>
    public class BoardRuntime : MonoBehaviour, ILevelLoadable
    {
        [FormerlySerializedAs("legacyField")]
        [SerializeField] FieldManager fieldManager;

        BoardData runtimeData;
        bool presentationSyncSuspended;

        public static BoardRuntime Current { get; private set; }
        public BoardModel Model { get; private set; }
        public BoardData Data => runtimeData;
        public FieldManager Field => fieldManager;
        public PlacementService Placement { get; } = new();

        void Awake()
        {
            Current = this;
        }

        public void OnLevelLoaded(Level level)
        {
            Build(level);
        }

        void LateUpdate()
        {
            if (presentationSyncSuspended || Model == null || fieldManager.cells == null)
                return;

            SyncStateFromPresentation();
        }

        void Build(Level level)
        {
            if (runtimeData)
                Destroy(runtimeData);

            runtimeData = ScriptableObject.CreateInstance<BoardData>();
            runtimeData.name = $"RuntimeBoard_{level.rows}x{level.columns}";
            runtimeData.Width = level.columns;
            runtimeData.Height = level.rows;
            runtimeData.PlayableCells = BuildPlayableCells(level);

            Model = new BoardModel(runtimeData);
            SyncStateFromPresentation();
        }

        List<BoardCoord> BuildPlayableCells(Level level)
        {
            var playable = new List<BoardCoord>(level.rows * level.columns);

            for (int row = 0; row < level.rows; row++)
            {
                for (int column = 0; column < level.columns; column++)
                {
                    if (!level.IsDisabled(row, column))
                        playable.Add(ToBoardCoord(row, column, level.rows));
                }
            }

            return playable;
        }

        public void SyncNow()
        {
            if (!presentationSyncSuspended && Model != null && fieldManager.cells != null)
                SyncStateFromPresentation();
        }

        public void SuspendPresentationSync()
        {
            presentationSyncSuspended = true;
        }

        public void ResumePresentationSync()
        {
            presentationSyncSuspended = false;

            if (Model != null && fieldManager.cells != null)
                SyncStateFromPresentation();
        }

        public bool TryGetCoord(Cell cell, out BoardCoord coord)
        {
            if (fieldManager.cells != null)
            {
                int rows = fieldManager.cells.GetLength(0);
                int columns = fieldManager.cells.GetLength(1);
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < columns; column++)
                    {
                        if (fieldManager.cells[row, column] != cell)
                            continue;

                        coord = ToBoardCoord(row, column, rows);
                        return true;
                    }
                }
            }

            coord = default;
            return false;
        }

        public bool TryGetCell(BoardCoord coord, out Cell cell)
        {
            cell = null;
            if (fieldManager.cells == null || Model == null || !Model.Contains(coord))
                return false;

            int row = Model.Height - 1 - coord.Y;
            int column = coord.X;
            if (row < 0 || row >= fieldManager.cells.GetLength(0) ||
                column < 0 || column >= fieldManager.cells.GetLength(1))
                return false;

            cell = fieldManager.cells[row, column];
            return cell != null;
        }

        void SyncStateFromPresentation()
        {
            int rows = fieldManager.cells.GetLength(0);
            int columns = fieldManager.cells.GetLength(1);

            if (Model.Width != columns || Model.Height != rows)
                return;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    var cell = fieldManager.cells[row, column];
                    var coord = ToBoardCoord(row, column, rows);

                    if (!Model.IsPlayable(coord))
                        continue;

                    if (cell.busy)
                        Model.SetOccupied(coord);
                    else
                        Model.SetEmpty(coord);
                }
            }
        }

        static BoardCoord ToBoardCoord(int row, int column, int rowCount)
        {
            return new BoardCoord(column, rowCount - 1 - row);
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;

            if (runtimeData)
                Destroy(runtimeData);
        }
    }
}
