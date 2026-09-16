using TMPro;
using DG.Tweening;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public class ScoreText : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;

        public void ShowScore(int value, Vector3 transformPosition)
        {
            // Ignore the transform position and use the center field position
            scoreText.transform.position = transformPosition;
            scoreText.text = "+" + value;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.alpha = 1f;
            scoreText.transform.DOKill();
            scoreText.transform.localScale = Vector3.one * .72f;
            scoreText.transform.DOScale(1.18f, .16f).SetEase(Ease.OutBack)
                .OnComplete(() => scoreText.transform.DOScale(1f, .24f).SetEase(Ease.OutQuad));
            scoreText.DOColor(new Color(1f, .92f, .35f), .12f)
                .OnComplete(() => scoreText.DOColor(Color.white, .22f));
        }

        void OnDisable()
        {
            if (scoreText != null) scoreText.transform.DOKill();
        }
    }
}
