using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;

namespace RainbowBlockSaga.Integration.Toolkit
{
    /// <summary>
    /// Step 6 queue presentation bridge.
    /// GameSession owns BlockQueue + SpawnStrategy; this component only maps between
    /// BlockShapeData and the preserved toolkit ShapeTemplate / CellDeck visuals.
    /// </summary>
    public class ToolkitQueueSpawnMigrationBridge : MonoBehaviour
    {
        [SerializeField] ToolkitGameSessionMigrationBridge sessionBridge;

        public BlockPuzzleGameToolkit.Scripts.Gameplay.CellDeckManager DeckManager =>
            sessionBridge.DeckManager;

        public GameSession Session => sessionBridge.GetOrCreateSession();

        void OnEnable()
        {
            CellDeckManager.BatchProvider = ProvideBatch;
            CellDeckManager.BatchPresented = OnBatchPresented;
            CellDeckManager.ShapeConsumed = OnShapeConsumed;
            CellDeckManager.ShapeAdded = OnShapeAdded;
            CellDeckManager.RecoveryRequested = OnRecoveryRequested;
        }

        void OnDisable()
        {
            if (CellDeckManager.BatchProvider == ProvideBatch)
                CellDeckManager.BatchProvider = null;

            if (CellDeckManager.BatchPresented == OnBatchPresented)
                CellDeckManager.BatchPresented = null;

            if (CellDeckManager.ShapeConsumed == OnShapeConsumed)
                CellDeckManager.ShapeConsumed = null;

            if (CellDeckManager.ShapeAdded == OnShapeAdded)
                CellDeckManager.ShapeAdded = null;

            if (CellDeckManager.RecoveryRequested == OnRecoveryRequested)
                CellDeckManager.RecoveryRequested = null;
        }

        ShapeTemplate[] ProvideBatch()
        {
            var session = Session;
            if (session == null)
                return null;

            // A non-empty null batch means "the new owner intentionally has no next batch".
            // This prevents CellDeckManager from falling back to its legacy random generator
            // after GameSession has already decided NoValidMoves.
            if (session.IsEnded)
                return new ShapeTemplate[sessionBridge.DeckManager.cellDecks.Length];

            sessionBridge.RefreshSpawnProfile();
            session.EnsureBatch();

            var result =
                new ShapeTemplate[session.Queue.Shapes.Count];

            for (int i = 0; i < session.Queue.Shapes.Count; i++)
            {
                result[i] =
                    ToolkitShapeDataAdapter.GetTemplate(
                        session.Queue.Shapes[i]);
            }

            return result;
        }

        void OnBatchPresented(ShapeTemplate[] templates)
        {
            var session = Session;
            if (session == null || session.IsEnded)
                return;

            // Normal provider batches are already owned by Session.Queue.
            // This method also supports tutorial / externally forced batches.
            if (MatchesCurrentQueue(session, templates))
                return;

            var batch = new List<BlockShapeData>();

            foreach (var template in templates)
            {
                if (template == null)
                    continue;

                var data =
                    ToolkitShapeDataAdapter.GetOrCreate(template);

                if (data != null)
                    batch.Add(data);
            }

            session.SetExternalBatch(batch);
        }

        void OnShapeConsumed(Shape shape)
        {
            // Consumption now happens inside GameSession.ResolveExternalPlacement()
            // before the legacy ShapePlaced compatibility event is published.
        }

        void OnRecoveryRequested()
        {
            sessionBridge.RecoverFromNoMoves();
        }

        void OnShapeAdded(ShapeTemplate template)
        {
            var session = Session;
            if (session == null ||
                session.IsEnded ||
                template == null)
                return;

            session.AddExternalShape(
                ToolkitShapeDataAdapter.GetOrCreate(template));
        }

        bool MatchesCurrentQueue(
            GameSession session,
            ShapeTemplate[] templates)
        {
            if (templates == null ||
                templates.Length != session.Queue.Shapes.Count)
                return false;

            for (int i = 0; i < templates.Length; i++)
            {
                if (ToolkitShapeDataAdapter.GetTemplate(
                        session.Queue.Shapes[i]) != templates[i])
                    return false;
            }

            return true;
        }
    }
}
