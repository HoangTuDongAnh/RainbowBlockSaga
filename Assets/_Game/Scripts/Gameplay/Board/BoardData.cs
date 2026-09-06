using System.Collections.Generic;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Board
{
    [CreateAssetMenu(fileName = "BoardData", menuName = "Rainbow Block Saga/Board/Board Data")]
    public class BoardData : BaseData
    {
        [Min(1)] public int Width = 8;
        [Min(1)] public int Height = 8;
        [Tooltip("Empty means every coordinate inside Width x Height is playable.")]
        public List<BoardCoord> PlayableCells = new();
        public BoardViewProfile ViewProfile;

        public bool IsPlayable(BoardCoord coord)
        {
            if (coord.X < 0 || coord.X >= Width || coord.Y < 0 || coord.Y >= Height) return false;
            return PlayableCells.Count == 0 || PlayableCells.Contains(coord);
        }
    }
}
