using System;
using System.Collections;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.Popups.Daily;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class MainMenu : Popup
    {
        public CustomButton endlessMode;
        public CustomButton arcadeMode;
        public CustomButton settingsButton;
        public CustomButton luckySpin;
        [SerializeField] private CustomButton dailyReward;
        public GameObject playObject;
        [SerializeField] private RectTransform homeCoin;
        public RectTransform HomeCoin => homeCoin;

        [SerializeField]
        private GameObject freeSpinMarker;

        [SerializeField]
        private Image background;

        public Action OnAnimationEnded;

        private const string LastFreeSpinTimeKey = "LastFreeSpinTime";

        private void OnEnable()
        {
            UpdateFreeSpinMarker();
            StartCoroutine(RevealDailyReward());
        }

        private IEnumerator RevealDailyReward()
        {
            // The press animator owns scale. Fade independently from the button animation.
            var daily = dailyReward.GetComponent<CanvasGroup>();
            daily.alpha = 0f;
            daily.blocksRaycasts = false;
            var spin = luckySpin.GetComponent<CanvasGroup>();
            if (GameManager.instance.GameSettings.enableLuckySpin)
            {
                yield return FadeIn(spin);
                yield return new WaitForSecondsRealtime(.12f);
            }
            yield return FadeIn(daily);
        }

        private static IEnumerator FadeIn(CanvasGroup group)
        {
            group.blocksRaycasts = false;
            for (float elapsed = 0f; elapsed < .32f; elapsed += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.SmoothStep(0f, 1f, elapsed / .32f);
                yield return null;
            }
            group.alpha = 1f;
            group.blocksRaycasts = true;
        }

        private void OnDestroy()
        {
            LuckySpin.FreeSpinUsed -= UpdateFreeSpinMarker;
        }

        private void Start()
        {
            LuckySpin.FreeSpinUsed += UpdateFreeSpinMarker;
            endlessMode.onClick.AddListener(PlayEndlessMode);
            arcadeMode.onClick.AddListener(PlayArcadeMode);
            settingsButton.onClick.AddListener(SettingsButtonClicked);
            luckySpin.onClick.AddListener(LuckySpinButtonClicked);
            dailyReward.onClick.AddListener(() => MenuManager.instance.ShowPopup<DailyBonus>());
            dailyReward.gameObject.SetActive(GameManager.instance.dailyBonusSettings.dailyBonusEnabled);
            UpdateFreeSpinMarker();
            GameDataManager.LevelNum = PlayerPrefs.GetInt("Level", 1);
            luckySpin.gameObject.SetActive(GameManager.instance.GameSettings.enableLuckySpin);
        }

        private bool CanUseFreeSpinToday() => RewardClock.CanClaimToday(LastFreeSpinTimeKey);

        private void UpdateFreeSpinMarker()
        {
            var isFreeSpinAvailable = CanUseFreeSpinToday();
            freeSpinMarker.SetActive(isFreeSpinAvailable);
        }

        private void PlayEndlessMode()
        {
            // Keep the existing mode ID so Endless resumes saved Endless games.
            PlayModeWithResumeCheck(EGameMode.Endless);
        }

        private void PlayArcadeMode()
        {
            GameManager.instance.SetGameMode(EGameMode.Adventure);
            var saved = RunSnapshot.Load(EGameMode.Adventure);
            if (saved != null)
            {
                MenuManager.instance.ShowPopup<ContinueGamePopup>(null, result =>
                {
                    if (result == EPopupResult.Cancel) return;
                    if (result == EPopupResult.Restart)
                    {
                        RunSnapshot.Delete(EGameMode.Adventure);
                        GameManager.instance.OpenMap();
                    }
                    else SceneLoader.instance.StartGameScene(saved.level);
                });
                return;
            }
            GameManager.instance.OpenMap();
        }

        private void PlayModeWithResumeCheck(EGameMode mode)
        {
            GameManager.instance.SetGameMode(mode);

            if (RunSnapshot.Load(mode) == null)
            {
                GameManager.instance.OpenMap();
                return;
            }

            MenuManager.instance.ShowPopup<ContinueGamePopup>(
                null,
                result =>
                {
                    if (result == EPopupResult.Cancel) return;
                    if (result == EPopupResult.Restart)
                    {
                        GameState.Delete(mode);
                        RunSnapshot.Delete(mode);
                    }

                    GameManager.instance.OpenMap();
                });
        }

        private void SettingsButtonClicked()
        {
            MenuManager.instance.ShowPopup<Settings>();
        }

        private void LuckySpinButtonClicked()
        {
            MenuManager.instance.ShowPopup<LuckySpin>(null, _ => UpdateFreeSpinMarker());
        }

        public void OnAnimationEnd(){
            OnAnimationEnded?.Invoke();
        }
    }
}
