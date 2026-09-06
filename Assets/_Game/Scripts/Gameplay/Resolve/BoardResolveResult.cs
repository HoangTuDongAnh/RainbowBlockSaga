using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Resolve
{
    public class BoardResolveResult
    {
        public int ClearedLines { get; }
        public IReadOnlyCollection<BoardCoord> ClearedCells { get; }
        public IReadOnlyList<int> Rows { get; }
        public IReadOnlyList<int> Columns { get; }

        public BoardResolveResult(
            int clearedLines,
            IReadOnlyCollection<BoardCoord> clearedCells,
            IReadOnlyList<int> rows,
            IReadOnlyList<int> columns)
        {
            ClearedLines = clearedLines;
            ClearedCells = clearedCells;
            Rows = rows;
            Columns = columns;
        }

        public BoardResolveResult(int clearedLines, IReadOnlyCollection<BoardCoord> clearedCells)
            : this(clearedLines, clearedCells, new List<int>(), new List<int>())
        {
        }
    }
}
