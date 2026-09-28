using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RainbowBlockSaga.Presentation.Scripts.GUI;

namespace RainbowBlockSaga.Presentation.Gameplay
{
    public sealed class EndlessSupportButton : MonoBehaviour
    {
        [SerializeField] CustomButton button;
        [SerializeField] Image icon;
        [SerializeField] TMP_Text countLabel;
        [SerializeField] TMP_Text priceLabel;
        [SerializeField] TMP_Text nameLabel;

        public EndlessSupportItem Item;
        public CustomButton Button => button;
        int purchasePrice;

        public void Configure(Sprite sprite, string displayName, int price)
        {
            purchasePrice = price;
            icon.sprite = sprite;
            nameLabel.text = displayName;
        }

        public void SetCount(int value)
        {
            countLabel.text = value.ToString();
            priceLabel.text = value > 0 ? "USE" : purchasePrice.ToString();
        }

        public void SetSelected(bool selected)
        {
            icon.transform.localScale = Vector3.one * (selected ? 1.08f : 1f);
            nameLabel.gameObject.SetActive(selected);
            nameLabel.text = "TAP A CELL";
        }

        public void SetInteractable(bool value)
        {
            button.interactable = value;
        }
    }
}
