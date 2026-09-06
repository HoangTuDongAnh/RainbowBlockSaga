using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Block
{
    [CreateAssetMenu(fileName = "BlockShape", menuName = "Rainbow Block Saga/Block/Shape")]
    public class BlockShapeData : BaseData
    {
        public List<BoardCoord> Cells = new() { new BoardCoord(0, 0) };
        [Min(0f)] public float SpawnWeight = 1f;
        [Min(1)] public int MinAdventureLevel = 1;

        public IReadOnlyList<BoardCoord> GetNormalizedCells()
        {
            if (Cells.Count == 0) return Cells;
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            foreach (var cell in Cells)
            {
                if (cell.X < minX) minX = cell.X;
                if (cell.Y < minY) minY = cell.Y;
            }

            var result = new List<BoardCoord>(Cells.Count);
            foreach (var cell in Cells) result.Add(new BoardCoord(cell.X - minX, cell.Y - minY));
            return result;
        }
    }
}
