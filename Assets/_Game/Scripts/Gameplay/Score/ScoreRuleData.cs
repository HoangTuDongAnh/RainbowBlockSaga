using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Gameplay.Score
{
    [CreateAssetMenu(fileName = "ScoreRule", menuName = "Rainbow Block Saga/Score/Rule")]
    public class ScoreRuleData : ScriptableObject
    {
        [Min(0)] public int PlacementScorePerCell = 10;
        [FormerlySerializedAs("BaseLineScore")]
        [Min(0)] public int ClearScorePerCell = 10;
        [Min(0f)] public float AdditionalLineMultiplier = .5f;

        [Header("Combo")]
        public bool UseComboStreak;
        [Min(1)] public int ResetComboAfterMisses = 3;
    }
}
