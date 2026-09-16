using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using TMPro;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PrePlayScore : PrePlay
    {
        public TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
            {
                scoreText.text = levelManager.GetCurrentLevel().targetInstance[0].amount.ToString();
            }

        }
    }
}