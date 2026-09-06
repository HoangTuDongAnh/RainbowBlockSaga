using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;

namespace RainbowBlockSaga.Gameplay.Resolve
{
    public class LineDetectionResult
    {
        public List<int> Rows { get; } = new();
        public List<int> Columns { get; } = new();
        public HashSet<BoardCoord> Cells { get; } = new();
        public int LineCount => Rows.Count + Columns.Count;
    }
}
