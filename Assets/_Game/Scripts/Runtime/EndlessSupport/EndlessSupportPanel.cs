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
        EndlessSupportItem? pendingTarget;

        const string CountPrefix = "RBS_EndlessSupport_";

        void Awake()
        {
            if (buttons == null || buttons.Length == 0)
                throw new InvalidOperationException("EndlessSupportPanel requires three inspector-wired buttons.");

            foreach (var config in configs)
                counts[config.item] = PlayerPrefs.GetInt(Key(config.item), 0);
        }

        void OnEnable()
        {
            hintLabel.text = "";
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
            foreach (var view in buttons)
            {
                if (view != null && view.Button != null)
                    view.Button.onClick.RemoveAllListeners();
            }
            pendingTarget = null;
            tray.CancelSupportShape();
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

            // Match the board's UI camera, including screen-space overlay canvases.
            var canvas = field.GetComponentInParent<Canvas>();
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            for (int row = 0; row < field.RowCount; row++)
            for (int column = 0; column < field.ColumnCount; column++)
            {
                var cell = field.cells[row, column];
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)cell.transform, position, camera)) continue;
                if (UseAt(pendingTarget.Value, row, column))
                {
                    pendingTarget = null;
                    RefreshViews();
                }
                return;
            }
        }

        bool CanUseItems() => GameDataManager.GetGameMode() == EGameMode.Endless &&
            !GameManager.instance.IsTutorialMode() &&
            EventManager.GameStatus == EGameState.Playing && field.IsReady &&
            sessionRuntime.CanAcceptPlacement &&
            !ResolveScoreRuntime.Current.IsPresenting;

        void SelectOrBuy(EndlessSupportItem item)
        {
            if (!CanUseItems()) return;
            if (!counts.TryGetValue(item, out int count)) return;
            if (count == 0)
            {
                var config = GetConfig(item);
                if (!ResourceManager.instance.Consume("Coins", config.price)) { hintLabel.text = "NOT ENOUGH COINS"; return; }
                counts[item] = 1;
                Save(item);
                RefreshViews();
                hintLabel.text = "+1 " + config.displayName + " - TAP TO USE";
                if (SoundBase.instance != null) SoundBase.instance.PlaySound(SoundBase.instance.coinsSpend);
                return;
            }

            if (item == EndlessSupportItem.RainbowCell)
            {
                pendingTarget = null;
                if (tray.HasSupportShape) tray.CancelSupportShape();
                else tray.ShowSupportShape(rainbowShape, () =>
                {
                    counts[item]--;
                    Save(item);
                    hintLabel.text = "";
                    RefreshViews();
                });
                RefreshViews();
                hintLabel.text = tray.HasSupportShape ? "DRAG BLOCK - TAP AGAIN TO CANCEL" : "";
                return;
            }
            tray.CancelSupportShape();
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
            hintLabel.text = pendingTarget.HasValue ? "TAP A CELL TO BLAST 3 x 3" : "";
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
            foreach (var view in buttons)
            {
                if (view == null) continue;
                var config = GetConfig(view.Item);
                view.Configure(config.icon, config.displayName, config.price);
                view.SetCount(counts[view.Item]);
                view.SetSelected(pendingTarget == view.Item || (view.Item == EndlessSupportItem.RainbowCell && tray.HasSupportShape));
                view.SetInteractable(true);
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
