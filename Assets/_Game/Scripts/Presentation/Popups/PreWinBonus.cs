using RainbowBlockSaga.Presentation.Scripts.GUI;
using DG.Tweening;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PreWinBonus : PreWin
    {
        [SerializeField] private TargetPanelInPopup targetPanel;

        public override void AfterShowAnimation()
        {
            targetPanel.AnimateTargets();
            targetPanel.OnAnimationComplete += OnAnimationComplete;
        }

        private void OnAnimationComplete()
        {
            targetPanel.OnAnimationComplete -= OnAnimationComplete;
            base.AfterShowAnimation();
        }

    }
    
}