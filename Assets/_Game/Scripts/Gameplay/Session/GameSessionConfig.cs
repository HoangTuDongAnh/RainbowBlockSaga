using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Score;
using RainbowBlockSaga.Gameplay.Spawn;
using RainbowBlockSaga.Gameplay.Objectives;

namespace RainbowBlockSaga.Gameplay.Session
{
    public class GameSessionConfig
    {
        public BoardData Board;
        public SpawnProfileData Spawn;
        public ScoreRuleData ScoreRule;
        public ObjectiveData Objective;
        public int AdventureLevel = 1;
    }
}
