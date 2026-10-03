using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class FailedEndless : Failed
    {
        public GameObject failedStuff;
        public GameObject bestScoreStuff;

        public TextMeshProUGUI[] scoreText;
        public TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI runSummaryText;
        protected BaseModeHandler modeHandler;

        protected override void OnEnable()
        {
            base.OnEnable();
            modeHandler = FindObjectOfType<BaseModeHandler>(false);
            var score = modeHandler.score;
            var bestScore = modeHandler.bestScore;
            scoreText[0].text = score.ToString();
            scoreText[1].text = score.ToString();
            bestScoreText.text = Mathf.Max(score, bestScore).ToString();
            var level = FindObjectOfType<LevelManager>();
            if (level.IsEndlessMode)
                runSummaryText.text = $"HIGHEST COMBO  {level.HighestCombo}\n{level.RewardSummary}";
            if (score > bestScore)
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
