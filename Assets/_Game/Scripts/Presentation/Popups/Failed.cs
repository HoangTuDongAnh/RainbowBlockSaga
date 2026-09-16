using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.System;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Failed : Popup
    {
        public CustomButton retryButton;

        protected virtual void OnEnable()
        {
            retryButton.onClick.AddListener(Retry);
            closeButton.onClick.AddListener(() => GameManager.instance.MainMenu());
        }

        private void Retry()
        {
            StopInteration();

            GameManager.instance.RestartLevel();
            Close();
        }
    }
}