using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using UnityEngine;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Maps ShapeTemplate presentation assets to runtime BlockShapeData and back.
    /// </summary>
    public static class ShapeDataAdapter
    {
        static readonly Dictionary<ShapeTemplate, BlockShapeData> Cache = new();
        static readonly Dictionary<BlockShapeData, ShapeTemplate> ReverseCache = new();

        public static BlockShapeData GetOrCreate(ShapeTemplate template)
        {
            if (template == null)
                return null;

            if (Cache.TryGetValue(template, out var data) && data)
                return data;

            data = ScriptableObject.CreateInstance<BlockShapeData>();
            data.name = $"Runtime_{template.name}";
            data.Cells = BuildCells(template);
            data.SpawnWeight = Mathf.Max(0.0001f, template.chanceForSpawn);
            data.MinAdventureLevel = Mathf.Max(1, template.spawnFromLevel);

            Cache[template] = data;
            ReverseCache[data] = template;
            return data;
        }

        public static ShapeTemplate GetTemplate(BlockShapeData data)
        {
            if (data != null && ReverseCache.TryGetValue(data, out var template))
                return template;

            return null;
        }

        static List<BoardCoord> BuildCells(ShapeTemplate template)
        {
            int minColumn = int.MaxValue;
            int maxRow = int.MinValue;

            for (int row = 0; row < template.rows.Length; row++)
            {
                var cells = template.rows[row].cells;
                for (int column = 0; column < cells.Length; column++)
                {
                    if (!cells[column])
                        continue;

                    minColumn = Mathf.Min(minColumn, column);
                    maxRow = Mathf.Max(maxRow, row);
                }
            }

            var result = new List<BoardCoord>();
            if (minColumn == int.MaxValue)
                return result;

            for (int row = 0; row < template.rows.Length; row++)
            {
                var cells = template.rows[row].cells;
                for (int column = 0; column < cells.Length; column++)
                {
                    if (!cells[column])
                        continue;

                    result.Add(new BoardCoord(column - minColumn, maxRow - row));
                }
            }

            return result;
        }
    }
}
