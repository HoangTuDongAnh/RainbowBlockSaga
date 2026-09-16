using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using TMPro;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class WinScore : Win
    {
        public TextMeshProUGUI scoreText;
        public TargetScriptable scoreTarget;

        private void Start()
        {
            var targetManager = FindObjectOfType<TargetManager>();
            scoreText.text = targetManager.GetTargetGuiElements().TryGetValue(scoreTarget, out var targetGuiElement)
                ? targetGuiElement.countText.text
                : "0";
        }
    }
}