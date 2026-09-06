using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;

namespace RainbowBlockSaga.Integration.Toolkit
{
    /// <summary>
    /// Final Step 7 compatibility boundary.
    /// GameSession is authoritative for Classic NoValidMoves. Presentation is guaranteed by
    /// both SessionEnded and a compatibility check, so a missed lifecycle event cannot leave
    /// the player stuck on a dead board.
    /// </summary>
    public class ToolkitSessionLifecyclePresentationBridge : MonoBehaviour
    {
        [SerializeField] ToolkitGameSessionMigrationBridge sessionBridge;
        [SerializeField] LevelManager levelManager;

        GameSession observedSession;
        GameSessionResult pendingResult;
        bool losePresented;

        void OnEnable()
        {
            sessionBridge.SessionCreated += OnSessionCreated;
            sessionBridge.SessionEnded += OnSessionEnded;
            LevelManager.ExternalClassicLifecycleEnabled = true;

            BindResolveBridge();

            if (sessionBridge.Session != null)
                OnSessionCreated(sessionBridge.Session);
        }

        void OnDisable()
        {
            sessionBridge.SessionCreated -= OnSessionCreated;
            sessionBridge.SessionEnded -= OnSessionEnded;
            LevelManager.ExternalClassicLifecycleEnabled = false;

            var resolveBridge = ToolkitResolveScoreMigrationBridge.Current;
            if (resolveBridge != null)
                resolveBridge.PresentationCompleted -= OnPresentationCompleted;

            observedSession = null;
            pendingResult = null;
            losePresented = false;
        }

        void Update()
        {
            BindResolveBridge();

            var session = sessionBridge.Session;
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
                sessionBridge.SynchronizeQueueFromVisualDecks();
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

        void BindResolveBridge()
        {
            var resolveBridge = ToolkitResolveScoreMigrationBridge.Current;
            if (resolveBridge == null)
                return;

            resolveBridge.PresentationCompleted -= OnPresentationCompleted;
            resolveBridge.PresentationCompleted += OnPresentationCompleted;
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

            var resolveBridge = ToolkitResolveScoreMigrationBridge.Current;
            if (resolveBridge != null && resolveBridge.IsPresenting)
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
