namespace RainbowBlockSaga.Gameplay.Session
{
    public enum GameSessionEndReason
    {
        ObjectiveCompleted,
        NoValidMoves,
        Cancelled
    }

    public class GameSessionResult
    {
        public GameSessionEndReason Reason { get; }
        public int Score { get; }
        public bool Success => Reason == GameSessionEndReason.ObjectiveCompleted;

        public GameSessionResult(GameSessionEndReason reason, int score)
        {
            Reason = reason;
            Score = score;
        }
    }
}
