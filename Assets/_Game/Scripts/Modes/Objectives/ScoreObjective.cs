using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using UnityEngine;

namespace RainbowBlockSaga.Modes.Objectives
{
    public class ScoreObjective : IObjective
    {
        readonly int target;
        int score;
        public bool IsCompleted => score >= target;
        public float Progress01 => Mathf.Clamp01((float)score / target);

        public ScoreObjective(int target) => this.target = target;
        public void Reset() => score = 0;
        public void OnGameplayResolved(int currentScore, PlacementResult placement, BoardResolveResult resolve) => score = currentScore;
    }
}
