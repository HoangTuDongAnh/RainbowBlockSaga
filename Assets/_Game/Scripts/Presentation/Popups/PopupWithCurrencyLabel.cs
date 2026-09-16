using RainbowBlockSaga.Presentation.Scripts.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PopupWithCurrencyLabel : Popup
    {
        private UIPanel _panel;

        public override void AfterShowAnimation()
        {
            _panel = FindObjectOfType<UIPanel>(true);
            _panel?.gameObject.SetActive(true);
            base.AfterShowAnimation();
        }

        public override void Close()
        {
            base.Close();
            if (SceneManager.GetActiveScene().name != "HexagonPuzzle")
            {
                _panel?.gameObject.SetActive(false);
            }
        }

        protected void ShowCoinsSpendFX(Vector3 position)
        {
            SoundBase.instance.PlaySound(SoundBase.instance.coinsSpend);
            var fx = Instantiate(Resources.Load<GameObject>("FX/CoinsSpendFX"), position, Quaternion.identity, transform.parent);
            fx.transform.localScale = Vector3.one;
        }
    }
}