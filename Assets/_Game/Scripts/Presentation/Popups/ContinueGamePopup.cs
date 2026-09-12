using RainbowBlockSaga.Presentation.Scripts.GUI;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    /// <summary>
    /// Lets the player continue an unfinished run or discard it and start fresh.
    /// </summary>
    public class ContinueGamePopup : Popup
    {
        [SerializeField] CustomButton continueButton;
        [SerializeField] CustomButton newGameButton;

        protected override void Awake()
        {
            base.Awake();

            continueButton.onClick.AddListener(ContinueGame);
            newGameButton.onClick.AddListener(NewGame);
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
    }
}
