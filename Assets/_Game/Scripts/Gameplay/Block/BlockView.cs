using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Board;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Gameplay.Block
{
    public class BlockView : MonoBehaviour
    {
        [SerializeField] RectTransform container;
        [SerializeField] Image cellPrefab;
        [SerializeField] float cellSize = 42f;
        [SerializeField] float spacing = 3f;

        readonly List<GameObject> spawnedCells = new();
        public BlockShapeData Data { get; private set; }

        public void Initialize(BlockShapeData data)
        {
            Data = data;
            Rebuild();
        }

        void Rebuild()
        {
            foreach (var item in spawnedCells)
                if (item) Destroy(item);
            spawnedCells.Clear();

            var cells = Data.GetNormalizedCells();
            int maxX = 0;
            int maxY = 0;
            foreach (var coord in cells)
            {
                if (coord.X > maxX) maxX = coord.X;
                if (coord.Y > maxY) maxY = coord.Y;
            }

            float width = (maxX + 1) * cellSize + maxX * spacing;
            float height = (maxY + 1) * cellSize + maxY * spacing;
            container.sizeDelta = new Vector2(width, height);
            ((RectTransform)transform).sizeDelta = new Vector2(Mathf.Max(150f, width), Mathf.Max(150f, height));

            Vector2 origin = new(-width * .5f + cellSize * .5f, -height * .5f + cellSize * .5f);
            foreach (var coord in cells)
            {
                var image = Instantiate(cellPrefab, container);
                var rect = image.rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
                rect.pivot = new Vector2(.5f, .5f);
                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = origin + new Vector2(coord.X * (cellSize + spacing), coord.Y * (cellSize + spacing));
                spawnedCells.Add(image.gameObject);
            }
        }
    }
}
