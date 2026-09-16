using RainbowBlockSaga.Presentation.Scripts.Audio;
using RainbowBlockSaga.Presentation.Scripts.Data;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.System;
using DG.Tweening;
using TMPro;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PreFailed : PopupWithCurrencyLabel
    {
        public TextMeshProUGUI continuePrice;
        public TextMeshProUGUI timerText;
        public CustomButton continueButton;
        public CustomButton rewardButton;
        public TextMeshProUGUI timeLeftText;
        protected int timer;
        protected int price;
        protected bool hasContinued = false;

        protected virtual void OnEnable()
        {
            price = GameManager.instance.GameSettings.continuePrice;
            continuePrice.text = price.ToString();
            continueButton.onClick.AddListener(Continue);
            
            InitializeTimer();
            
            timerText.text = timer.ToString();
            SoundBase.instance.PlaySound(SoundBase.instance.warningTime);
            rewardButton?.gameObject.SetActive(false);
            if(GameDataManager.GetLevel().enableTimer && timeLeftText != null)
            {
                timeLeftText.gameObject.SetActive(true);
            }
        }

        protected virtual void InitializeTimer()
        {
            timer = GameManager.instance.GameSettings.failedTimerStart;
        }

        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            // Start the timer only after the popup animation is complete
            InvokeRepeating(nameof(UpdateTimer), 1, 1);
        }

        protected virtual void UpdateTimer()
        {
            // Only decrement timer if this popup is active and not already expired
            if (MenuManager.instance.GetLastPopup() == this && timer > 0)
            {
                timer--;
                SaveTimerState();
            }

            timerText.text = timer.ToString();
            if (timer <= 0)
            {
                continueButton.interactable = false;
                if (rewardButton != null) rewardButton.interactable = false;
                hasContinued = true;

                CancelInvoke(nameof(UpdateTimer));
                EventManager.GameStatus = EGameState.Failed;
                Close();
            }
        }

        protected virtual void SaveTimerState() { }

        public void PauseTimer()
        {
            CancelInvoke(nameof(UpdateTimer));
        }

        protected virtual void Continue()
        {
            if (timer <= 0 || hasContinued)
            {
                return;
            }

            var coinsResource = ResourceManager.instance.GetResource("Coins");
            if (coinsResource.Consume(price))
            {
                hasContinued = true;
                continueButton.interactable = false;
                if (rewardButton != null) rewardButton.interactable = false;
                                
                CancelInvoke(nameof(UpdateTimer));
                ShowCoinsSpendFX(continueButton.transform.position);
                StopInteration();
                OnContinue();
            }
        }

        public void OnContinue()
        {
            DOTween.Kill(this);
            DOVirtual.DelayedCall(0.5f, ContinueGame);
        }

        public void ContinueGame()
        {
            result = EPopupResult.Continue;
            EventManager.GameStatus = EGameState.Playing;
            Close();
        }
    }
}