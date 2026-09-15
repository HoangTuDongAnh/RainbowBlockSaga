using System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Contracts
{
    [Serializable]
    public class EndlessScoringSettings
    {
        [Tooltip("Last value is used for all higher combos.")]
        public float[] ComboMultipliers = { 1f, 1.1f, 1.2f, 1.3f, 1.5f };
        [Tooltip("Consecutive placements without a clear before resetting the streak.")]
        [Min(1)] public int ResetAfterMisses = 1;
        [Min(1)] public int SmallFeedbackCombo = 3;
        [Min(1)] public int StrongFeedbackCombo = 4;
        [Tooltip("Award once when a streak reaches this combo. Full clear can also trigger the bonus; never twice in one turn.")]
        [Min(1)] public int RainbowCombo = 5;
        [Min(0)] public int RainbowBonus = 100;
        public AudioClip RainbowSound;
        [Range(0f, .3f)] public float FlashOpacity = .18f;

        public float Multiplier(int combo) => ComboMultipliers == null || ComboMultipliers.Length == 0
            ? 1f : Mathf.Max(1f, ComboMultipliers[Mathf.Clamp(combo - 1, 0, ComboMultipliers.Length - 1)]);
    }

    public readonly struct ResolveScoreFeedback
    {
        public readonly bool IsEndless;
        public readonly int BlockScore, LineScore, RainbowBonus, Combo;
        public readonly float Multiplier;
        public readonly bool FullClear, RainbowTriggered;
        public int Total => BlockScore + Mathf.RoundToInt(LineScore * Multiplier) + RainbowBonus;
        public ResolveScoreFeedback(bool endless, int block, int line, float multiplier, int bonus, int combo, bool fullClear, bool rainbow)
        {
            IsEndless = endless; BlockScore = block; LineScore = line; Multiplier = multiplier;
            RainbowBonus = bonus; Combo = combo; FullClear = fullClear; RainbowTriggered = rainbow;
        }
    }
}
