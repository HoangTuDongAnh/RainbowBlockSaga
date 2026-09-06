using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Placement
{
    public class PlacementService
    {
        public List<BoardCoord> GetPlacementCells(BlockShapeData shape, BoardCoord anchor)
        {
            var result = new List<BoardCoord>(shape.Cells.Count);
            foreach (var offset in shape.GetNormalizedCells()) result.Add(anchor + offset);
            return result;
        }

        public bool CanPlace(BoardModel board, BlockShapeData shape, BoardCoord anchor)
        {
            foreach (var coord in GetPlacementCells(shape, anchor))
                if (!board.IsEmpty(coord))
                    return false;
            return true;
        }

        public PlacementResult Place(BoardModel board, BlockShapeData shape, BoardCoord anchor)
        {
            var coords = GetPlacementCells(shape, anchor);
            if (!CanPlace(board, shape, anchor)) return new PlacementResult(false, coords);
            foreach (var coord in coords) board.SetOccupied(coord);
            return new PlacementResult(true, coords);
        }

        public List<BoardCoord> GetValidPlacements(BoardModel board, BlockShapeData shape)
        {
            var result = new List<BoardCoord>();
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                {
                    var anchor = new BoardCoord(x, y);
                    if (CanPlace(board, shape, anchor)) result.Add(anchor);
                }
            return result;
        }

        public bool HasAnyValidPlacement(BoardModel board, BlockShapeData shape)
        {
            for (int y = 0; y < board.Height; y++)
                for (int x = 0; x < board.Width; x++)
                    if (CanPlace(board, shape, new BoardCoord(x, y)))
                        return true;
            return false;
        }
    }
}
