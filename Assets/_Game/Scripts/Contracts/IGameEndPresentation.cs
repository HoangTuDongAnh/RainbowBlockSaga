namespace RainbowBlockSaga.Presentation.Contracts
{
    /// <summary>
    /// Presentation boundary for the no-valid-moves / lose flow.
    /// </summary>
    public interface IGameEndPresentation
    {
        bool IsEndlessMode { get; }

        void SetRuntimeLifecycleOwnership(bool enabled);
        void PresentNoValidMoves();
    }
}
