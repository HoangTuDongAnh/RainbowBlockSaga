using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.System;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public sealed class AdventureObjectivesPopup : Popup
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text limits;
        [SerializeField] TMP_Text[] objectives;
        [SerializeField] Image[] icons;
        [SerializeField] CustomButton playButton;
        [SerializeField] CustomButton mapButton;
        bool playRequested;
        bool mapRequested;

        protected override void Awake()
        {
            base.Awake();
            playButton.onClick.AddListener(() => { playRequested = true; Close(); });
            mapButton.onClick.AddListener(() => { mapRequested = true; Close(); });
        }

        public void Configure(LevelManager manager)
        {
            var level = manager.GetCurrentLevel();
            title.text = "LEVEL " + manager.currentLevel;
            var targets = Object.FindFirstObjectByType<TargetManager>().GetTargets();
            var content = (RectTransform)title.transform.parent;
            float height = 700 + Mathf.Max(1, targets.Count) * 90;
            GetComponent<PopupContentFit>().SetDesignSize(new Vector2(1000, height));
            foreach (string name in new[] { "Card", "Rim", "Shadow" })
            {
                var frame = (RectTransform)content.Find(name);
                frame.sizeDelta = new Vector2(frame.sizeDelta.x, height - (name == "Card" ? 35 : 0));
            }
            title.rectTransform.anchoredPosition = new Vector2(0, height / 2 - 105);
            ((RectTransform)content.Find("Subtitle")).anchoredPosition = new Vector2(0, height / 2 - 180);
            ((RectTransform)content.Find("Objectives")).anchoredPosition = new Vector2(0, 10);
            limits.rectTransform.anchoredPosition = new Vector2(0, -height / 2 + 265);
            ((RectTransform)playButton.transform).anchoredPosition = new Vector2(0, -height / 2 + 160);
            ((RectTransform)mapButton.transform).anchoredPosition = new Vector2(0, -height / 2 + 55);
            for (int i = 0; i < objectives.Length; i++)
            {
                bool visible = i < targets.Count;
                objectives[i].transform.parent.gameObject.SetActive(visible);
                if (!visible) continue;
                var target = targets[i];
                bool gems = target.targetScriptable.bonusItem != null;
                objectives[i].text = gems ? "Collect " + target.amount + " gems" : "Reach " + target.amount + " points";
                icons[i].gameObject.SetActive(gems);
                if (gems) icons[i].sprite = target.targetScriptable.bonusItem.sprite;
            }
            limits.text = "";
            if (level.moveLimit > 0)
            {
                int moves = level.moveLimit - manager.MovesUsed;
                limits.text = moves + (moves == 1 ? " MOVE" : " MOVES");
            }
            if (level.enableTimer)
                limits.text += (limits.text.Length > 0 ? "  •  " : "") + manager.timerManager.RemainingTime + " SECONDS";
            if (limits.text.Length == 0) limits.text = "Take your time. Make every block count!";
        }

        public override void AfterHideAnimation()
        {
            bool start = playRequested;
            bool map = mapRequested;
            playRequested = mapRequested = false;
            base.AfterHideAnimation();
            if (map) GameManager.instance.OpenMap();
            else if (start && EventManager.GameStatus == EGameState.PrepareGame)
                EventManager.GameStatus = EGameState.Playing;
        }
    }
}
