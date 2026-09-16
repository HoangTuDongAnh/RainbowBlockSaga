using RainbowBlockSaga.Presentation.Scripts.LevelsData;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay.Managers
{
    public interface ILevelLoadable
    {
        void OnLevelLoaded(Level level);
    }

    public interface IBeforeLevelLoadable
    {
        void OnLevelLoaded(Level level);
    }
}