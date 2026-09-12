using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Runtime session lifecycle controller.
    /// GameSession is authoritative for Classic NoValidMoves. SessionEnded plus a runtime safety check guarantees the end flow is presented.
    /// </summary>
    public class SessionLifecycleRuntime : MonoBehaviour
    {
        [FormerlySerializedAs("sessionBridge")]
        [SerializeField] GameSessionRuntime sessionRuntime;
        [SerializeField] LevelManager levelManager;

        GameSession observedSession;
        GameSessionResult pendingResult;
        bool losePresented;

        void OnEnable()
        {
            sessionRuntime.SessionCreated += OnSessionCreated;
            sessionRuntime.SessionEnded += OnSessionEnded;
            LevelManager.ExternalClassicLifecycleEnabled = true;

            BindResolveRuntime();

            if (sessionRuntime.Session != null)
                OnSessionCreated(sessionRuntime.Session);
        }

        void OnDisable()
        {
            sessionRuntime.SessionCreated -= OnSessionCreated;
            sessionRuntime.SessionEnded -= OnSessionEnded;
            LevelManager.ExternalClassicLifecycleEnabled = false;

            var resolveRuntime = ResolveScoreRuntime.Current;
            if (resolveRuntime != null)
                resolveRuntime.PresentationCompleted -= OnPresentationCompleted;

            observedSession = null;
            pendingResult = null;
            losePresented = false;
        }

        void Update()
        {
            BindResolveRuntime();

            var session = sessionRuntime.Session;
            if (session == null)
                return;

            if (observedSession != session)
                OnSessionCreated(session);

            // Keep the new queue aligned with the visible toolkit slots, then re-evaluate.
            // This is the safety net for cases where the final visual batch was populated
            // before the session lifecycle event was observed.
            if (!session.IsEnded &&
                (EventManager.GameStatus == EGameState.Playing ||
                 EventManager.GameStatus == EGameState.Tutorial))
            {
                sessionRuntime.SynchronizeQueueFromVisualDecks();
                session.EvaluateEndState();
            }

            if (session.IsEnded &&
                session.Result != null &&
                session.Result.Reason == GameSessionEndReason.NoValidMoves &&
                !losePresented)
            {
                QueueOrPresent(session.Result);
            }
        }

        void BindResolveRuntime()
        {
            var resolveRuntime = ResolveScoreRuntime.Current;
            if (resolveRuntime == null)
                return;

            resolveRuntime.PresentationCompleted -= OnPresentationCompleted;
            resolveRuntime.PresentationCompleted += OnPresentationCompleted;
        }

        void OnSessionCreated(GameSession session)
        {
            observedSession = session;
            pendingResult = null;
            losePresented = false;
        }

        void OnSessionEnded(GameSessionResult result)
        {
            if (result.Reason != GameSessionEndReason.NoValidMoves)
                return;

            QueueOrPresent(result);
        }

        void QueueOrPresent(GameSessionResult result)
        {
            if (losePresented)
                return;

            // Step 7 migrates Classic lifecycle only. Other toolkit modes retain their
            // original target/timer state handlers until those modes are migrated separately.
            if (levelManager.GetGameMode() != EGameMode.Classic)
                return;

            var resolveRuntime = ResolveScoreRuntime.Current;
            if (resolveRuntime != null && resolveRuntime.IsPresenting)
            {
                pendingResult = result;
                return;
            }

            PresentLose(result);
        }

        void OnPresentationCompleted()
        {
            if (pendingResult == null || losePresented)
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
                $"Rainbow Block Saga no-valid-moves: presenting original end flow, score={result.Score}");

            levelManager.PresentExternalLose();
        }
    }
}
