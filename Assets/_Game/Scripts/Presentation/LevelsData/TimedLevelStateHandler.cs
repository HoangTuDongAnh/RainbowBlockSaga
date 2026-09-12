using UnityEngine;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Popups;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    // [CreateAssetMenu(fileName = "TimedStateHandler", menuName = "Rainbow Blocks Saga/Levels/TimedStateHandler")]
    public class TimedLevelStateHandler : LevelStateHandler
    {
        private protected override void HandlePreFailed(LevelManager levelManager)
        {
            var level = levelManager.GetCurrentLevel();
            var preFailedPopup = level.levelType.preFailedPopup;
            levelManager.timerManager?.StopTimer();

            levelManager.StartCoroutine(levelManager.EndAnimations(() =>
            {
                if (levelManager.timerManager != null && levelManager.timerManager.RemainingTime <= 0)
                {
                    EventManager.GameStatus = EGameState.Failed;
                    return;
                }

                if (preFailedPopup != null && GameManager.instance.GameSettings.enablePreFailedPopup)
                {
                    MenuManager.instance.ShowPopup(preFailedPopup, levelManager.ClearEmptyCells, result =>
                    {
                        if (result == EPopupResult.Continue)
                        {
                            levelManager.cellDeck.UpdateCellDeckAfterFail();
                            levelManager.timerManager?.InitializeTimer(levelManager.timerManager.RemainingTime );
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

        private protected override void HandlePreWin(LevelManager levelManager)
        {
            levelManager.timerManager?.StopTimer();
            base.HandlePreWin(levelManager);
        }
    }
}