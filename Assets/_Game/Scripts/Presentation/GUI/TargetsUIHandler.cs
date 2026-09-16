using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetsUIHandler : MonoBehaviour
    {
        public GameObject ScoreLabel;
        public GameObject TargetsLabel;
        public GameObject ClassicModeLabel;
        public GameObject TimedModeLabel;

        public void OnLevelLoaded(ELevelType levelTypeElevelType)
        {
            ScoreLabel.SetActive(levelTypeElevelType == ELevelType.Score);
            TargetsLabel.SetActive(levelTypeElevelType == ELevelType.CollectItems);
            ClassicModeLabel.SetActive(levelTypeElevelType == ELevelType.Classic && GameDataManager.GetGameMode() == EGameMode.Classic);
            TimedModeLabel.SetActive(levelTypeElevelType == ELevelType.Classic && GameDataManager.GetGameMode() == EGameMode.Timed);
        }
    }
}