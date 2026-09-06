// ©2015 - 2025 Candy Smith
// Original toolkit presentation preserved. Step 4 adds assembly-safe migration hooks only.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    public class CellDeckManager : MonoBehaviour
    {
        /// <summary>
        /// Optional migration hooks.
        /// They deliberately use toolkit types only so CandySmith.BlockPuzzle.Main never depends
        /// on Assembly-CSharp / RainbowBlockSaga.
        /// </summary>
        public static Func<ShapeTemplate[]> BatchProvider;
        public static Action<ShapeTemplate[]> BatchPresented;
        public static Action<Shape> ShapeConsumed;
        public static Action<ShapeTemplate> ShapeAdded;
        public static Action RecoveryRequested;

        public CellDeck[] cellDecks;

        [SerializeField] private FieldManager field;
        [SerializeField] private ItemFactory itemFactory;
        [SerializeField] public Shape shapePrefab;

        private readonly HashSet<ShapeTemplate> usedShapes = new();

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

            if (shape != null)
                ShapeConsumed?.Invoke(shape);

            if (GameManager.instance.IsTutorialMode())
                return;

            if (cellDecks.Any(x => !x.IsEmpty))
                return;

            if (TryFillFromMigrationProvider())
                return;

            FillLegacyBatch();
        }

        bool TryFillFromMigrationProvider()
        {
            if (BatchProvider == null)
                return false;

            var batch = BatchProvider.Invoke();
            if (batch == null || batch.Length == 0)
                return false;

            FillCellDecksWithShapes(batch);
            return true;
        }

        void FillLegacyBatch()
        {
            var usedShapeTemplates = new HashSet<ShapeTemplate>(GetShapes().Select(s => s.shapeTemplate));
            var fitShapesCount = 0;

            for (var index = 0; index < cellDecks.Length; index++)
            {
                var cellDeck = cellDecks[index];
                if (!cellDeck.IsEmpty)
                    continue;

                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                Shape randomShape;

                if (fitShapesCount < 2 && index >= cellDecks.Length - 2)
                    randomShape = itemFactory.CreateRandomShapeFits(shapeObject);
                else
                    randomShape = itemFactory.CreateRandomShape(usedShapeTemplates, shapeObject);

                if (field.CanPlaceShape(randomShape))
                    fitShapesCount++;

                cellDeck.FillCell(randomShape);
            }
        }

        public void FillCellDecksWithShapes(ShapeTemplate[] shapes)
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
                cellDeck.FillCell(shape);
                presented[index] = shapeTemplate;
            }

            BatchPresented?.Invoke(presented);
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

            if (TryFillFromMigrationProvider())
                return;

            foreach (var cellDeck in cellDecks)
            {
                cellDeck.ClearCell();
                cellDeck.FillCell(itemFactory.CreateRandomShapeFits(PoolObject.GetObject(shapePrefab.gameObject)));
            }
        }

        public void OnSceneActivated(Level level)
        {
            if (!GameManager.instance.IsTutorialMode())
                StartCoroutine(DelayedFillFitShapesOnly());
        }

        private IEnumerator DelayedFillFitShapesOnly()
        {
            yield return new WaitForSeconds(0.2f);

            if (TryFillFromMigrationProvider())
                yield break;

            FillFitShapesOnlyLegacy();
        }

        private void FillFitShapesOnlyLegacy()
        {
            if (cellDecks.All(x => x.IsEmpty))
                usedShapes.Clear();

            for (var index = 0; index < cellDecks.Length; index++)
            {
                var cellDeck = cellDecks[index];
                cellDeck.ClearCell();

                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                var shape = itemFactory.CreateRandomShapeFits(shapeObject, usedShapes);

                if (shape != null)
                {
                    cellDeck.FillCell(shape);
                    if (shape.shapeTemplate != null)
                        usedShapes.Add(shape.shapeTemplate);
                }
                else
                {
                    shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    shape = itemFactory.CreateRandomShape(usedShapes, shapeObject);
                    cellDeck.FillCell(shape);

                    if (shape.shapeTemplate != null)
                        usedShapes.Add(shape.shapeTemplate);
                }
            }
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
