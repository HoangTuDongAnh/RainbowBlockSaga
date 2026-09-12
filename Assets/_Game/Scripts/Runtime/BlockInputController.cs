using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.System;
using BlockPuzzleGameToolkit.Scripts.System.Haptic;
using RainbowBlockSaga.Gameplay.Board;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RainbowBlockSaga.Runtime
{
    /// <summary>
    /// Runtime input controller.
    /// Uses the current Shape presentation while BoardModel remains the authoritative
    /// occupancy state for placement validation.
    /// </summary>
    public class BlockInputController : MonoBehaviour
    {
        const float VerticalOffset = 300f;
        const float PresentationCellSize = 126f;

        RectTransform rectTransform;
        BlockViewAdapter blockView;
        Shape legacyShape;
        readonly List<Item> items = new();
        readonly Dictionary<Cell, Item> currentHits = new();
        readonly List<BoardCoord> currentCoords = new();

        Canvas canvas;
        Camera eventCamera;
        BoardRuntime boardRuntime;
        FieldManager fieldPresentation;
        HighlightManager highlightManager;

        Vector2 originalPosition;
        Vector3 originalScale;
        bool isDragging;
        int activeTouchId = -1;
        bool hasValidPlacement;

        void OnEnable()
        {
            rectTransform = (RectTransform)transform;
            blockView = GetComponent<BlockViewAdapter>();
            legacyShape = blockView.LegacyShape;
            legacyShape.OnShapeUpdated += UpdateItems;
            UpdateItems();

            canvas = GetComponentInParent<Canvas>();
            eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            EventManager.GetEvent(EGameEvent.TimerExpired).Subscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.LevelAboutToComplete).Subscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Subscribe(CancelDragIfActive);
        }

        void OnDisable()
        {
            if (legacyShape != null)
                legacyShape.OnShapeUpdated -= UpdateItems;

            EventManager.GetEvent(EGameEvent.TimerExpired).Unsubscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.LevelAboutToComplete).Unsubscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Unsubscribe(CancelDragIfActive);

            if (isDragging)
                CancelDragWithReturn();
        }

        void UpdateItems()
        {
            blockView.Refresh();
            items.Clear();
            items.AddRange(legacyShape.GetActiveItems());
        }

        void Update()
        {
            if (EventManager.GameStatus != EGameState.Playing &&
                EventManager.GameStatus != EGameState.Tutorial)
                return;

            HandleTouchInput();
            if (activeTouchId == -1)
                HandleMouseInput();
        }

        void HandleTouchInput()
        {
            if (Touchscreen.current == null)
                return;

            if (isDragging && activeTouchId != -1)
            {
                bool found = false;
                for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                {
                    var touch = Touchscreen.current.touches[i];
                    if (touch.touchId.ReadValue() != activeTouchId)
                        continue;

                    found = true;
                    HandleDrag(touch.position.ReadValue());

                    var phase = touch.phase.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                        phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                        EndDrag();
                    break;
                }

                if (!found)
                    EndDrag();

                return;
            }

            if (isDragging)
                return;

            for (int i = 0; i < Touchscreen.current.touches.Count; i++)
            {
                var touch = Touchscreen.current.touches[i];
                if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
                    continue;

                var position = touch.position.ReadValue();
                if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, position, eventCamera))
                    continue;

                activeTouchId = touch.touchId.ReadValue();
                BeginDrag();
                break;
            }
        }

        void HandleMouseInput()
        {
            if (Mouse.current == null)
                return;

            var position = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame && !isDragging)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, position, eventCamera))
                    BeginDrag();
                return;
            }

            if (Mouse.current.leftButton.isPressed && isDragging)
            {
                HandleDrag(position);
                return;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
                EndDrag();
        }

        void BeginDrag()
        {
            var context = BlockRuntimeContext.Current;
            boardRuntime = BoardRuntime.Current;

            // Shapes are pooled before the gameplay hierarchy is enabled.
            // Resolve scene-local references only when an actual drag starts.
            if (context == null || boardRuntime == null || boardRuntime.Model == null)
                return;

            highlightManager = context.HighlightManager;
            fieldPresentation = boardRuntime.PresentationSource as FieldManager;

            if (highlightManager == null || fieldPresentation == null)
                return;

            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;
            originalScale = transform.localScale;
            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;

            ClearPlacementState();
        }

        void HandleDrag(Vector2 pointerPosition)
        {
            float cellSize = fieldPresentation.GetCellSize();
            float scale = cellSize / PresentationCellSize;
            transform.localScale = new Vector3(scale, scale, 1f);

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform, pointerPosition, eventCamera, out var localPoint))
            {
                var canvasRect = (RectTransform)canvas.transform;
                float canvasWidth = canvasRect.rect.width;
                float normalizedX = localPoint.x / canvasWidth;
                float scaleY = rectTransform.rect.height / canvasRect.rect.height * 2.5f;

                rectTransform.anchoredPosition = new Vector2(
                    normalizedX * canvasWidth,
                    localPoint.y / scaleY + VerticalOffset + scaleY);
            }

            UpdatePreview();
        }

        void UpdatePreview()
        {
            highlightManager.ClearAllHighlights();
            ClearPlacementState();

            boardRuntime.SyncNow();
            if (!TryCollectPlacement())
                return;

            foreach (var pair in currentHits)
                highlightManager.HighlightCell(pair.Key.transform, pair.Value);

            // Calculate line-preview FX after temporary cell highlights exist.
            if (items.Count > 0)
                highlightManager.HighlightFill(
                    fieldPresentation.GetFilledLines(true),
                    items[0].itemTemplate);

            hasValidPlacement = true;
        }

        bool TryCollectPlacement()
        {
            if (boardRuntime.Model == null || items.Count == 0)
                return false;

            var uniqueCells = new HashSet<Cell>();

            foreach (var item in items)
            {
                var cell = GetCellUnderItem(item);
                if (cell == null || cell.IsDestroying() || !cell.IsEmpty())
                    return false;

                if (!uniqueCells.Add(cell))
                    return false;

                if (!boardRuntime.TryGetCoord(cell, out var coord))
                    return false;

                // BoardModel is the new occupancy authority.
                if (!boardRuntime.Model.IsEmpty(coord))
                    return false;

                currentHits.Add(cell, item);
                currentCoords.Add(coord);
            }

            return currentHits.Count == items.Count;
        }

        void EndDrag()
        {
            if (!isDragging)
                return;

            isDragging = false;
            activeTouchId = -1;

            // Re-evaluate at the final pointer position before committing.
            highlightManager.ClearAllHighlights();
            ClearPlacementState();
            boardRuntime.SyncNow();

            if (!TryCollectPlacement())
            {
                ReturnToDeck();
                return;
            }

            foreach (var pair in currentHits)
                highlightManager.HighlightCell(pair.Key.transform, pair.Value);

            foreach (var coord in currentCoords)
                boardRuntime.Model.SetOccupied(coord);

            HapticFeedback.TriggerHapticFeedback(HapticFeedback.HapticForce.Light);
            SoundBase.instance.PlaySound(SoundBase.instance.placeShape);

            foreach (var pair in currentHits)
            {
                pair.Key.FillCell(pair.Value.itemTemplate);
                pair.Key.AnimateFill();

                if (pair.Value.bonusItemTemplate != null)
                    pair.Key.SetBonus(pair.Value.bonusItemTemplate);
            }

            // Resolve and score before publishing the presentation ShapePlaced event.
            // Queue/Spawn then observes the post-resolve BoardModel.
            var resolveRuntime = ResolveScoreRuntime.Current;
            bool resolved = resolveRuntime != null &&
                            resolveRuntime.TryResolvePlacement(
                                blockView.Data,
                                legacyShape,
                                currentCoords);

            if (resolved)
                EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(legacyShape);

            ClearPlacementState();
        }

        void CancelDragIfActive()
        {
            if (isDragging)
                CancelDragWithReturn();
        }

        void CancelDragWithReturn()
        {
            isDragging = false;
            activeTouchId = -1;
            ReturnToDeck();
        }

        void ReturnToDeck()
        {
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;

            if (highlightManager != null)
            {
                highlightManager.ClearAllHighlights();
                highlightManager.OnDragEndedWithoutPlacement();
            }

            ClearPlacementState();
        }

        void ClearPlacementState()
        {
            hasValidPlacement = false;
            currentHits.Clear();
            currentCoords.Clear();
        }

        Cell GetCellUnderItem(Item item)
        {
            var hit = Physics2D.Raycast(item.transform.position, Vector2.zero, 1f);
            return hit.collider != null && hit.collider.CompareTag("Cell")
                ? hit.collider.GetComponent<Cell>()
                : null;
        }
    }
}
