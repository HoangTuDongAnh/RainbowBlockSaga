using RainbowBlockSaga.Presentation.Scripts.GUI;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Quit : PopupWithCurrencyLabel
    {
        public CustomButton yes;

        private void OnEnable()
        {
            yes.onClick.AddListener(Application.Quit);
        }
    }
}