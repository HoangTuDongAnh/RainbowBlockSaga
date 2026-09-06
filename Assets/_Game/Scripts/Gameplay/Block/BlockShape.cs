using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Block
{
    public class BlockShape
    {
        public BlockShapeData Data { get; }
        public IReadOnlyList<BoardCoord> Cells { get; }

        public BlockShape(BlockShapeData data)
        {
            Data = data;
            Cells = data.GetNormalizedCells();
        }
    }
}
