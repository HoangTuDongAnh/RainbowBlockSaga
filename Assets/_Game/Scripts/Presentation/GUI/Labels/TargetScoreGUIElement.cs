using System.Collections.Generic;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.GUI.Labels
{
    public class TargetScoreGUIElement : TargetGUIElement
    {
        public Transform circleBack;
        public Slider scoreSlider;
        public TextMeshProUGUI totalText;
        private TargetManager targetInstance;
        public float duration = 0.5f;
        private Tween currentTween;

        private void OnEnable()
        {
            scoreSlider.onValueChanged.AddListener(UpdateScoreText);
            
            // Check if this element is in a popup by looking for PreWinScore component in parent hierarchy
            var preWinScore = GetComponentInParent<RainbowBlockSaga.Presentation.Scripts.Popups.PreWinScore>();
            if (preWinScore != null)
            {
                return;
            }
            
            targetInstance = FindObjectOfType<TargetManager>(true);
            if (targetInstance != null)
            {
                var targets = targetInstance.GetTargets();
                if (targets != null)
                {
                    Init(targets);
                }
            }

        }


        private void Init(List<Target> obj)
        {
            foreach (var target in obj)
            {
                if (target.amount == 0)
                {
                    continue;
                }

                if (target.targetScriptable.GetType() == typeof(ScoreTargetScriptable))
                {
                    if (target.targetScriptable == null) return;
                    targetInstance?.RegisterTargetGuiElement(target.targetScriptable, this);
                    totalText.text = target.totalAmount.ToString();
                    SetupScoreSlider(target.amount);
                    return;
                }
            }
        }

        private void SetupScoreSlider(int maxValue)
        {
            scoreSlider.maxValue = maxValue;
            scoreSlider.value = 0;
        }

        public override void UpdateCount(int newCount, bool isTargetCompleted)
        {
            currentTween?.Kill();
            float targetValue = Mathf.Clamp(newCount, 0, scoreSlider.maxValue);
            currentTween = scoreSlider.DOValue(targetValue, duration)
                .SetEase(Ease.InOutQuad);
        }

        private void UpdateScoreText(float value)
        {
            countText.text = value.ToString("0");

            var handler = scoreSlider.handleRect;
            if (handler != null)
            {
                circleBack.position = handler.position;
            }
        }

        private void OnDisable()
        {
            currentTween?.Kill();
            scoreSlider.onValueChanged.RemoveListener(UpdateScoreText);
        }
    }
}
