using System;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Placement;
using RainbowBlockSaga.Gameplay.Resolve;
using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Session
{
    public class GameplayController : MonoBehaviour
    {
        [SerializeField] BoardView boardView;
        [SerializeField] BlockQueueView blockQueueView;

        public GameSession Session { get; private set; }
        public event Action<GameSession> SessionStarted;
        public event Action<GameSessionResult> SessionEnded;

        public void Initialize(GameSessionConfig config)
        {
            UnbindSession();
            Session = new GameSession(config);
            boardView.Initialize(Session.Board);
            Session.Resolved += OnResolved;
            Session.Ended += OnEnded;
            Session.Start();
            blockQueueView.Initialize(this);
            boardView.Refresh();
            SessionStarted?.Invoke(Session);
        }

        public bool TryPlace(BlockShapeData shape, BoardCoord anchor)
        {
            bool placed = Session.TryPlace(shape, anchor);
            if (placed) boardView.Refresh();
            return placed;
        }

        public bool CanPlace(BlockShapeData shape, BoardCoord anchor) => Session.Placement.CanPlace(Session.Board, shape, anchor);
        public BoardView BoardView => boardView;

        void OnResolved(PlacementResult placement, BoardResolveResult resolve, int gain) => boardView.Refresh();
        void OnEnded(GameSessionResult result) => SessionEnded?.Invoke(result);

        void UnbindSession()
        {
            if (Session == null) return;
            Session.Resolved -= OnResolved;
            Session.Ended -= OnEnded;
        }

        void OnDestroy() => UnbindSession();
    }
}
