using BlockPuzzleGameToolkit.Scripts.GUI;
using TMPro;
using UnityEngine;

public class BoosterPurchaseItem : MonoBehaviour
{
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI amountText;
    public CustomButton buyButton;

    [HideInInspector] public BooterShopSettings.BoosterShopItem data;

    private void Start()
    {
        if (data != null)
        {
            priceText.text = data.priceCoins.ToString();
            amountText.text = data.amount.ToString();

            buyButton.onClick.AddListener(BuyBooster);
        }
    }

    private void BuyBooster()
    {
        GetComponentInParent<BooterShop>().BuyBooster(data);
    }
}
