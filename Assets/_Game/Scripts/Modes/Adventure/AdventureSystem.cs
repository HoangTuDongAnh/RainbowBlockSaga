using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Systems.Save;
using RainbowBlockSaga.Foundation;
using UnityEngine;

namespace RainbowBlockSaga.Modes.Adventure
{
    public class AdventureSystem : Singleton<AdventureSystem>
    {
        [SerializeField] AdventureData data;
        public AdventureData Data => data;

        public void InitializeData(AdventureData adventureData)
        {
            data = adventureData;
        }

        public bool IsUnlocked(int levelNumber) => levelNumber <= SaveSystem.Ins.Data.HighestAdventureLevel;
        public AdventureLevelData GetLevel(int levelNumber) => data.GetLevel(levelNumber);

        public GameSessionConfig CreateSessionConfig(AdventureLevelData level)
        {
            return new GameSessionConfig
            {
                Board = level.Board,
                Spawn = level.SpawnProfile,
                ScoreRule = level.ScoreRule,
                Objective = level.Objective,
                AdventureLevel = level.LevelNumber
            };
        }

        public void Complete(AdventureLevelData level, GameSessionResult result)
        {
            if (!result.Success)
                return;

            bool firstClear =
                level.LevelNumber >= SaveSystem.Ins.Data.HighestAdventureLevel;

            SaveSystem.Ins.Data.HighestAdventureLevel =
                Mathf.Max(
                    SaveSystem.Ins.Data.HighestAdventureLevel,
                    level.LevelNumber + 1);

            if (firstClear)
                SaveSystem.Ins.Data.Coins += level.Reward;

            SaveSystem.Ins.Save();
        }
    }
}
