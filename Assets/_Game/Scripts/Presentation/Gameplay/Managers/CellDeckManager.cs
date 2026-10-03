using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay.Pool;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Contracts;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public class CellDeckManager : MonoBehaviour, IBlockTrayPresentation
    {
        public int SlotCount => cellDecks != null ? cellDecks.Length : 0;

        public global::System.Func<UnityEngine.Object[]> BatchProvider { get; set; }

        public event global::System.Action<UnityEngine.Object[]> BatchPresented;
        public event global::System.Action<UnityEngine.Object> ShapeAdded;
        public event global::System.Action RecoveryRequested;

        public CellDeck[] cellDecks;

        [SerializeField] private ItemFactory itemFactory;
        [SerializeField] public Shape shapePrefab;

        private void OnEnable()
        {
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(FillCellDecks);
        }

        private void OnDisable()
        {
            ClearCellDecks();
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(FillCellDecks);
        }

        public void FillCellDecks(Shape shape = null)
        {
            RemoveUsedShapes(shape);

            if (GameManager.instance.IsTutorialMode())
                return;

            if (cellDecks.Any(x => !x.IsEmpty))
                return;

            FillFromRuntimeProvider();
        }

        void FillFromRuntimeProvider()
        {
            if (BatchProvider == null)
                throw new InvalidOperationException(
                    "CellDeckManager requires QueueSpawnRuntime to provide a batch.");

            var handles = BatchProvider.Invoke();
            if (handles == null || handles.Length == 0)
                return;

            var batch = new ShapeTemplate[handles.Length];

            for (int i = 0; i < handles.Length; i++)
                batch[i] = handles[i] as ShapeTemplate;

            FillCellDecksWithShapes(batch);
        }

        public void FillCellDecksWithShapes(ShapeTemplate[] shapes, bool generateBonuses = true)
        {
            if (shapes == null || shapes.Length == 0)
                return;

            ClearCellDecks();

            int count = Mathf.Min(cellDecks.Length, shapes.Length);
            var presented = new ShapeTemplate[count];

            for (var index = 0; index < count; index++)
            {
                var cellDeck = cellDecks[index];
                var shapeTemplate = shapes[index];
                if (shapeTemplate == null)
                    continue;

                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                var shape = shapeObject.GetComponent<Shape>();

                shape.UpdateShape(shapeTemplate);
                shape.UpdateColor(itemFactory.GetColor());
                if (generateBonuses) itemFactory.GenerateBonus(shape);
                cellDeck.FillCell(shape);
                presented[index] = shapeTemplate;
            }

            var presentedHandles = new UnityEngine.Object[presented.Length];
            for (int i = 0; i < presented.Length; i++)
                presentedHandles[i] = presented[i];

            BatchPresented?.Invoke(presentedHandles);
        }

        private void RemoveUsedShapes(Shape shape)
        {
            if (shape == null)
                return;

            foreach (var cellDeck in cellDecks)
            {
                if (cellDeck.shape != shape)
                    continue;

                // Detach the visual from its slot before returning it to the pool.
                cellDeck.FillCell(null);
                PoolObject.Return(shape.gameObject);
                break;
            }
        }

        public void ClearCellDecks()
        {
            foreach (var cellDeck in cellDecks)
                cellDeck.ClearCell();
        }

        public Shape[] GetShapes()
        {
            return cellDecks.Select(x => x.shape).Where(x => x != null).ToArray();
        }

        public void UpdateCellDeckAfterFail()
        {
            RecoveryRequested?.Invoke();
            ClearCellDecks();

            FillFromRuntimeProvider();
        }

        public void OnSceneActivated(Level level)
        {
            if (!GameManager.instance.IsTutorialMode())
                StartCoroutine(DelayedFillFromRuntime());
        }

        private IEnumerator DelayedFillFromRuntime()
        {
            yield return new WaitForSeconds(0.2f);
            if (cellDecks.All(x => x.IsEmpty))
                FillFromRuntimeProvider();
        }

        public UnityEngine.Object[] GetVisibleShapeHandles()
        {
            var shapes = GetShapes();
            var handles = new UnityEngine.Object[shapes.Length];

            for (int i = 0; i < shapes.Length; i++)
                handles[i] = shapes[i].shapeTemplate;

            return handles;
        }

        public void Clear()
        {
            ClearCellDecks();
        }

        public void AddShapeToFreeCell(ShapeTemplate shapeTemplate)
        {
            foreach (var cellDeck in cellDecks)
            {
                if (!cellDeck.IsEmpty)
                    continue;

                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                var shape = shapeObject.GetComponent<Shape>();
                shape.UpdateShape(shapeTemplate);
                shape.UpdateColor(itemFactory.GetColor());
                cellDeck.FillCell(shape);

                ShapeAdded?.Invoke(shapeTemplate);
                return;
            }
        }
    }
}
