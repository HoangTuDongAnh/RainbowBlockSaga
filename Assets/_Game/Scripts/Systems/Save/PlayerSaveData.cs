using System;

namespace RainbowBlockSaga.Systems.Save
{
    [Serializable]
    public class PlayerSaveData
    {
        public int HighestAdventureLevel = 1;
        public int EndlessHighScore;
        public int Coins;
    }
}
