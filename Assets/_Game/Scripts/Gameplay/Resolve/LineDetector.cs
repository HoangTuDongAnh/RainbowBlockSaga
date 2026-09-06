using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Resolve
{
    public class LineDetector
    {
        public LineDetectionResult Detect(BoardModel board)
        {
            var result = new LineDetectionResult();
            DetectRows(board, result);
            DetectColumns(board, result);
            return result;
        }

        void DetectRows(BoardModel board, LineDetectionResult result)
        {
            for (int y = 0; y < board.Height; y++)
            {
                bool hasPlayable = false;
                bool full = true;
                for (int x = 0; x < board.Width; x++)
                {
                    var coord = new BoardCoord(x, y);
                    if (!board.IsPlayable(coord)) continue;
                    hasPlayable = true;
                    if (!board.IsOccupied(coord)) { full = false; break; }
                }
                if (!hasPlayable || !full) continue;
                result.Rows.Add(y);
                for (int x = 0; x < board.Width; x++)
                {
                    var coord = new BoardCoord(x, y);
                    if (board.IsPlayable(coord)) result.Cells.Add(coord);
                }
            }
        }

        void DetectColumns(BoardModel board, LineDetectionResult result)
        {
            for (int x = 0; x < board.Width; x++)
            {
                bool hasPlayable = false;
                bool full = true;
                for (int y = 0; y < board.Height; y++)
                {
                    var coord = new BoardCoord(x, y);
                    if (!board.IsPlayable(coord)) continue;
                    hasPlayable = true;
                    if (!board.IsOccupied(coord)) { full = false; break; }
                }
                if (!hasPlayable || !full) continue;
                result.Columns.Add(x);
                for (int y = 0; y < board.Height; y++)
                {
                    var coord = new BoardCoord(x, y);
                    if (board.IsPlayable(coord)) result.Cells.Add(coord);
                }
            }
        }
    }
}
