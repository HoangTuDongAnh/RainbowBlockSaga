using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class FailedClassic : Failed
    {
        public GameObject failedStuff;
        public GameObject bestScoreStuff;

        public TextMeshProUGUI[] scoreText;
        public TextMeshProUGUI bestScoreText;
        protected BaseModeHandler modeHandler;

        protected override void OnEnable()
        {
            base.OnEnable();
            modeHandler = FindObjectOfType<BaseModeHandler>(false);
            var score = modeHandler.score;
            var bestScore = modeHandler.bestScore;
            scoreText[0].text = score.ToString();
            scoreText[1].text = score.ToString();
            bestScoreText.text = bestScore.ToString();
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