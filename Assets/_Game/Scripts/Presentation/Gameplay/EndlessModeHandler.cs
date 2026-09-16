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

            // Load current score from game state using the proper mode-specific loading
            var state = GameDataManager.isTestPlay ? null : GameState.Load(EGameMode.Endless) as EndlessGameState;
            if (state != null)
            {
                score = state.score;
                bestScore = Mathf.Max(bestScore, state.bestScore);
                bestScoreText.text = bestScore.ToString();
                scoreText.text = score.ToString();
            }
            else
            {
                score = 0;
                scoreText.text = "0";
            }
        }

        protected override void SaveGameState()
        {
            var fieldManager = _levelManager?.GetFieldManager();
            if (fieldManager != null)
            {
                var state = new EndlessGameState
                {
                    score = score,
                    bestScore = bestScore,
                    gameMode = EGameMode.Endless,
                    gameStatus = EventManager.GameStatus
                };
                GameState.Save(state, fieldManager);
            }
        }

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
