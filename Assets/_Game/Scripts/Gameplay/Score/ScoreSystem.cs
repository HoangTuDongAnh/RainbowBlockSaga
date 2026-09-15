using System;
using RainbowBlockSaga.Presentation.Contracts;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Score
{
    public class ScoreSystem
    {
        readonly ScoreRuleData rule;

        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int Misses { get; private set; }
        public ResolveScoreFeedback LastTurn { get; private set; }

        public event Action<int> Changed;
        public event Action<int> ComboChanged;

        public ScoreSystem(ScoreRuleData rule)
        {
            this.rule = rule;
        }

        public int Apply(PlacementResult placement, BoardResolveResult resolve)
        {
            if (placement == null || !placement.Success || resolve == null) return 0;
            int block = placement.Cells.Count * rule.PlacementScorePerCell;
            int line = resolve.ClearedCells.Count * rule.ClearScorePerCell;
            float multiplier = 1f;
            bool rainbow = false;
            if (resolve.ClearedLines > 0)
            {
                Misses = 0;
                Combo++;
                ComboChanged?.Invoke(Combo);
                multiplier = rule.IsEndless ? rule.Endless.Multiplier(Combo) : rule.UseComboStreak
                    ? Mathf.Max(1, Combo) : 1f + (resolve.ClearedLines - 1) * rule.AdditionalLineMultiplier;
                rainbow = rule.IsEndless && (Combo == Mathf.Max(1, rule.Endless.RainbowCombo) || resolve.IsFullClear);
            }
            else if (rule.IsEndless || rule.UseComboStreak)
            {
                Misses++;
                var limit = rule.IsEndless ? rule.Endless.ResetAfterMisses : rule.ResetComboAfterMisses;
                if (Misses >= Mathf.Max(1, limit))
                {
                    Misses = 0;
                    if (Combo != 0) { Combo = 0; ComboChanged?.Invoke(Combo); }
                }
            }
            LastTurn = new ResolveScoreFeedback(rule.IsEndless, block, line, multiplier,
                rainbow ? Mathf.Max(0, rule.Endless.RainbowBonus) : 0, Combo, resolve.IsFullClear, rainbow);
            int gain = LastTurn.Total;
            Score += gain;
            Changed?.Invoke(Score);
            return gain;
        }

        public void SetScore(int value)
        {
            Score = Mathf.Max(0, value);
            Changed?.Invoke(Score);
        }

        public void Reset()
        {
            LastTurn = default;
            Score = 0;
            Combo = 0;
            Misses = 0;

            Changed?.Invoke(Score);
            ComboChanged?.Invoke(Combo);
        }
    }
}
