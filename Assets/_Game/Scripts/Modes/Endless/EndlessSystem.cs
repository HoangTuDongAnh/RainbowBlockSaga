using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Systems.Save;
using RainbowBlockSaga.Foundation;
using UnityEngine;

namespace RainbowBlockSaga.Modes.Endless
{
    public class EndlessSystem : Singleton<EndlessSystem>
    {
        [SerializeField] EndlessModeData data;
        public GameSessionConfig CreateSessionConfig() => new()
        {
            Board = data.Board,
            Spawn = data.SpawnProfile,
            ScoreRule = data.ScoreRule,
            Objective = null
        };

        public void Complete(GameSessionResult result)
        {
            if (result.Score <= SaveSystem.Ins.Data.EndlessHighScore) return;
            SaveSystem.Ins.Data.EndlessHighScore = result.Score;
            SaveSystem.Ins.Save();
        }
    }
}
