using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Placement
{
    public class PlacementResult
    {
        public bool Success { get; }
        public IReadOnlyList<BoardCoord> Cells { get; }

        public PlacementResult(bool success, IReadOnlyList<BoardCoord> cells)
        {
            Success = success;
            Cells = cells;
        }
    }
}
