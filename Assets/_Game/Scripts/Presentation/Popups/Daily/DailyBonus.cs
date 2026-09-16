using System;
using System.Linq;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.GUI.Labels;
using RainbowBlockSaga.Presentation.Scripts.Settings;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups.Daily
{
    public class DailyBonus : PopupWithCurrencyLabel
    {
        [SerializeField]
        public DayHandle[] dayHandles;

        // Instance of the custom scriptable object that stores daily bonus settings
        private DailyBonusSettings settings;
        private int rewardStreak;

        public DayHandle[] daysPrefabs;

        public Transform dayHandlesParent;
        private int days;

        // This method is automatically called when the script becomes enabled
        public void OnEnable()
        {
            // Load daily bonus settings
            settings = LoadSettings("Settings/DailyBonusSettings");

            days = settings.rewards.Length;

            // Update the reward streak count and store it
            rewardStreak = UpdateRewardStreak();

            // Update each day handle based on the current reward streak
            UpdateDayHandles(rewardStreak);
        }

        // Loads and returns daily bonus settings stored at the specified path
        public DailyBonusSettings LoadSettings(string path)
        {
            return Resources.Load<DailyBonusSettings>(path);
        }

        // Checks the last reward date and the current date 
        // to determine and update the reward streak
        public int UpdateRewardStreak()
        {
            var today = DateTime.Today;
            var lastRewardDate = DateTime.Parse(PlayerPrefs.GetString("DailyBonusDay", today.Subtract(TimeSpan.FromDays(1)).ToString()));

            if (today > lastRewardDate)
            {
                var rewardStreak = GetRewardStreak() + 1;
                PlayerPrefs.SetString("DailyBonusDay", today.ToString());
                PlayerPrefs.SetInt("RewardStreak", rewardStreak = (int)Mathf.Repeat(rewardStreak, dayHandles.Length));
                return rewardStreak;
            }

            return GetRewardStreak();
        }

        // Updates the status of each day handle in the scene 
        // according to the current reward streak
        public void UpdateDayHandles(int rewardStreak)
        {
            dayHandles = new DayHandle[days];
            for (var i = 0; i < days; i++)
            {
                var status = i < rewardStreak ? EDailyStatus.passed : i == rewardStreak ? EDailyStatus.current : EDailyStatus.locked;
                var dayHandle = Instantiate(daysPrefabs[(int)status], dayHandlesParent);
                dayHandles[i] = dayHandle;
                dayHandle.SetDay(i + 1, settings.rewards[i]);

                dayHandle.SetStatus(status);
            }
        }

        // Gets and returns the reward streak count from player preferences 
        public int GetRewardStreak()
        {
            return PlayerPrefs.GetInt("RewardStreak", -1);
        }

        public override void Close()
        {
            StopInteration();
            closeButton.interactable = false;
            var resource = dayHandles[rewardStreak].RewardData.resource;

            var dayHandle = dayHandles.First(i => i.DailyStatus == EDailyStatus.current);
            LabelAnim.AnimateForResource(resource, dayHandle.transform.position, "+" + dayHandle.RewardData.count, resource.sound, () => CloseDaily(resource));
        }

        private void CloseDaily(ResourceObject resource)
        {
            resource.Add(dayHandles[rewardStreak].RewardData.count);
            base.Close();
        }
    }
}