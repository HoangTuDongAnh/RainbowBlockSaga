using RainbowBlockSaga.Presentation.Scripts.Settings;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups.Daily
{
    public class DayHandle : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI dayText;

        [SerializeField]
        private TextMeshProUGUI coinsCountText;

        [SerializeField]
        private GameObject sparklePrefab;

        public EDailyStatus DailyStatus { get; private set; }

        public RewardSetting RewardData { get; set; }

        public void SetDay(int day, RewardSetting rewardSetting)
        {
            dayText.text = dayText.text + " " + day;
            coinsCountText.text = rewardSetting.count.ToString();
            RewardData = rewardSetting;
        }

        public void SetStatus(EDailyStatus eDailyStatus)
        {
            DailyStatus = eDailyStatus;

            var list = GetComponentsInChildren<DayToggle>();
            foreach (var dayToggle in list)
            {
                dayToggle.SetStatus(eDailyStatus);
            }
        }
    }
}