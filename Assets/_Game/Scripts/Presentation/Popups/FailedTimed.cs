using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.System;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class FailedTimed : FailedClassic
    {
        public TextMeshProUGUI timeText;
        private TimedModeHandler timedModeHandler;

        protected override void OnEnable()
        {
            base.OnEnable();
            timedModeHandler = modeHandler as TimedModeHandler;
            var remainingTime = timedModeHandler.GetRemainingTime();

            bool isTimerFinished = remainingTime <= 0;
            if (!isTimerFinished)
            {
                scoreText[1].text = "0";
                bestScoreStuff.SetActive(false);
                failedStuff.SetActive(true);
            }
        }
    }
}