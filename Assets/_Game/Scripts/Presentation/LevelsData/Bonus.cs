using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    public class Bonus : FillAndPreview
    {
        public Image image;
        //[HideInInspector]
        public BonusItemTemplate bonusItemTemplate;

        [SerializeField]
        private int _side = 62;

        public override void FillIcon(ScriptableData iconScriptable)
        {
            UpdateColor((BonusItemTemplate)iconScriptable);
        }

        private void UpdateColor(BonusItemTemplate bonusItemTemplate)
        {
            this.bonusItemTemplate = bonusItemTemplate;
            image.sprite = bonusItemTemplate.sprite;
            image.SetNativeSize();
            if (image.rectTransform.sizeDelta.x > _side || image.rectTransform.sizeDelta.y > _side)
            {
                if (image.rectTransform.sizeDelta.x > image.rectTransform.sizeDelta.y)
                {
                    image.rectTransform.sizeDelta = new Vector2(_side, _side * image.rectTransform.sizeDelta.y / image.rectTransform.sizeDelta.x);
                }
                else
                {
                    image.rectTransform.sizeDelta = new Vector2(_side * image.rectTransform.sizeDelta.x / image.rectTransform.sizeDelta.y, _side);
                }
            }
        }

        public void SetTransparency(float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}