using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Session;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Runtime
{
    public class QueueSpawnRuntime : MonoBehaviour
    {
        [FormerlySerializedAs("sessionBridge")]
        [SerializeField] GameSessionRuntime sessionRuntime;

        IBlockTrayPresentation tray;
        IShapeCatalog catalog;

        public GameSession Session => sessionRuntime.GetOrCreateSession();

        void Start()
        {
            BindPresentation();
        }

        void OnEnable()
        {
            BindPresentation();
        }

        void BindPresentation()
        {
            if (sessionRuntime == null)
                return;

            tray = sessionRuntime.TrayPresentation;
            catalog = sessionRuntime.ShapeCatalog;

            if (tray == null || catalog == null)
                return;

            tray.BatchProvider = ProvideBatch;

            tray.BatchPresented -= OnBatchPresented;
            tray.BatchPresented += OnBatchPresented;

            tray.ShapeAdded -= OnShapeAdded;
            tray.ShapeAdded += OnShapeAdded;

            tray.RecoveryRequested -= OnRecoveryRequested;
            tray.RecoveryRequested += OnRecoveryRequested;
        }

        void OnDisable()
        {
            if (tray == null)
                return;

            if (tray.BatchProvider == ProvideBatch)
                tray.BatchProvider = null;

            tray.BatchPresented -= OnBatchPresented;
            tray.ShapeAdded -= OnShapeAdded;
            tray.RecoveryRequested -= OnRecoveryRequested;
        }

        UnityEngine.Object[] ProvideBatch()
        {
            var session = Session;
            if (session == null)
                return null;

            if (session.IsEnded)
                return new UnityEngine.Object[tray.SlotCount];

            sessionRuntime.RefreshSpawnProfile();
            session.EnsureBatch();

            var result =
                new UnityEngine.Object[session.Queue.Shapes.Count];

            for (int i = 0; i < session.Queue.Shapes.Count; i++)
            {
                result[i] =
                    ShapeDataAdapter.GetPresentationHandle(
                        session.Queue.Shapes[i]);
            }

            return result;
        }

        void OnBatchPresented(UnityEngine.Object[] handles)
        {
            var session = Session;
            if (session == null || session.IsEnded)
                return;

            if (MatchesCurrentQueue(session, handles))
                return;

            var batch = new List<BlockShapeData>();

            foreach (var handle in handles)
            {
                var data =
                    ShapeDataAdapter.GetOrCreate(
                        handle,
                        catalog);

                if (data != null)
                    batch.Add(data);
            }

            session.SetExternalBatch(batch);
        }

        void OnRecoveryRequested()
        {
            sessionRuntime.RecoverFromNoMoves();
        }

        void OnShapeAdded(UnityEngine.Object handle)
        {
            var session = Session;
            if (session == null ||
                session.IsEnded ||
                handle == null)
                return;

            var data =
                ShapeDataAdapter.GetOrCreate(
                    handle,
                    catalog);

            if (data != null)
                session.AddExternalShape(data);
        }

        bool MatchesCurrentQueue(
            GameSession session,
            UnityEngine.Object[] handles)
        {
            if (handles == null ||
                handles.Length != session.Queue.Shapes.Count)
                return false;

            for (int i = 0; i < handles.Length; i++)
            {
                if (ShapeDataAdapter.GetPresentationHandle(
                        session.Queue.Shapes[i]) != handles[i])
                    return false;
            }

            return true;
        }
    }
}
