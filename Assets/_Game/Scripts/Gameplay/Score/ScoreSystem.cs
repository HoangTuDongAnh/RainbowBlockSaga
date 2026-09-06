using System;
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

        public event Action<int> Changed;
        public event Action<int> ComboChanged;

        public ScoreSystem(ScoreRuleData rule)
        {
            this.rule = rule;
        }

        public int Apply(PlacementResult placement, BoardResolveResult resolve)
        {
            int gain = placement.Cells.Count * rule.PlacementScorePerCell;

            if (resolve.ClearedLines > 0)
            {
                Combo++;
                ComboChanged?.Invoke(Combo);

                if (rule.UseComboStreak)
                {
                    gain += resolve.ClearedLines * rule.BaseLineScore * Mathf.Max(1, Combo);
                }
                else
                {
                    float multiplier =
                        1f + (resolve.ClearedLines - 1) * rule.AdditionalLineMultiplier;

                    gain += Mathf.RoundToInt(
                        resolve.ClearedLines *
                        rule.BaseLineScore *
                        multiplier);
                }
            }
            else if (rule.UseComboStreak)
            {
                Misses++;

                if (Misses >= rule.ResetComboAfterMisses)
                {
                    Misses = 0;

                    if (Combo != 0)
                    {
                        Combo = 0;
                        ComboChanged?.Invoke(Combo);
                    }
                }
            }

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
            Score = 0;
            Combo = 0;
            Misses = 0;

            Changed?.Invoke(Score);
            ComboChanged?.Invoke(Combo);
        }
    }
}
