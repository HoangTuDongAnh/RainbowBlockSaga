using UnityEngine;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Popups;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    // [CreateAssetMenu(fileName = "AdventureStateHandler", menuName = "Rainbow Blocks Saga/Levels/AdventureStateHandler")]
    public class AdventureLevelStateHandler : LevelStateHandler
    {
        private protected override void HandlePreFailed(LevelManager levelManager)
        {
            Debug.Log("Adventure mode: Game is about to end...");
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
                            if (level.enableTimer)
                            {
                                float newTime = levelManager.timerManager.RemainingTime > 0 ?
                                    levelManager.timerManager.RemainingTime :
                                    GameManager.instance.GameSettings.continueTimerBonus;
                                levelManager.timerManager.InitializeTimer(newTime);
                            }
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