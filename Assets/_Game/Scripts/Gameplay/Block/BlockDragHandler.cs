using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Gameplay.Session;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RainbowBlockSaga.Gameplay.Block
{
    public class BlockDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] BlockView blockView;
        [SerializeField] Vector2 dragOffset = new(0f, 110f);

        RectTransform rect;
        Transform originalParent;
        int originalSiblingIndex;
        Vector2 originalPosition;
        GameplayController gameplay;
        RectTransform dragRoot;

        public void Initialize(GameplayController gameplayController, RectTransform root)
        {
            gameplay = gameplayController;
            dragRoot = root;
        }

        void Awake() => rect = (RectTransform)transform;

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            originalPosition = rect.anchoredPosition;
            transform.SetParent(dragRoot, true);
            transform.SetAsLastSibling();
            Move(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Move(eventData.position);
            UpdatePreview(eventData);
        }

        void Move(Vector2 screenPosition)
        {
            rect.position = screenPosition + dragOffset;
        }

        void UpdatePreview(PointerEventData eventData)
        {
            Vector2 samplePosition = eventData.position + dragOffset;
            if (!gameplay.BoardView.TryGetCoord(samplePosition, eventData.pressEventCamera, out var anchor))
            {
                gameplay.BoardView.ClearPreview();
                return;
            }

            var coords = gameplay.Session.Placement.GetPlacementCells(blockView.Data, anchor);
            bool valid = gameplay.CanPlace(blockView.Data, anchor);
            gameplay.BoardView.ShowPreview(coords, valid);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 samplePosition = eventData.position + dragOffset;
            bool placed = false;
            if (gameplay.BoardView.TryGetCoord(samplePosition, eventData.pressEventCamera, out BoardCoord anchor))
                placed = gameplay.TryPlace(blockView.Data, anchor);

            gameplay.BoardView.ClearPreview();
            if (placed) return;

            transform.SetParent(originalParent, false);
            transform.SetSiblingIndex(originalSiblingIndex);
            rect.anchoredPosition = originalPosition;
        }
    }
}
