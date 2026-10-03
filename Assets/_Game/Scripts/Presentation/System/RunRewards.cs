using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    public static class RunRewards
    {
        public static int EndlessCoins(int score) => Mathf.Max(0, score) / 100 * (score < 1000 ? 5 : score <= 5000 ? 7 : 10);
        public static int AdventureCoins(int level) => level > 0 && level % 5 == 0 ? 50 : 10;
        public static string Grant(EGameMode mode, string runId, int level, int score)
        {
            if (GameDataManager.isTestPlay) return "TEST PLAY - NO REWARD";
            string receipt = mode == EGameMode.Adventure ? "RBS_RewardLevel_" + level : "RBS_RewardRun_" + runId;
            if (PlayerPrefs.HasKey(receipt)) return "REWARD ALREADY RECEIVED";
            int coins = mode == EGameMode.Adventure ? AdventureCoins(level) : EndlessCoins(score);
            string boosterText = "";
            if (mode == EGameMode.Adventure && level % 5 == 0 && Random.value < .1f)
            {
                string[] boosters = { "RainbowCell", "Bomb3x3", "ShuffleTray" };
                int index = Random.Range(0, boosters.Length);
                string key = "RBS_EndlessSupport_" + boosters[index];
                PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + 1);
                boosterText = new[] { " + RAINBOW", " + BOMB", " + SHUFFLE" }[index];
            }
            ResourceManager.instance.GetResource("Coins").GrantOnce(receipt, coins);
            return "+" + coins + " COINS" + boosterText;
        }
    }
}
