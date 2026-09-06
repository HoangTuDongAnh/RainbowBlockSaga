using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Gameplay.Board
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] RectTransform container;
        [SerializeField] GridLayoutGroup grid;
        [SerializeField] BoardCellView cellPrefab;

        readonly Dictionary<BoardCoord, BoardCellView> cells = new();
        BoardModel model;

        public RectTransform Container => container;

        public void Initialize(BoardModel board)
        {
            model = board;
            Rebuild();
            Refresh();
        }

        public void Rebuild()
        {
            foreach (Transform child in container) Destroy(child.gameObject);
            cells.Clear();

            ConfigureGrid();
            for (int y = model.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var coord = new BoardCoord(x, y);
                    var cell = Instantiate(cellPrefab, container);
                    cell.name = $"Cell_{x}_{y}";
                    cell.Initialize(coord);
                    cell.SetPlayable(model.IsPlayable(coord));
                    cells.Add(coord, cell);
                }
            }
        }

        void ConfigureGrid()
        {
            var profile = model.Data.ViewProfile;
            float spacing = profile ? profile.Spacing : 4f;
            Vector2 padding = profile ? profile.Padding : new Vector2(8f, 8f);
            float min = profile ? profile.MinCellSize : 24f;
            float max = profile ? profile.MaxCellSize : 120f;

            float width = container.rect.width - padding.x * 2f - spacing * (model.Width - 1);
            float height = container.rect.height - padding.y * 2f - spacing * (model.Height - 1);
            float size = Mathf.Clamp(Mathf.Min(width / model.Width, height / model.Height), min, max);

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = model.Width;
            grid.spacing = new Vector2(spacing, spacing);
            grid.padding = new RectOffset(Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.y), Mathf.RoundToInt(padding.y));
            grid.cellSize = new Vector2(size, size);
        }

        public void Refresh()
        {
            foreach (var pair in cells)
                pair.Value.SetOccupied(model.IsOccupied(pair.Key));
        }

        public bool TryGetCoord(Vector2 screenPosition, Camera eventCamera, out BoardCoord coord)
        {
            coord = default;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screenPosition, eventCamera, out var local)) return false;

            var profile = model.Data.ViewProfile;
            float spacing = profile ? profile.Spacing : 4f;
            Vector2 padding = profile ? profile.Padding : new Vector2(8f, 8f);
            float stepX = grid.cellSize.x + spacing;
            float stepY = grid.cellSize.y + spacing;

            float left = -container.rect.width * container.pivot.x + padding.x;
            float top = container.rect.height * (1f - container.pivot.y) - padding.y;
            int x = Mathf.FloorToInt((local.x - left) / stepX);
            int visualRow = Mathf.FloorToInt((top - local.y) / stepY);
            int y = model.Height - 1 - visualRow;
            var candidate = new BoardCoord(x, y);
            if (!model.Contains(candidate)) return false;
            coord = candidate;
            return true;
        }

        public void ShowPreview(IReadOnlyList<BoardCoord> coords, bool valid)
        {
            ClearPreview();
            foreach (var coord in coords)
                if (cells.TryGetValue(coord, out var cell))
                    cell.SetPreview(true, valid);
        }

        public void ClearPreview()
        {
            foreach (var cell in cells.Values) cell.SetPreview(false, true);
        }
    }
}
