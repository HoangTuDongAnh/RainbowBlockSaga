using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.GUI.Labels;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using UnityEngine;
using System.Collections;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PreWinScore : PreWin
    {
        public TargetScriptable scoreTarget;
        public TargetScoreGUIElement scoreSlider;
        private bool animationCompleted = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            var targetManager = FindObjectOfType<TargetManager>();
            if(targetManager!=null)
            {
                if (!targetManager.GetTargetGuiElements().TryGetValue(scoreTarget, out var targetGuiElement))
                {
                    scoreSlider.UpdateCount(0, false);
                    return;
                }
                // Get the final score value
                if (!int.TryParse(targetGuiElement.countText.text, out int finalScore))
                {
                    return;
                }
                // Set up the total score and animate
                scoreSlider.totalText.text = finalScore.ToString();
                scoreSlider.scoreSlider.maxValue = finalScore;
                scoreSlider.scoreSlider.value = 0;
                scoreSlider.duration = 1f;
                animationCompleted = false;

                StartCoroutine(AnimateWithDelay(finalScore));
            }

        }

        private IEnumerator AnimateWithDelay(int finalScore)
        {
            yield return new WaitForSeconds(.3f);
            
            scoreSlider.UpdateCount(finalScore, true);
            
            float startTime = Time.time;
            float elapsedTime = 0f;
            
            // Wait until the slider animation is complete or timeout
            while (Mathf.Abs(scoreSlider.scoreSlider.value - finalScore) > 0.01f && elapsedTime < scoreSlider.duration * 1.5f)
            {
                elapsedTime = Time.time - startTime;
                yield return null;
            }

            // Force the final value if we timed out
            if (Mathf.Abs(scoreSlider.scoreSlider.value - finalScore) > 0.01f)
            {
                scoreSlider.scoreSlider.value = finalScore;
            }
            
            animationCompleted = true;
            yield return new WaitForSeconds(.1f);
            base.AfterShowAnimation();
            yield return new WaitForSeconds(.5f);
            Close();

        }

        public override void AfterShowAnimation()
        {
        }
    }
}