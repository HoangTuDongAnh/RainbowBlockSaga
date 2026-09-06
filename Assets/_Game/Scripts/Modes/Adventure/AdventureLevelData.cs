using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Gameplay.Spawn;
using RainbowBlockSaga.Modes.Objectives;
using UnityEngine;

namespace RainbowBlockSaga.Modes.Adventure
{
    [CreateAssetMenu(fileName = "AdventureLevel", menuName = "Rainbow Block Saga/Adventure/Level")]
    public class AdventureLevelData : BaseData
    {
        [Min(1)] public int LevelNumber = 1;
        public BoardData Board;
        public SpawnProfileData SpawnProfile;
        public ScoreRuleData ScoreRule;
        public ObjectiveData Objective;
        [Min(0)] public int Reward = 0;
    }
}
