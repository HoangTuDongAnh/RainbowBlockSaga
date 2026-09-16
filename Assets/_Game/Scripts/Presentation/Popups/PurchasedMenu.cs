using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PurchasedMenu : Popup
    {
        [SerializeField]
        private Transform icon;

        [SerializeField]
        private TextMeshProUGUI text;

        public void SetIconSprite(GameObject imagePrefab, string boostName)
        {
            Instantiate(imagePrefab, icon);
            text.text = "You got " + boostName + " boost";
        }
    }
}