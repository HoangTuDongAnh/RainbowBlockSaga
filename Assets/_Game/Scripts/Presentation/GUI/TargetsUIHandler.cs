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

        public void OnLevelLoaded(ELevelType levelTypeElevelType)
        {
            ScoreLabel.SetActive(levelTypeElevelType == ELevelType.Score);
            TargetsLabel.SetActive(levelTypeElevelType == ELevelType.CollectItems);
            EndlessModeLabel.SetActive(levelTypeElevelType == ELevelType.Endless && GameDataManager.GetGameMode() == EGameMode.Endless);
            TimedModeLabel.SetActive(levelTypeElevelType == ELevelType.Endless && GameDataManager.GetGameMode() == EGameMode.Timed);
        }
    }
}