using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Gameplay.Spawn;
using UnityEngine;

namespace RainbowBlockSaga.Modes.Endless
{
    [CreateAssetMenu(fileName = "EndlessMode", menuName = "Rainbow Block Saga/Endless/Mode")]
    public class EndlessModeData : BaseData
    {
        public BoardData Board;
        public SpawnProfileData SpawnProfile;
        public ScoreRuleData ScoreRule;
    }
}
