using BlockPuzzleGameToolkit.Scripts.GUI;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// Presentation-only popup scaffold.
    /// The actual save/continue decision flow will be wired after Phase B is complete.
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
