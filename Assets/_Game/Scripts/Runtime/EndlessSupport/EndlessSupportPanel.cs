using System;
using System.Collections.Generic;
using RainbowBlockSaga.Gameplay.Block;
using RainbowBlockSaga.Gameplay.Board;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Audio;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Contracts;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] CellDeckManager tray;
        [SerializeField] ItemFactory itemFactory;
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
            bool endless = GameDataManager.GetGameMode() == EGameMode.Endless;
            if (!endless) return;

            foreach (var view in buttons)
            {
                if (view == null || view.Button == null) continue;
                var captured = view.Item;
                view.Button.onClick.AddListener(() => SelectOrBuy(captured));
            }
            RefreshViews();
        }

        void Start()
        {
            if (GameDataManager.GetGameMode() != EGameMode.Endless)
                gameObject.SetActive(false);
        }

        void OnDisable()
        {
            foreach (var view in buttons)
            {
                if (view != null && view.Button != null)
                    view.Button.onClick.RemoveAllListeners();
            }
            pendingTarget = null;
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
            EventManager.GameStatus == EGameState.Playing && field.IsReady &&
            !ResolveScoreRuntime.Current.IsPresenting;

        void SelectOrBuy(EndlessSupportItem item)
        {
            if (!CanUseItems()) return;
            if (!counts.TryGetValue(item, out int count)) return;
            if (count == 0)
            {
                var config = GetConfig(item);
                if (!ResourceManager.instance.Consume("Coins", config.price)) return;
                counts[item] = 1;
                Save(item);
                RefreshViews();
                if (SoundBase.instance != null) SoundBase.instance.PlaySound(SoundBase.instance.coinsSpend);
                return;
            }

            if (item == EndlessSupportItem.ShuffleTray)
            {
                if (UseShuffle())
                {
                    counts[item]--;
                    Save(item);
                    RefreshViews();
                }
                return;
            }

            pendingTarget = pendingTarget == item ? null : item;
            RefreshViews();
        }

        bool UseAt(EndlessSupportItem item, int row, int column)
        {
            if (field.cells == null || row < 0 || column < 0 ||
                row >= field.RowCount || column >= field.ColumnCount)
                return false;

            if (item == EndlessSupportItem.RainbowCell)
            {
                var cell = field.cells[row, column];
                if (cell.IsDisabled() || !cell.IsEmpty()) return false;
                cell.FillCell(itemFactory.GetColor());
                ResolveSupportScore(new[] { new BoardCoord(column, field.RowCount - 1 - row) });
            }
            else if (item == EndlessSupportItem.Bomb3x3)
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

        void ResolveSupportScore(IReadOnlyList<BoardCoord> coords)
        {
            boardRuntime.SyncNow();
            var session = sessionRuntime.GetOrCreateSession();
            if (session == null) return;
            var data = ScriptableObject.CreateInstance<BlockShapeData>();
            data.Cells = new List<BoardCoord> { new BoardCoord(0, 0) };
            var outcome = session.ResolveExternalPlacement(data, coords);
            Destroy(data);
            if (outcome == null) return;
            foreach (int row in outcome.Resolve.Rows)
                for (int column = 0; column < field.ColumnCount; column++)
                    if (!field.cells[field.RowCount - 1 - row, column].IsEmpty())
                        field.cells[field.RowCount - 1 - row, column].DestroyCell();
            foreach (int column in outcome.Resolve.Columns)
                for (int row = 0; row < field.RowCount; row++)
                    if (!field.cells[field.RowCount - 1 - row, column].IsEmpty())
                        field.cells[field.RowCount - 1 - row, column].DestroyCell();
            if (levelManager != null)
                levelManager.PresentExternalResolve(
                    null,
                    new List<List<Cell>>(),
                    outcome.ScoreGain,
                    session.Score.Combo,
                    null,
                    session.Score.LastTurn);
            boardRuntime.SyncNow();
        }

        void RefreshViews()
        {
            foreach (var view in buttons)
            {
                if (view == null) continue;
                var config = GetConfig(view.Item);
                view.Configure(config.icon, config.displayName, config.price);
                view.SetCount(counts[view.Item]);
                view.SetSelected(pendingTarget == view.Item);
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
