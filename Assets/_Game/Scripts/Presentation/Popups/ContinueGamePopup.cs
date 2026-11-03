using RainbowBlockSaga.Presentation.Scripts.GUI;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    /// <summary>
    /// Lets the player continue an unfinished run or discard it and start fresh.
    /// </summary>
    public class ContinueGamePopup : Popup
    {
        [SerializeField] CustomButton continueButton;
        [SerializeField] CustomButton newGameButton;
        [SerializeField] Button backButton;

        protected override void Awake()
        {
            base.Awake();

            continueButton.onClick.AddListener(ContinueGame);
            newGameButton.onClick.AddListener(NewGame);
            backButton.onClick.AddListener(Close);
        }

        void ContinueGame()
        {
            result = EPopupResult.Continue;
            Close();
        }

        void NewGame()
        {
            result = EPopupResult.Restart;
            Close();
        }

        public override void Close()
        {
            // Back/Escape leaves the saved run untouched on the main menu.
            if (result != EPopupResult.Continue && result != EPopupResult.Restart)
                result = EPopupResult.Cancel;
            base.Close();
        }
    }
}
