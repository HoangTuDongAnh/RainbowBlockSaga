using System.Linq;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    // One source for playable Arcade levels; progression and the editor use actual IDs.
    public static class ArcadeLevelCatalog
    {
        public static Level[] LoadAll() => Resources.LoadAll<Level>("Levels")
            .Where(level => level.Number > 0).OrderBy(level => level.Number).ToArray();

        public static Level Find(int number) => LoadAll().FirstOrDefault(level => level.Number == number);
        public static Level Next(int number) => LoadAll().FirstOrDefault(level => level.Number > number);
    }
}
