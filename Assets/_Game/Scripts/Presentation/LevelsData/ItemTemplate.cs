using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    [CreateAssetMenu(fileName = "ItemTemplate", menuName = "Rainbow Blocks Saga/Items/ItemTemplate", order = 1)]
    public class ItemTemplate : ScriptableData
    {
        public Color backgroundColor;
        public Color underlayColor;
        public Color bottomColor;
        public Color topColor;
        public Color leftColor;
        public Color rightColor;
        public Color overlayColor;

        public Sprite backgroundSprite;
        public Sprite underlaySprite;
        public Sprite bottomSprite;
        public Sprite topSprite;
        public Sprite leftSprite;
        public Sprite rightSprite;
        public Sprite overlaySprite;

        public bool[] colorEnable = new bool[7] { true, true, true, true, true, true, true };

        public Item customItemPrefab;

        public bool HasCustomPrefab() => customItemPrefab != null;
    }
}