using System;

namespace RainbowBlockSaga.Presentation.Contracts
{
    public enum GameplaySessionState
    {
        Unknown,
        Prepare,
        Tutorial,
        Paused,
        Playing,
        PreFailed,
        Failed,
        PreWin,
        Win,
        WinWaiting,
        Loaded
    }

    /// <summary>
    /// Neutral lifecycle/score contract used by the gameplay runtime.
    /// Presentation can keep its own managers and enums behind this boundary.
    /// </summary>
    public interface IGameplaySessionPresentation
    {
        event Action RestartRequested;
        event Action<GameplaySessionState> StateChanged;

        GameplaySessionState CurrentState { get; }

        int CurrentScore { get; }
        int ScorePerLine { get; }
        int ResetComboAfterMoves { get; }

        void ResetCurrentScore();
    }
}
