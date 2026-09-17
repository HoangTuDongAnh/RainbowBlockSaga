using System;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Owns the Endless no-valid-moves lifecycle.
    /// Popup/end presentation is delegated through IGameEndPresentation.
    /// </summary>
    public class SessionLifecycleRuntime : MonoBehaviour
    {
        [SerializeField] GameSessionRuntime sessionRuntime;

        [SerializeField] ResolveScoreRuntime resolveRuntime;

        [SerializeField] MonoBehaviour endPresentationSource;

        IGameEndPresentation endPresentation;

        GameSession observedSession;
        GameSessionResult pendingResult;
        bool losePresented;

        void Awake()
        {
            endPresentation =
                endPresentationSource as IGameEndPresentation;

            if (endPresentation == null)
                throw new InvalidOperationException(
                    "SessionLifecycleRuntime requires an IGameEndPresentation source.");

            if (resolveRuntime == null)
                throw new InvalidOperationException(
                    "SessionLifecycleRuntime requires a ResolveScoreRuntime reference.");
        }

        void OnEnable()
        {
            sessionRuntime.SessionCreated += OnSessionCreated;
            sessionRuntime.SessionEnded += OnSessionEnded;

            endPresentation?.SetRuntimeLifecycleOwnership(true);

            resolveRuntime.PresentationCompleted += OnPresentationCompleted;

            if (sessionRuntime.Session != null)
                OnSessionCreated(sessionRuntime.Session);
        }

        void OnDisable()
        {
            sessionRuntime.SessionCreated -= OnSessionCreated;
            sessionRuntime.SessionEnded -= OnSessionEnded;

            endPresentation?.SetRuntimeLifecycleOwnership(false);

            resolveRuntime.PresentationCompleted -= OnPresentationCompleted;

            observedSession = null;
            pendingResult = null;
            losePresented = false;
        }

        void Update()
        {
            var session = sessionRuntime.GetOrCreateSession();
            if (session == null)
                return;

            if (observedSession != session)
                OnSessionCreated(session);

            var state =
                sessionRuntime.CurrentPresentationState;

            if (!session.IsEnded &&
                (state == GameplaySessionState.Playing ||
                 state == GameplaySessionState.Tutorial))
            {
                sessionRuntime.SynchronizeQueueFromVisualDecks();
                session.EvaluateEndState();
            }

            if (session.IsEnded &&
                session.Result != null &&
                session.Result.Reason ==
                    GameSessionEndReason.NoValidMoves &&
                !losePresented)
            {
                QueueOrPresent(session.Result);
            }
        }

        void OnSessionCreated(GameSession session)
        {
            observedSession = session;
            pendingResult = null;
            losePresented = false;
        }

        void OnSessionEnded(GameSessionResult result)
        {
            if (result.Reason !=
                GameSessionEndReason.NoValidMoves)
                return;

            QueueOrPresent(result);
        }

        void QueueOrPresent(GameSessionResult result)
        {
            if (losePresented ||
                endPresentation == null ||
                !endPresentation.IsEndlessMode)
                return;

            if (resolveRuntime.IsPresenting)
            {
                pendingResult = result;
                return;
            }

            PresentLose(result);
        }

        void OnPresentationCompleted()
        {
            if (pendingResult == null ||
                losePresented)
                return;

            var result = pendingResult;
            pendingResult = null;

            PresentLose(result);
        }

        void PresentLose(GameSessionResult result)
        {
            if (losePresented)
                return;

            losePresented = true;

            Debug.Log(
                "Rainbow Blocks Saga no-valid-moves: " +
                "presenting end flow, score=" +
                result.Score);

            endPresentation.PresentNoValidMoves();
        }
    }
}
