using System.Collections;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(CanvasGroup))]
    public sealed class LogoIntroPlayback : MonoBehaviour
    {
        private static readonly int Appearance = Animator.StringToHash("Base Layer.rbs-logo-appearance");
        [SerializeField, Min(0f), Tooltip("Extra wait in seconds after the two initialization frames.")]
        private float initialDelay = 0.3f;

        [SerializeField, Range(0.25f, 1.5f), Tooltip("Intro speed only. Lower values make each letter easier to follow; idle stays at normal speed.")]
        private float introPlaybackSpeed = 0.7f;

        private Animator logoAnimator;
        private CanvasGroup visibility;
        private Coroutine startRoutine;

        private void Awake()
        {
            logoAnimator = GetComponent<Animator>();
            visibility = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            // Scene initialization can produce a large first unscaled delta and skip the intro.
            logoAnimator.enabled = false;
            visibility.alpha = 0;
            startRoutine = StartCoroutine(BeginAfterInitialization());
        }

        private IEnumerator BeginAfterInitialization()
        {
            yield return null;
            yield return null;
            if (initialDelay > 0)
                yield return new WaitForSecondsRealtime(initialDelay);
            logoAnimator.enabled = true;
            logoAnimator.Rebind();
            logoAnimator.speed = Mathf.Clamp(introPlaybackSpeed, 0.25f, 1.5f);
            logoAnimator.Play(Appearance, 0, 0);
            logoAnimator.Update(0);
            visibility.alpha = 1;
            while (logoAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == Appearance &&
                   logoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
            logoAnimator.speed = 1;
            startRoutine = null;
        }

        private void OnDisable()
        {
            if (startRoutine != null)
            {
                StopCoroutine(startRoutine);
                startRoutine = null;
            }
            logoAnimator.enabled = false;
            logoAnimator.speed = 1;
            visibility.alpha = 0;
        }
    }
}
