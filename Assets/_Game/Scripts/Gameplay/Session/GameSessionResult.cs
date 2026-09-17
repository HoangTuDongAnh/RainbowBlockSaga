namespace RainbowBlockSaga.Gameplay.Session
{
    public enum GameSessionEndReason
    {
        NoValidMoves,
        Cancelled
    }

    public class GameSessionResult
    {
        public GameSessionEndReason Reason { get; }
        public int Score { get; }
        public GameSessionResult(GameSessionEndReason reason, int score)
        {
            Reason = reason;
            Score = score;
        }
    }
}
