using UnityEngine;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Popups;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    // [CreateAssetMenu(fileName = "ClassicStateHandler", menuName = "Rainbow Blocks Saga/Levels/ClassicStateHandler")]
    public class ClassicLevelStateHandler : LevelStateHandler
    {
        private protected override void HandlePreFailed(LevelManager levelManager)
        {
            var level = levelManager.GetCurrentLevel();
            var preFailedPopup = level.levelType.preFailedPopup;
            levelManager.timerManager?.StopTimer();

            levelManager.StartCoroutine(levelManager.EndAnimations(() =>
            {
                if (preFailedPopup != null && GameManager.instance.GameSettings.enablePreFailedPopup)
                {
                    MenuManager.instance.ShowPopup(preFailedPopup, levelManager.ClearEmptyCells, result =>
                    {
                        if (result == EPopupResult.Continue)
                        {
                            levelManager.cellDeck.UpdateCellDeckAfterFail();
                            EventManager.GameStatus = EGameState.Playing;
                        }
                    });
                }
                else
                {
                    EventManager.GameStatus = EGameState.Failed;
                }
            }));
        }
    }
}