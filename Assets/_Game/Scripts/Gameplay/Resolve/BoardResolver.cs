using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Resolve
{
    public class BoardResolver
    {
        readonly LineDetector detector = new();

        public BoardResolveResult Resolve(BoardModel board)
        {
            var detection = detector.Detect(board);

            foreach (var coord in detection.Cells)
                board.SetEmpty(coord);

            return new BoardResolveResult(
                detection.LineCount,
                detection.Cells,
                new List<int>(detection.Rows),
                new List<int>(detection.Columns));
        }
    }
}
