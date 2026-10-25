using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public class EndlessModeHandler : BaseModeHandler
    {
        public Image rhombusImage;
        public bool NewBestThisRun { get; private set; }

        protected override void LoadScores()
        {
            // Load best score from resources
            bestScore = ResourceManager.instance.GetResource("Score").GetValue();
            bestScoreText.text = bestScore.ToString();
            NewBestThisRun = false;

            score = _levelManager.RunScore;
            scoreText.text = score.ToString();
        }

        protected override void SaveGameState() => _levelManager.SaveRun();

        protected override void DeleteGameState()
        {
            GameState.Delete(EGameMode.Endless);
        }

        public override void OnLose()
        {
            if (GameDataManager.isTestPlay) return;
            score = _levelManager.RunScore;
            bestScore = ResourceManager.instance.GetResource("Score").GetValue();
            NewBestThisRun = score > bestScore;
            if (NewBestThisRun)
            {
                ResourceManager.instance.GetResource("Score").Set(score);
                bestScore = score;
            }
            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
                _counterCoroutine = null;
            }
            _displayedScore = score;
            scoreText.text = score.ToString();
            bestScoreText.text = bestScore.ToString();

            base.OnLose();
        }
    }
}
