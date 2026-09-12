using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Objectives
{
    [CreateAssetMenu(fileName = "ScoreObjective", menuName = "Rainbow Block Saga/Objective/Score")]
    public class ScoreObjectiveData : ObjectiveData
    {
        [Min(1)] public int TargetScore = 300;
        public override IObjective CreateRuntime() => new ScoreObjective(TargetScore);
    }
}
