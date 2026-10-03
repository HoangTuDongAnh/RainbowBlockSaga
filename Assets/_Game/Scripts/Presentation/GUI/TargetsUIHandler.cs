using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetsUIHandler : MonoBehaviour
    {
        public GameObject ScoreLabel;
        public GameObject TargetsLabel;
        public GameObject EndlessModeLabel;
        public GameObject TimedModeLabel;

        public void OnLevelLoaded(ELevelType levelType)
        {
            // Activate only the selected panel last, so it owns target registrations.
            ScoreLabel.SetActive(false);
            TargetsLabel.SetActive(false);
            EndlessModeLabel.SetActive(false);
            TimedModeLabel.SetActive(false);
            // Activate only the selected panel last, so it owns target registrations.
            ScoreLabel.SetActive(false);
            TargetsLabel.SetActive(false);
            EndlessModeLabel.SetActive(false);
            TimedModeLabel.SetActive(false);
            ScoreLabel.SetActive(levelType == ELevelType.Score);
            TargetsLabel.SetActive(levelType == ELevelType.CollectItems);
            EndlessModeLabel.SetActive(levelType == ELevelType.Endless && GameDataManager.GetGameMode() == EGameMode.Endless);
            TimedModeLabel.SetActive(levelType == ELevelType.Endless && GameDataManager.GetGameMode() == EGameMode.Timed);
        }
    }
}
