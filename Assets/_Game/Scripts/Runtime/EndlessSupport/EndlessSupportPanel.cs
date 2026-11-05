using System;
using System.Collections.Generic;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Audio;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using System.Collections;
using RainbowBlockSaga.Presentation.RuntimeAdapters;
using UnityEngine.EventSystems;

namespace RainbowBlockSaga.Presentation.Gameplay
{
    /// <summary>Inspector-wired Endless-only support item controller.</summary>
    public sealed class EndlessSupportPanel : MonoBehaviour
    {
        [Serializable]
        struct ItemConfig
        {
            public EndlessSupportItem item;
            public Sprite icon;
            public int price;
            public string displayName;
        }

        [SerializeField] FieldManager field;
        [SerializeField] ShapeTemplate rainbowShape;
        [SerializeField] TMP_Text hintLabel;
        [SerializeField] CellDeckManager tray;
        [SerializeField] BoardRuntime boardRuntime;
        [SerializeField] GameSessionRuntime sessionRuntime;
        [SerializeField] LevelManager levelManager;
        [SerializeField] EndlessSupportButton[] buttons;
        [SerializeField] ItemConfig[] configs =
        {
            new ItemConfig { item = EndlessSupportItem.RainbowCell, price = 30, displayName = "RAINBOW" },
            new ItemConfig { item = EndlessSupportItem.Bomb3x3, price = 50, displayName = "BOMB 3x3" },
            new ItemConfig { item = EndlessSupportItem.ShuffleTray, price = 40, displayName = "SHUFFLE" }
        };

        readonly Dictionary<EndlessSupportItem, int> counts = new();
        readonly Dictionary<Cell, Color> previewColors = new();
        EndlessSupportItem? pendingTarget;
        int previewRow = -1, previewColumn = -1;
        Coroutine hintTimeout;

        const string CountPrefix = "RBS_EndlessSupport_";

        [SerializeField] GameObject hintBacking;

        void Awake()
        {
            if (buttons == null || buttons.Length == 0)
                throw new InvalidOperationException("EndlessSupportPanel requires three inspector-wired buttons.");

            foreach (var config in configs)
                counts[config.item] = PlayerPrefs.GetInt(Key(config.item), 0);
        }

