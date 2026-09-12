using System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Contracts
{
    /// <summary>
    /// Neutral board presentation contract.
    /// Runtime gameplay depends on this contract, not on FieldManager/Cell/Level.
    /// </summary>
    public interface IBoardPresentation
    {
        event Action BoardChanged;

        bool IsReady { get; }
        int RowCount { get; }
        int ColumnCount { get; }
        float CellSize { get; }

        bool IsPlayable(int row, int column);
        bool IsOccupied(int row, int column);

        bool TryGetCellHandle(int row, int column, out UnityEngine.Object handle);
        bool TryGetCellPosition(UnityEngine.Object handle, out int row, out int column);
    }
}
