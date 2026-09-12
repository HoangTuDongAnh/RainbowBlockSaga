using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Runtime queue/spawn presentation controller.
    /// GameSession owns BlockQueue + SpawnStrategy; this component maps runtime shape data to the current ShapeTemplate / CellDeck presentation.
    /// </summary>
    public class QueueSpawnRuntime : MonoBehaviour
    {
        [FormerlySerializedAs("sessionBridge")]
        [SerializeField] GameSessionRuntime sessionRuntime;

        public BlockPuzzleGameToolkit.Scripts.Gameplay.CellDeckManager DeckManager =>
            sessionRuntime.DeckManager;

        public GameSession Session => sessionRuntime.GetOrCreateSession();

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
            // This prevents CellDeckManager from falling back to its presentation random generator
            // after GameSession has already decided NoValidMoves.
            if (session.IsEnded)
                return new ShapeTemplate[sessionRuntime.DeckManager.cellDecks.Length];

            sessionRuntime.RefreshSpawnProfile();
            session.EnsureBatch();

            var result =
                new ShapeTemplate[session.Queue.Shapes.Count];

            for (int i = 0; i < session.Queue.Shapes.Count; i++)
            {
                result[i] =
                    ShapeDataAdapter.GetTemplate(
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
                    ShapeDataAdapter.GetOrCreate(template);

                if (data != null)
                    batch.Add(data);
            }

            session.SetExternalBatch(batch);
        }

        void OnShapeConsumed(Shape shape)
        {
            // Consumption now happens inside GameSession.ResolveExternalPlacement()
            // before the presentation ShapePlaced compatibility event is published.
        }

        void OnRecoveryRequested()
        {
            sessionRuntime.RecoverFromNoMoves();
        }

        void OnShapeAdded(ShapeTemplate template)
        {
            var session = Session;
            if (session == null ||
                session.IsEnded ||
                template == null)
                return;

            session.AddExternalShape(
                ShapeDataAdapter.GetOrCreate(template));
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
                if (ShapeDataAdapter.GetTemplate(
                        session.Queue.Shapes[i]) != templates[i])
                    return false;
            }

            return true;
        }
    }
}