        void OnEnable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel).Subscribe(ResetInteraction);
            hintLabel.text = "";
            hintBacking.SetActive(false);
            foreach (var config in configs)
                counts[config.item] = PlayerPrefs.GetInt(Key(config.item), 0);
            foreach (var view in buttons)
            {
                if (view == null || view.Button == null) continue;
                var captured = view.Item;
                view.Button.onClick.AddListener(() => SelectOrBuy(captured));
            }
            RefreshViews();
        }

        void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel).Unsubscribe(ResetInteraction);
            ResetInteraction();
            foreach (var view in buttons)
            {
                if (view != null && view.Button != null)
                    view.Button.onClick.RemoveAllListeners();
            }
        }

        void ResetInteraction()
        {
            ClearPreview();
            if (hintTimeout != null) StopCoroutine(hintTimeout);
            hintTimeout = null;
            hintLabel.text = "";
            hintBacking.SetActive(false);
            pendingTarget = null;
            tray.CancelSupportShape();
            RefreshViews();
        }

        void Update()
        {
            if (!pendingTarget.HasValue || !CanUseItems()) return;
            Vector2 position;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                position = Touchscreen.current.primaryTouch.position.ReadValue();
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                position = Mouse.current.position.ReadValue();
            else return;

            // A popup's full-screen raycast layer must win over the board.
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, hits);
            if (hits.Count == 0 || !hits[0].gameObject.transform.IsChildOf(field.transform)) return;

            // Match the board's UI camera, including screen-space overlay canvases.
            var canvas = field.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            for (int row = 0; row < field.RowCount; row++)
            for (int column = 0; column < field.ColumnCount; column++)
            {
                var cell = field.cells[row, column];
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)cell.transform, position, camera)) continue;
                if (previewRow != row || previewColumn != column)
                {
                    PreviewArea(row, column);
                    SetHint("TAP AGAIN TO BLAST");
                    return;
                }
                if (UseAt(pendingTarget.Value, row, column))
                {
                    pendingTarget = null;
                    ClearPreview();
                    SetHint("");
                    RefreshViews();
                }
                return;
            }
        }

        bool CanUseItems() => GameDataManager.GetGameMode() == EGameMode.Endless &&
            !GameManager.instance.IsTutorialMode() &&
            EventManager.GameStatus == EGameState.Playing && field.IsReady &&
            sessionRuntime.CanAcceptPlacement &&
            !BlockInputController.AnyDragging &&
            !ResolveScoreRuntime.Current.IsPresenting;

        void SetHint(string message, float timeout = 0f)
        {
            if (hintTimeout != null) StopCoroutine(hintTimeout);
            hintLabel.text = message;
            hintBacking.SetActive(!string.IsNullOrEmpty(message));
            hintTimeout = timeout > 0f ? StartCoroutine(HideHintAfter(timeout)) : null;
        }

        IEnumerator HideHintAfter(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            hintLabel.text = "";
            hintBacking.SetActive(false);
            hintTimeout = null;
        }

        void PreviewArea(int row, int column)
        {
            ClearPreview();
            previewRow = row;
            previewColumn = column;
            for (int r = row - 1; r <= row + 1; r++)
            for (int c = column - 1; c <= column + 1; c++)
            {
                if (r < 0 || c < 0 || r >= field.RowCount || c >= field.ColumnCount) continue;
                var cell = field.cells[r, c];
                if (cell.IsDisabled()) continue;
                previewColors[cell] = cell.image.color;
                cell.image.color = Color.Lerp(cell.image.color, new Color(1f, .45f, .35f), .65f);
            }
        }

        void ClearPreview()
        {
            foreach (var pair in previewColors)
                if (pair.Key != null) pair.Key.image.color = pair.Value;
            previewColors.Clear();
            previewRow = previewColumn = -1;
        }

        void SelectOrBuy(EndlessSupportItem item)
        {
            if (!CanUseItems()) return;
            if (!counts.TryGetValue(item, out int count)) return;
            if (count == 0)
            {
                var config = GetConfig(item);
                if (!ResourceManager.instance.Consume("Coins", config.price)) { SetHint("NOT ENOUGH COINS", 2.5f); return; }
                counts[item] = 1;
                Save(item);
                RefreshViews();
                SetHint("READY: " + config.displayName, 2f);
                if (SoundBase.instance != null) SoundBase.instance.PlaySound(SoundBase.instance.coinsSpend);
                return;
            }

            if (item == EndlessSupportItem.RainbowCell)
            {
                ClearPreview();
                pendingTarget = null;
                if (tray.HasSupportShape) tray.CancelSupportShape();
                else tray.ShowSupportShape(rainbowShape, () =>
                {
                    counts[item]--;
                    Save(item);
                    SetHint("");
                    RefreshViews();
                });
                RefreshViews();
                SetHint(tray.HasSupportShape ? "DRAG THE RAINBOW BLOCK" : "");
                return;
            }
            tray.CancelSupportShape();
            ClearPreview();
            if (item == EndlessSupportItem.ShuffleTray)
            {
                if (UseShuffle())
                {
                    counts[item]--;
                    levelManager.SaveRun();
                    Save(item);
                    RefreshViews();
                }
                return;
            }

            pendingTarget = pendingTarget == item ? null : item;
            SetHint(pendingTarget.HasValue ? "TAP A CELL" : "");
            RefreshViews();
        }

        bool UseAt(EndlessSupportItem item, int row, int column)
        {
            if (field.cells == null || row < 0 || column < 0 ||
                row >= field.RowCount || column >= field.ColumnCount)
                return false;

            if (!CanUseItems() || counts[item] <= 0) return false;
            if (item == EndlessSupportItem.Bomb3x3)
            {
                bool destroyed = false;
                for (int r = row - 1; r <= row + 1; r++)
                for (int c = column - 1; c <= column + 1; c++)
                    if (r >= 0 && c >= 0 && r < field.RowCount && c < field.ColumnCount &&
                        !field.cells[r, c].IsDisabled() && !field.cells[r, c].IsEmpty())
                    {
                        field.cells[r, c].DestroyCell();
                        destroyed = true;
                    }
                if (!destroyed) return false;
                boardRuntime.SyncNow();
            }
            else return false;

            counts[item]--;
            levelManager.SaveRun();
            Save(item);
            RefreshViews();
            return true;
        }

        bool UseShuffle()
        {
            var session = sessionRuntime.GetOrCreateSession();
            if (session == null || session.IsEnded || session.IsPaused) return false;
            pendingTarget = null;
            boardRuntime.SyncNow();
            session.Queue.Clear();
            tray.ClearCellDecks();
            tray.FillCellDecks();
            return true;
        }

        void RefreshViews()
        {
            bool hasSelection = pendingTarget.HasValue || tray.HasSupportShape;
            foreach (var view in buttons)
            {
                if (view == null) continue;
                var config = GetConfig(view.Item);
                view.Configure(config.icon, config.displayName, config.price);
                view.SetCount(counts[view.Item]);
                bool selected = pendingTarget == view.Item ||
                    (view.Item == EndlessSupportItem.RainbowCell && tray.HasSupportShape);
                view.SetSelected(selected);
                view.SetInteractable(!hasSelection || selected);
            }
        }

        ItemConfig GetConfig(EndlessSupportItem item)
        {
            foreach (var config in configs)
                if (config.item == item) return config;
            throw new InvalidOperationException($"Missing config for {item}.");
        }

        static string Key(EndlessSupportItem item) => CountPrefix + item;
        void Save(EndlessSupportItem item) { PlayerPrefs.SetInt(Key(item), counts[item]); PlayerPrefs.Save(); }
    }
}
