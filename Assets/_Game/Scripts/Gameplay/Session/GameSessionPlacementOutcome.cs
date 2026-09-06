using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;

namespace RainbowBlockSaga.Gameplay.Session
{
    public class GameSessionPlacementOutcome
    {
        public PlacementResult Placement { get; }
        public BoardResolveResult Resolve { get; }
        public int ScoreGain { get; }

        public GameSessionPlacementOutcome(
            PlacementResult placement,
            BoardResolveResult resolve,
            int scoreGain)
        {
            Placement = placement;
            Resolve = resolve;
            ScoreGain = scoreGain;
        }
    }
}
