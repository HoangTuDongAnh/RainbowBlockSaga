using System.Collections.Generic;

namespace RainbowBlockSaga.Gameplay.Board
{
    public class BoardModel
    {
        readonly BoardData data;
        readonly Dictionary<BoardCoord, BoardCellState> cells = new();

        public int Width => data.Width;
        public int Height => data.Height;
        public BoardData Data => data;

        public BoardModel(BoardData data)
        {
            this.data = data;
            Build();
        }

        void Build()
        {
            cells.Clear();
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    var coord = new BoardCoord(x, y);
                    cells[coord] = data.IsPlayable(coord) ? BoardCellState.Empty : BoardCellState.Disabled;
                }
            }
        }

        public bool Contains(BoardCoord coord) => cells.ContainsKey(coord);
        public bool IsPlayable(BoardCoord coord) => cells.TryGetValue(coord, out var state) && state != BoardCellState.Disabled;
        public bool IsEmpty(BoardCoord coord) => cells.TryGetValue(coord, out var state) && state == BoardCellState.Empty;
        public bool IsOccupied(BoardCoord coord) => cells.TryGetValue(coord, out var state) && state == BoardCellState.Occupied;
        public BoardCellState GetState(BoardCoord coord) => cells[coord];

        public void SetOccupied(BoardCoord coord) => cells[coord] = BoardCellState.Occupied;
        public void SetEmpty(BoardCoord coord)
        {
            if (IsPlayable(coord)) cells[coord] = BoardCellState.Empty;
        }

        public IEnumerable<BoardCoord> GetPlayableCoords()
        {
            foreach (var pair in cells)
                if (pair.Value != BoardCellState.Disabled)
                    yield return pair.Key;
        }

        public IEnumerable<BoardCoord> GetOccupiedCoords()
        {
            foreach (var pair in cells)
                if (pair.Value == BoardCellState.Occupied)
                    yield return pair.Key;
        }

        public void ClearAll()
        {
            var coords = new List<BoardCoord>(cells.Keys);
            foreach (var coord in coords)
                if (cells[coord] != BoardCellState.Disabled)
                    cells[coord] = BoardCellState.Empty;
        }
    }
}
