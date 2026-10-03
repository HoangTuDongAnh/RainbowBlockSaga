using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using RainbowBlockSaga.Presentation.Scripts.System;
using DG.Tweening;
using UnityEngine;
using System;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetPanelInPopup : TargetPanelBase
    {
        private Popup _popup;
        [SerializeField] private bool animate = true;

        protected override void OnEnableInternal()
        {
            _popup = GetComponentInParent<Popup>();
            if (_popup != null)
            {
                ShowTargets();
                _popup.OnShowAction += AnimateTargets;
            }
        }

        protected override void OnDisableInternal()
        {
            if (_popup != null)
            {
                _popup.OnShowAction -= AnimateTargets;
            }
        }

        private void ShowTargets()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
            var targets = targetManager?.GetTargetGuiElements();
            if (targets != null)
            {
                foreach (var target in targets)
                {
                    // Debug.Log("Target in popup: " + target.Key.name + " Amount: " + target.Value);
                    var targetElement = Instantiate(target.Value, transform);
                    targetElement.transform.localScale = animate ? Vector3.zero : Vector3.one * 1.5f;
                    if (EventManager.GameStatus == EGameState.PreWin || EventManager.GameStatus == EGameState.Win)
                    {
                        if (targetElement is TargetBonusGUIElement bonus) bonus.TargetCheck();
                    }
                }
            }
        }

        public void AnimateTargets()
        {
            if (!animate) return;
            
            float delay = 0f;
            var childCount = transform.childCount;
            var currentChild = 0;
            
            foreach (Transform child in transform)
            {
                currentChild++;
                var sequence = DOTween.Sequence();
                sequence.Append(child.DOScale(Vector3.one * 1.5f, 0.1f).SetEase(Ease.OutBack).SetDelay(delay));
                
                if (currentChild == childCount)
                {
                    sequence.OnComplete(() => OnAnimationComplete?.Invoke());
                }
                
                delay += 0.01f;
            }
        }

        public Action OnAnimationComplete;
    }
} 
