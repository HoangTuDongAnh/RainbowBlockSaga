using System.Collections.Generic;
using UnityEngine;

namespace RainbowBlockSaga.Modes.Adventure
{
    [CreateAssetMenu(fileName = "AdventureData", menuName = "Rainbow Block Saga/Adventure/Data")]
    public class AdventureData : BaseData
    {
        public List<AdventureLevelData> Levels = new();

        public AdventureLevelData GetLevel(int levelNumber)
        {
            foreach (var level in Levels)
                if (level && level.LevelNumber == levelNumber)
                    return level;
            return null;
        }
    }
}
