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

        protected override void LoadScores()
        {
            // Load best score from resources
            bestScore = ResourceManager.instance.GetResource("Score").GetValue();
            bestScoreText.text = bestScore.ToString();

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
            bestScore = ResourceManager.instance.GetResource("Score").GetValue();
            if (score > bestScore)
            {
                ResourceManager.instance.GetResource("Score").Set(score);
            }

            base.OnLose();
        }
    }
}
