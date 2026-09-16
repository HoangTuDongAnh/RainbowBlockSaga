using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Gameplay.Managers;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PreFailedTitleManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;

        private void OnEnable()
        {
            UpdateTitleText();
        }

        private void UpdateTitleText()
        {
            var level = GameDataManager.GetLevel();
            if (!level.enableTimer)
            {
                titleText.text = "REVIVE WITH NEW SHAPES";
            }
            else
            {
                // For timer-enabled levels, check remaining time
                var timerManager = FindObjectOfType<TimerManager>();
                if (timerManager != null && timerManager.RemainingTime > 0)
                {
                    titleText.text = "REVIVE WITH NEW SHAPES";
                }
                else
                {
                    titleText.text = "CONTINUE WITH EXTRA SECONDS";
                }
            }
        }
    }
}
