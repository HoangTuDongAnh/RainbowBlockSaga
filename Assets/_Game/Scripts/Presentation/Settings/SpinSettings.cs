using System;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.Popups.Reward;

namespace RainbowBlockSaga.Presentation.Scripts.Settings
{
    public class SpinSettings : SettingsBase
    {
        public int costToSpin = 10;
        public RewardSettingSpin[] rewards = Array.Empty<RewardSettingSpin>();
    }

    [Serializable]
    public class RewardSettingSpin
    {
        public ResourceObject resource;
        public RewardVisual rewardVisualPrefab;
        public int count;
        public RewardPopup rewardPopupPrefab;
    }
}