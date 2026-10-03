using System;
using System.Collections.Generic;
using System.Linq;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public partial class LevelManager
    {
        [SerializeField] private TextMeshProUGUI movesLabel;
        public int RunScore { get; private set; }
        public int RunMisses { get; private set; }
        public int HighestCombo { get; private set; }
        public int MovesUsed { get; private set; }
        public string RewardSummary { get; private set; } = "";
        public bool OutOfMoves => gameMode == EGameMode.Adventure && _levelData.moveLimit > 0 && MovesUsed >= _levelData.moveLimit;
        private string runId;
        private bool runFinished, runReady, savePending;
        private readonly HashSet<Cell> resolvedCells = new();

        public void ResetRunStats()
        {
            RunScore = RunMisses = HighestCombo = MovesUsed = 0;
            comboCounter = 0;
        }
        private void BeginRun()
        {
            ResetRunStats();
            runId = Guid.NewGuid().ToString("N");
            runFinished = false;
            runReady = false;
            RewardSummary = "";
            resolvedCells.Clear();
            UpdateMovesLabel();
        }
        private void UpdateMovesLabel()
        {
            bool visible = gameMode == EGameMode.Adventure && _levelData.moveLimit > 0 && !GameManager.instance.IsTutorialMode();
            movesLabel.gameObject.SetActive(visible);
            if (visible) movesLabel.text = "MOVES " + Mathf.Max(0, _levelData.moveLimit - MovesUsed);
            movesLabel.rectTransform.anchoredPosition = new Vector2(_levelData.enableTimer ? 260 : 0, 50);
            ((RectTransform)timerPanel.transform).anchoredPosition = new Vector2(visible ? -260 : 0, 11);
        }
        private void CommitTurn(Shape shape, List<List<Cell>> lines, int scoreGain, int combo)
        {
            comboCounter = combo;
            HighestCombo = Mathf.Max(HighestCombo, combo);
            RunScore += scoreGain;
            RunMisses = lines != null && lines.Count > 0 ? 0 : RunMisses + 1;
            int reset = IsEndlessMode ? GameManager.instance.GameSettings.endlessScoring.ResetAfterMisses : GameManager.instance.GameSettings.ResetComboAfterMoves;
            if (RunMisses >= Mathf.Max(1, reset)) RunMisses = 0;
            if (gameMode == EGameMode.Adventure && shape != null && !GameManager.instance.IsTutorialMode()) MovesUsed++;
            resolvedCells.Clear();
            if (lines != null) foreach (var cell in lines.SelectMany(x => x)) resolvedCells.Add(cell);
            UpdateMovesLabel();
            savePending = true;
        }
        private void LateUpdate()
        {
            if (savePending && runReady) { savePending = false; SaveRun(); }
        }
        private void OnApplicationQuit() => SaveRun();
        public void SaveRun()
        {
            if (!runReady || runFinished || GameDataManager.isTestPlay || GameManager.instance.IsTutorialMode() ||
                gameMode == EGameMode.Timed || field.cells == null) return;
            var snapshot = new RunSnapshot {
                runId = runId, mode = gameMode, level = currentLevel, rows = field.RowCount, columns = field.ColumnCount,
                score = RunScore, combo = comboCounter, misses = RunMisses, highestCombo = HighestCombo, movesUsed = MovesUsed,
                remainingTime = timerManager.RemainingTime,
                cells = new RunSnapshot.SavedCell[field.RowCount * field.ColumnCount],
                tray = new RunSnapshot.SavedShape[cellDeck.cellDecks.Length],
                targets = targetManager.GetTargets().Select(t => new RunSnapshot.SavedTarget { name = t.targetScriptable.name, remaining = t.amount }).ToArray()
            };
            for (int r = 0; r < field.RowCount; r++) for (int c = 0; c < field.ColumnCount; c++)
            {
                var cell = field.cells[r,c];
                var saved = !cell.IsEmpty() && !resolvedCells.Contains(cell) ? RunSnapshot.CaptureItem(cell.item) : new RunSnapshot.SavedCell();
                saved.disabled = cell.IsDisabled();
                snapshot.cells[r * field.ColumnCount + c] = saved;
            }
            for (int i = 0; i < snapshot.tray.Length; i++)
            {
                var shape = cellDeck.cellDecks[i].shape;
                snapshot.tray[i] = shape == null ? new RunSnapshot.SavedShape() : new RunSnapshot.SavedShape {
                    shape = shape.shapeTemplate.name, items = shape.GetActiveItems().Select(RunSnapshot.CaptureItem).ToArray()
                };
            }
            snapshot.Save();
        }
        private bool RestoreRun()
        {
            if (GameManager.instance.IsTutorialMode() || gameMode == EGameMode.Timed) return false;
            var saved = RunSnapshot.Load(gameMode);
            if (saved == null || saved.level != currentLevel || saved.rows != _levelData.rows || saved.columns != _levelData.columns) return false;
            var targets = targetManager.GetTargets();
            if (saved.targets.Length != targets.Count || saved.targets.Any(s => !targets.Any(t => t.targetScriptable.name == s.name))) return false;
            var items = Resources.LoadAll<ItemTemplate>("Items").ToDictionary(x => x.name);
            var bonuses = Resources.LoadAll<BonusItemTemplate>("").ToDictionary(x => x.name);
            var shapes = Resources.LoadAll<ShapeTemplate>("Shapes").ToDictionary(x => x.name);
            var rows = new LevelRow[saved.rows];
            for (int r=0;r<saved.rows;r++)
            {
                rows[r] = new LevelRow(saved.columns);
                for (int c=0;c<saved.columns;c++)
                {
                    var cell = saved.cells[r*saved.columns+c];
                    rows[r].cells[c] = string.IsNullOrEmpty(cell.item) ? null : items[cell.item];
                    rows[r].disabled[c] = cell.disabled;
                }
            }
            runId = saved.runId;
            RunScore = saved.score; comboCounter = saved.combo; RunMisses = saved.misses; HighestCombo = saved.highestCombo; MovesUsed = saved.movesUsed;
            field.RestoreFromState(rows);
            for(int r=0;r<saved.rows;r++) for(int c=0;c<saved.columns;c++)
            {
                var bonus = saved.cells[r*saved.columns+c].bonus;
                if (!string.IsNullOrEmpty(bonus)) field.cells[r,c].SetBonus(bonuses[bonus]);
            }
            foreach (var target in targets)
            {
                target.amount = Mathf.Clamp(saved.targets.First(t => t.name == target.targetScriptable.name).remaining, 0, target.totalAmount);
                if (target.targetScriptable is ScoreTargetScriptable)
                {
                    if (targetManager.GetTargetGuiElements().TryGetValue(target.targetScriptable, out var view))
                        view.UpdateCount(target.totalAmount - target.amount, target.OnCompleted());
                }
                else targetManager.UpdateTargetCount(target);
            }
            cellDeck.StopAllCoroutines();
            cellDeck.FillCellDecksWithShapes(saved.tray.Select(s => s == null || string.IsNullOrEmpty(s.shape) ? null : shapes[s.shape]).ToArray(), false);
            for (int i=0;i<saved.tray.Length;i++)
            {
                var shape = cellDeck.cellDecks[i].shape;
                if (shape == null) continue;
                var active = shape.GetActiveItems();
                for(int j=0;j<active.Count;j++)
                {
                    var item = saved.tray[i].items[j];
                    active[j].UpdateColor(items[item.item]);
                    if (!string.IsNullOrEmpty(item.bonus)) active[j].SetBonus(bonuses[item.bonus]);
                }
            }
            if (_levelData.enableTimer) timerManager.InitializeTimer(saved.remainingTime);
            UpdateMovesLabel();
            return true;
        }
        private void CompleteRun(bool won)
        {
            if (runFinished || GameManager.instance.IsTutorialMode()) return;
            runFinished = true;
            if (gameMode == EGameMode.Endless || won && gameMode == EGameMode.Adventure)
                RewardSummary = RunRewards.Grant(gameMode, runId, currentLevel, RunScore);
            RunSnapshot.Delete(gameMode);
        }
    }
}
