using System.Linq;
using DG.Tweening;
using RainbowBlockSaga.Presentation.Scripts.GUI.Labels;
using RainbowBlockSaga.Presentation.Scripts.Settings;
using TMPro;
using UnityEngine;
using RainbowBlockSaga.Presentation.Scripts.System;

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
        private TextMeshProUGUI thanksLabel;
        private int days;
        private bool claimStarted;

        protected override void Awake()
        {
            base.Awake();
            thanksLabel = closeButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        // This method is automatically called when the script becomes enabled
        public void OnEnable()
        {
            // Load daily bonus settings
            settings = LoadSettings("Settings/DailyBonusSettings");

            days = settings.rewards.Length;
            claimStarted = false;

            // Show the next unclaimed day.
            bool available = RewardClock.CanClaimToday("DailyBonusDay");
            rewardStreak = available ? UpdateRewardStreak() : Mathf.Clamp(GetRewardStreak(), 0, days - 1);
            thanksLabel.text = available ? "CLAIM" : "CLOSE";

            // Update each day handle based on the current reward streak
            UpdateDayHandles(rewardStreak, available);
        }

        // Loads and returns daily bonus settings stored at the specified path
        public DailyBonusSettings LoadSettings(string path)
        {
            return Resources.Load<DailyBonusSettings>(path);
        }

        // Select the next reward; claiming happens when Thanks is pressed.
        public int UpdateRewardStreak()
        {
            return (GetRewardStreak() + 1) % days;
        }

        // Updates the status of each day handle in the scene 
        // according to the current reward streak
        public void UpdateDayHandles(int rewardStreak, bool available = true)
        {
            dayHandles = new DayHandle[days];
            for (var i = 0; i < days; i++)
            {
                var status = EDailyStatus.locked;
                if (i < rewardStreak || (i == rewardStreak && !available))
                    status = EDailyStatus.passed;
                else if (i == rewardStreak)
                    status = EDailyStatus.current;
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
            if (claimStarted) return;
            claimStarted = true;
            if (!dayHandles.Any(day => day.DailyStatus == EDailyStatus.current) ||
                !RewardClock.CanClaimToday("DailyBonusDay"))
            {
                base.Close();
                return;
            }
            StopInteration();
            closeButton.interactable = false;
            var resource = dayHandles[rewardStreak].RewardData.resource;
            var dayHandle = dayHandles.First(i => i.DailyStatus == EDailyStatus.current);
            var amount = dayHandle.RewardData.count;
            resource.Add(amount);
            PlayerPrefs.SetInt("RewardStreak", rewardStreak);
            RewardClock.MarkClaimedToday("DailyBonusDay");

            dayHandle.SetStatus(EDailyStatus.passed);
            thanksLabel.text = "CLAIMED!";
            closeButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 6, 0.5f);
            LabelAnim.AnimateForResource(resource, dayHandle.transform.position, "+" + amount, resource.sound, CloseDaily);
        }

        private void CloseDaily()
        {
            base.Close();
        }
    }
}
