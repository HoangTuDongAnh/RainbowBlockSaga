using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    [Serializable]
    public class LevelRow
    {
        public ItemTemplate[] cells;
        public bool[] bonusItems; // bonus item with diamonds
        public bool[] disabled; // disabled collider
        public bool[] highlighted; // highlighted cell for tutorial

        public LevelRow(int size)
        {
            cells = new ItemTemplate[size];
            bonusItems = new bool[size];
            disabled = new bool[size];
            highlighted = new bool[size];
        }
    }

    [CreateAssetMenu(fileName = "Level", menuName = "Rainbow Blocks Saga/Levels/Level", order = 1)]
    public class Level : ScriptableObject
    {
        public int rows = 8;
        public int columns = 8;
        public LevelRow[] levelRows;
        public LevelTypeScriptable levelType;
        public bool enableTimer = false;
        public int timerDuration = 120;
        [Min(0), Tooltip("0 = unlimited. Only successful placements consume a move.")]
        public int moveLimit;

        [SerializeField]
        public Dictionary<Color, int> bonusItemColors;

        [SerializeField]
        public List<Target> targetInstance = new();

        public float emptyCellPercentage = 10f;

        public int Number => GetLevelNum();


        private void OnEnable()
        {
            InitializeIfNeeded();
        }

        public void InitializeIfNeeded()
        {
            rows = Mathf.Max(1, rows);
            columns = Mathf.Max(1, columns);
            if (levelRows == null || levelRows.Length != rows || levelRows.Any(row =>
                row == null || row.cells == null || row.cells.Length != columns ||
                row.bonusItems == null || row.bonusItems.Length != columns ||
                row.disabled == null || row.disabled.Length != columns ||
                row.highlighted == null || row.highlighted.Length != columns))
            {
                Resize(rows, columns);
            }
        }

        public ItemTemplate GetItem(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                return levelRows[row].cells[column];
            }

            return null;
        }

        public void SetBonus(int row, int column, bool bonus)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                levelRows[row].bonusItems[column] = bonus;
            }
        }

        public void Resize(int newRows, int newColumns)
        {
            newRows = Mathf.Max(1, newRows);
            newColumns = Mathf.Max(1, newColumns);
            var newLevelRows = new LevelRow[newRows];
            for (var i = 0; i < newRows; i++)
            {
                newLevelRows[i] = new LevelRow(newColumns);
                if (levelRows == null || i >= levelRows.Length || levelRows[i] == null)
                    continue;
                CopyRow(levelRows[i].cells, newLevelRows[i].cells);
                CopyRow(levelRows[i].bonusItems, newLevelRows[i].bonusItems);
                CopyRow(levelRows[i].disabled, newLevelRows[i].disabled);
                CopyRow(levelRows[i].highlighted, newLevelRows[i].highlighted);
            }

            rows = newRows;
            columns = newColumns;
            levelRows = newLevelRows;
        }

        private static void CopyRow<T>(T[] source, T[] destination)
        {
            if (source != null)
                Array.Copy(source, destination, Math.Min(source.Length, destination.Length));
        }

        private int GetLevelNum()
        {
            var levelName = name;
            var numericPart = new string(levelName.Where(char.IsDigit).ToArray());

            if (int.TryParse(numericPart, out var levelNum))
            {
                return levelNum;
            }

            Debug.LogWarning("Unable to parse the numeric part from the level name.");
            return -1;
        }

        public bool GetBonus(int row, int col)
        {
            if (row >= 0 && row < rows && col >= 0 && col < columns)
            {
                return levelRows[row].bonusItems[col];
            }

            return false;
        }

        public void UpdateTargets()
        {
            targetInstance.Clear();
            if (levelType == null || levelType.targets == null)
                return;
            foreach (var targetScriptable in levelType.targets)
            {
                if (targetScriptable != null)
                    targetInstance.Add(new Target(targetScriptable));
            }
        }

        public void SetItem(int row, int column, ItemTemplate item)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                levelRows[row].cells[column] = item;
            }

            if (item == null)
            {
                SetBonus(row, column, false);
            }
        }

        public bool IsDisabled(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].disabled == null || column >= levelRows[row].disabled.Length)
                {
                    return false;
                }

                return levelRows[row].disabled[column];
            }

            return false;
        }

        public void DisableCellToggle(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].disabled == null || column >= levelRows[row].disabled.Length)
                {
                    levelRows[row].disabled = new bool[columns];
                }

                levelRows[row].disabled[column] = !levelRows[row].disabled[column];
            }
        }

        //for tutorial
        public void HighlightCellToggle(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].highlighted == null || column >= levelRows[row].highlighted.Length)
                {
                    levelRows[row].highlighted = new bool[columns];
                }

                levelRows[row].highlighted[column] = !levelRows[row].highlighted[column];
            }
        }

        public bool IsCellHighlighted(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].highlighted == null || column >= levelRows[row].highlighted.Length)
                {
                    return false;
                }

                return levelRows[row].highlighted[column];
            }

            return false;
        }
    }
}
