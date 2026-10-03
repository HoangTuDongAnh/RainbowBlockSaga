using UnityEngine;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.GUI.Orientation;

namespace RainbowBlockSaga.Presentation.Gameplay
{
    // Board, tray and boosters use separate canvases: arrange them in screen space.
    [DefaultExecutionOrder(10000)]
    public sealed class EndlessGameplayLayout : MonoBehaviour
    {
        [SerializeField] RectTransform board;
        [SerializeField] RectTransform tray;
        [SerializeField] RectTransform support;
        [SerializeField] OrientationTransformableObject boardOrientation;
        [SerializeField] OrientationTransformableObject trayOrientation;
        [SerializeField, Range(0.05f, 0.3f)] float headerFraction = 0.16f;
        [SerializeField, Range(0.01f, 0.08f)] float gapFraction = 0.025f;
        readonly Vector3[] corners = new Vector3[4];
        bool applied;

        void OnEnable() => RefreshVisibility();

        bool RefreshVisibility()
        {
            bool show = GameDataManager.GetGameMode() == EGameMode.Endless &&
                !GameManager.instance.IsTutorialMode();
            if (support.gameObject.activeSelf != show)
                support.gameObject.SetActive(show);
            return show;
        }

        void LateUpdate()
        {
            if (!RefreshVisibility())
            {
                Restore();
                return;
            }
            // These transforms can be temporarily zero-scaled by screen transitions.
            if (board.lossyScale.x == 0 || tray.lossyScale.x == 0 || support.lossyScale.x == 0)
                return;
            applied = true;
            var safe = Screen.safeArea;
            float gap = safe.height * gapFraction;
            float bottom = safe.yMin + gap;
            float top = safe.yMax - safe.height * headerFraction;
            float trayRatio = tray.rect.height / tray.rect.width;
            const float supportRatio = 0.35f; // Includes count badges and price pills.
            if (safe.width <= safe.height)
            {
                float width = Mathf.Min(safe.width * 0.80f,
                    (top - bottom - 2 * gap) / (1 + trayRatio + supportRatio));
                Place(board, new Vector2(safe.center.x, top - width / 2), width);
                float trayTop = top - width - gap;
                Place(tray, new Vector2(safe.center.x, trayTop - width * trayRatio / 2), width);
                float supportTop = trayTop - width * trayRatio - gap;
                Place(support, new Vector2(safe.center.x, supportTop - width * supportRatio / 2), width);
            }
            else
            {
                float size = Mathf.Min(top - bottom, safe.width * 0.48f);
                float width = Mathf.Min(safe.width * 0.40f,
                    (top - bottom - gap) / (trayRatio + supportRatio));
                Place(board, new Vector2(safe.xMin + safe.width * 0.27f, (top + bottom) / 2), size);
                float x = safe.xMin + safe.width * 0.77f;
                Place(tray, new Vector2(x, top - width * trayRatio / 2), width);
                Place(support, new Vector2(x, top - width * trayRatio - gap - width * supportRatio / 2), width);
            }
        }

        void Place(RectTransform rect, Vector2 center, float width)
        {
            var canvas = rect.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            rect.localRotation = Quaternion.identity;
            rect.GetWorldCorners(corners);
            float currentWidth = Vector2.Distance(
                RectTransformUtility.WorldToScreenPoint(camera, corners[0]),
                RectTransformUtility.WorldToScreenPoint(camera, corners[3]));
            if (currentWidth < 0.001f) return;
            rect.localScale *= width / currentWidth;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                    (RectTransform)rect.parent, center, camera, out var position))
                rect.position = position;
        }

        void OnDisable() => Restore();
        void Restore()
        {
            if (!applied) return;
            // Restore the original preset for the current orientation, not a stale snapshot.
            if (Screen.width > Screen.height)
            {
                boardOrientation.PreviewLandscapeConfiguration();
                trayOrientation.PreviewLandscapeConfiguration();
            }
            else
            {
                boardOrientation.PreviewPortraitConfiguration();
                trayOrientation.PreviewPortraitConfiguration();
            }
            applied = false;
        }
    }
}
