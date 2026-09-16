using RainbowBlockSaga.Presentation.Scripts.GUI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Confirmation : Popup
    {
        public CustomButton yesButton;

        private void OnEnable()
        {
            yesButton.onClick.AddListener(Yes);
            closeButton.onClick.AddListener(No);
        }

        private void No()
        {
            StopInteration();

            result = EPopupResult.No;
            Close();
        }

        private void Yes()
        {
            StopInteration();

            result = EPopupResult.Yes;
            Close();
        }
    }
}