using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.System;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public partial class LevelManager
    {
        private void HandleGameStateChange(EGameState newState)
        {
            if (newState == EGameState.Failed) CompleteRun(false);
            if (newState == EGameState.Playing && runReady && !GameManager.instance.IsTutorialMode())
                StartCoroutine(CheckLose());
            var currentLevel = GetCurrentLevel();
            if (currentLevel == null || currentLevel.levelType == null)
                return;
            var stateHandler = currentLevel.levelType.stateHandler;
            
            if (stateHandler != null)
            {
                stateHandler.HandleState(newState, this);
            }
        }
    }
}
