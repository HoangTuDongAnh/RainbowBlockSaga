using RainbowBlockSaga.Presentation.Scripts.Enums;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public partial class LevelManager
    {
        private void HandleGameStateChange(EGameState newState)
        {
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
