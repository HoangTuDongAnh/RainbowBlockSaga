using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Data;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Shop/BooterShopSettings")]
public class BooterShopSettings : ScriptableObject
{
    [System.Serializable]
    public class BoosterShopItem
    {
        public ResourceObject boosterObject;  // ví dụ: ShuffleItem, BombItem...
        public int priceCoins;                // giá coin để mua
        public int amount;                    // số lượng booster nhận được sau khi mua
    }

    public List<BoosterShopItem> boosterItems = new();
}
