using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetBonusGUIElement : TargetGUIElement
    {
        public Bonus bonus;
        public GameObject check;
        private bool scoreTarget;
        private int total;

        public void FillElement(BonusItemTemplate bonusItemTemplate, int targetAmount)
        {
            scoreTarget = bonusItemTemplate == null;
            total = targetAmount;
            bonus.gameObject.SetActive(!scoreTarget);
            if (!scoreTarget) bonus.FillIcon(bonusItemTemplate);
            countText.text = targetAmount.ToString();
        }

        public override void UpdateCount(int newCount, bool isTargetCompleted)
        {
            countText.text = scoreTarget ? $"SCORE\n{newCount}/{total}" : newCount.ToString();
            countText.gameObject.SetActive(!isTargetCompleted);
            check.SetActive(isTargetCompleted);
            if (isTargetCompleted)
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
