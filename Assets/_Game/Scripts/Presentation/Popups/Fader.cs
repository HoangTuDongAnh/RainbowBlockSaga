using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Fader : MonoBehaviour
    {
        public Image fader;
        private readonly float defaultFadeTime = .1f;
        private readonly float maxValue = .997f;

        public bool IsFaded()
        {
            return fader.color.a >= maxValue;
        }

        public void FadeIn(float fadeAlpha, Action action = null)
        {
            FadeIn(fadeAlpha, defaultFadeTime, action);
        }

        public void FadeIn(float fadeAlpha, float duration, Action action = null)
        {
            fader.gameObject.SetActive(true);
            fader.DOFade(fadeAlpha, duration).OnComplete(() => action?.Invoke());
        }

        public void FadeOut()
        {
            FadeOut(defaultFadeTime);
        }

        public void FadeOut(float duration)
        {
            fader.DOFade(0, duration).OnComplete(() => fader.gameObject.SetActive(false));
        }

        public void FadeAfterLoadingScene()
        {
            FadeOut();
        }
    }
}