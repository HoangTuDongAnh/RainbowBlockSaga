using System;
using RainbowBlockSaga.Presentation.Scripts.Data;

namespace RainbowBlockSaga.Presentation.Scripts.Settings
{
    public class DailyBonusSettings : SettingsBase
    {
        public bool dailyBonusEnabled = true;
        public RewardSetting[] rewards = Array.Empty<RewardSetting>();
    }

    [Serializable]
    public class RewardSetting
    {
        public ResourceObject resource;
        public int count;
    }
}