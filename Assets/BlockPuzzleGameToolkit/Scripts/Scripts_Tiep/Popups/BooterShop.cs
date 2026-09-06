using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI.Labels;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.System;
using System.Linq;
using UnityEngine;

public class BooterShop : PopupWithCurrencyLabel
{
    public BoosterPurchaseItem[] items;
    public LabelAnim labelAnim;
    public TabPanel tabPanel;
    private BooterShopSettings settings;
    private ResourceObject coinsResource;

    private void OnEnable()
    {
        settings = Resources.Load<BooterShopSettings>("Settings/BooterShopSettings");
        coinsResource = ResourceManager.instance.GetResource("Coins");

        for (int i = 0; i < items.Length; i++)
        {
            if (i < settings.boosterItems.Count)
            {
                items[i].data = settings.boosterItems[i];
            }
        }
    }

    public void BuyBooster(BooterShopSettings.BoosterShopItem pack)
    {
        if (!ResourceManager.instance.Consume("Coins", pack.priceCoins))
        {
            tabPanel.ActivateTab(tabPanel.tabs.First(t => t.tabName == "Coins"));

            return;
        }
           
            pack.boosterObject.Add(pack.amount);

            // call animation

            var itemUI = System.Array.Find(items, i => i.data == pack);
            Vector3 endPosition = itemUI.priceText.transform.position;
            labelAnim?.AnimateFree(
            startPos: labelAnim.icon.transform.position,
            endPos: endPosition,
            popupTextPos: itemUI.buyButton.transform.position,
            "+"+pack.amount.ToString(),
            sound: pack.boosterObject.sound,
            callback: null
            );


    }
    public override void Close()
    {
        base.Close();
        if (StateManager.instance.CurrentState == EScreenStates.Game)
        {
            EventManager.GameStatus = EGameState.Playing;
        }
    }
}
