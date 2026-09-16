using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetBonusGUIElement : TargetGUIElement
    {
        public Bonus bonus;
        public GameObject check;

        public void FillElement(BonusItemTemplate bonusItemTemplate, int targetAmount)
        {
            bonus.FillIcon(bonusItemTemplate);
            countText.text = targetAmount.ToString();
        }

        public override void UpdateCount(int newCount, bool isTargetCompleted)
        {
            base.UpdateCount(newCount, isTargetCompleted);
            if (isTargetCompleted || newCount == 0)
            {
                TargetCheck();
            }
        }

        public void TargetCheck()
        {
            countText.gameObject.SetActive(false);
            check.SetActive(true);
        }
    }
}