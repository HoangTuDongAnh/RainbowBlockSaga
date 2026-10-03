using System;
using System.Linq;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    [Serializable]
    public sealed class RunSnapshot
    {
        public int version = 1;
        public string runId;
        public EGameMode mode;
        public int level, rows, columns, score, combo, misses, highestCombo, movesUsed;
        public float remainingTime;
        public SavedCell[] cells;
        public SavedShape[] tray;
        public SavedTarget[] targets;
        [Serializable] public sealed class SavedCell { public string item, bonus; public bool disabled; }
        [Serializable] public sealed class SavedShape { public string shape; public SavedCell[] items; }
        [Serializable] public sealed class SavedTarget { public string name; public int remaining; }

        public static string Key(EGameMode mode) => "RBS_Run_" + mode;
        public static RunSnapshot Load(EGameMode mode)
        {
            if (GameDataManager.isTestPlay || !PlayerPrefs.HasKey(Key(mode))) return null;
            try
            {
                var state = JsonUtility.FromJson<RunSnapshot>(PlayerPrefs.GetString(Key(mode)));
                return state != null && state.version == 1 && state.mode == mode && state.IsValid() ? state : null;
            }
            catch (Exception ex) { Debug.LogWarning("Cannot restore run: " + ex.Message); return null; }
        }
        public void Save() { PlayerPrefs.SetString(Key(mode), JsonUtility.ToJson(this)); PlayerPrefs.Save(); }
        public static void Delete(EGameMode mode) { PlayerPrefs.DeleteKey(Key(mode)); PlayerPrefs.Save(); }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(runId) || rows < 1 || columns < 1 || cells == null || cells.Length != rows * columns ||
                tray == null || tray.Length != 3 || targets == null || score < 0 || movesUsed < 0 || combo < 0 || misses < 0 || highestCombo < combo) return false;
            var items = Resources.LoadAll<ItemTemplate>("Items").Select(x => x.name).ToHashSet();
            if (targets.Any(t => t == null || string.IsNullOrEmpty(t.name) || t.remaining < 0) ||
                targets.Select(t => t.name).Distinct().Count() != targets.Length ||
                float.IsNaN(remainingTime) || float.IsInfinity(remainingTime) || remainingTime < 0) return false;
            var bonuses = Resources.LoadAll<BonusItemTemplate>("").Select(x => x.name).ToHashSet();
            var shapes = Resources.LoadAll<ShapeTemplate>("Shapes").ToDictionary(x => x.name);
            bool ValidCell(SavedCell c) => c != null && (string.IsNullOrEmpty(c.item) || items.Contains(c.item)) &&
                (string.IsNullOrEmpty(c.bonus) || !string.IsNullOrEmpty(c.item) && bonuses.Contains(c.bonus));
            if (!cells.All(ValidCell)) return false;
            foreach (var slot in tray)
            {
                if (slot == null || string.IsNullOrEmpty(slot.shape)) continue;
                if (!shapes.TryGetValue(slot.shape, out var shape) || slot.items == null ||
                    slot.items.Length != shape.rows.Sum(r => r.cells.Count(c => c)) ||
                    !slot.items.All(c => ValidCell(c) && !string.IsNullOrEmpty(c.item))) return false;
            }
            return true;
        }
        public static SavedCell CaptureItem(Item item) => new SavedCell {
            item = item.itemTemplate.name, bonus = item.HasBonusItem() ? item.bonusItemTemplate.name : ""
        };
    }
}
