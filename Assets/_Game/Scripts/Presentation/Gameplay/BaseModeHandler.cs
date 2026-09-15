using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.System;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using TMPro;
using UnityEngine;
using System.Collections;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public abstract class BaseModeHandler : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI bestScoreText;

        [HideInInspector]
        public int bestScore;

        [HideInInspector]
        public int score;

        protected LevelManager _levelManager;
        protected Coroutine _counterCoroutine;
        protected int _displayedScore = 0;
        [SerializeField]
        protected float counterSpeed = 0.01f;

        protected virtual void OnEnable()
        {
            _levelManager = FindObjectOfType<LevelManager>(true);
            
            if (_levelManager != null)
            {
                _levelManager.OnLose += OnLose;
                _levelManager.OnScored += OnScored;
            }

            LoadScores();
            _displayedScore = score;
        }

        protected virtual void OnDisable()
        {
            if (_counterCoroutine != null) StopCoroutine(_counterCoroutine);
            _counterCoroutine = null;
            if (_levelManager != null)
            {
                _levelManager.OnLose -= OnLose;
                _levelManager.OnScored -= OnScored;
            }
        }

        protected virtual void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveCurrentRun();
        }

        protected virtual void OnApplicationQuit()
        {
            SaveCurrentRun();
        }

        public void SaveCurrentRun()
        {
            if (!GameDataManager.isTestPlay && isActiveAndEnabled &&
                (EventManager.GameStatus == EGameState.Playing || EventManager.GameStatus == EGameState.Paused))
                SaveGameState();
        }

        public virtual void OnScored(int scoreToAdd)
        {
            int previousScore = _displayedScore;
            this.score += scoreToAdd;

            // Update UI immediately
            scoreText.text = score.ToString();

            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
            }
            _counterCoroutine = StartCoroutine(CountScore(previousScore, this.score));
        }

        protected IEnumerator CountScore(int startValue, int endValue)
        {
            _displayedScore = startValue;

            float duration = Mathf.Clamp(Mathf.Abs(endValue - startValue) * counterSpeed, .2f, .8f);
            for (float elapsed = 0; elapsed < duration; elapsed += Time.deltaTime)
            {
                _displayedScore = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, elapsed / duration));
                scoreText.text = _displayedScore.ToString();
                yield return null;
            }

            _displayedScore = endValue;
            scoreText.text = endValue.ToString();
        }

        public virtual void OnLose()
        {
            DeleteGameState();
        }

        public virtual void UpdateScore(int newScore)
        {
            int previousScore = _displayedScore;
            this.score = newScore;
            
            // Update UI immediately
            scoreText.text = score.ToString();
            
            // Animate the change
            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
            }
            _counterCoroutine = StartCoroutine(CountScore(previousScore, this.score));
        }

        public virtual void ResetScore()
        {
            // Stop any ongoing score animation
            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
                _counterCoroutine = null;
            }

            // Reset score and displayed score
            score = 0;
            _displayedScore = 0;

            // Update UI
            scoreText.text = "0";

            // Delete the game state since we're resetting
            DeleteGameState();
        }

        protected abstract void LoadScores();
        protected abstract void SaveGameState();
        protected abstract void DeleteGameState();
    }
}
