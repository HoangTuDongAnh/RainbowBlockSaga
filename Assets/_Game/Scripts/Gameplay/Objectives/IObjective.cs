using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;

namespace RainbowBlockSaga.Gameplay.Objectives
{
    public interface IObjective
    {
        bool IsCompleted { get; }
        float Progress01 { get; }
        void Reset();
        void OnGameplayResolved(int score, PlacementResult placement, BoardResolveResult resolve);
    }
}
