using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class FailedEndless : Failed
    {
        public GameObject failedStuff;
        public GameObject bestScoreStuff;

        public TextMeshProUGUI[] scoreText;
        public TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI runSummaryText;
        [SerializeField] private TextMeshProUGUI coinRewardText;
        [SerializeField] private Image coinRewardIcon;
        protected BaseModeHandler modeHandler;

        protected override void OnEnable()
        {
            base.OnEnable();
            modeHandler = FindObjectOfType<BaseModeHandler>(false);
            var level = FindObjectOfType<LevelManager>();
            var score = level.RunScore;
            var bestScore = modeHandler.bestScore;
            scoreText[0].text = score.ToString();
            scoreText[1].text = score.ToString();
            bestScoreText.text = Mathf.Max(score, bestScore).ToString();
            if (level.IsEndlessMode)
            {
                var reward = level.RewardSummary;
                var coinEnd = reward.IndexOf(" COINS", StringComparison.Ordinal);
                var hasCoins = reward.StartsWith("+", StringComparison.Ordinal) && coinEnd > 1;
                coinRewardIcon.gameObject.SetActive(hasCoins);
                runSummaryText.text = $"HIGHEST COMBO  {level.HighestCombo}";
                coinRewardText.text = hasCoins ? reward.Substring(0, coinEnd) : reward;
                coinRewardText.rectTransform.anchoredPosition = new Vector2(hasCoins ? 45 : 0, -265);
            }
            else
            {
                coinRewardIcon.gameObject.SetActive(false);
                coinRewardText.text = string.Empty;
                runSummaryText.text = string.Empty;
            }
            if (modeHandler is EndlessModeHandler endless && endless.NewBestThisRun)
            {
                bestScoreStuff.SetActive(true);
                failedStuff.SetActive(false);
            }
            else
            {
                failedStuff.SetActive(true);
                bestScoreStuff.SetActive(false);
            }
        }
    }
}
