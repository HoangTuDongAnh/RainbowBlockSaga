using System;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class Settings : PopupWithCurrencyLabel
    {
        [SerializeField]
        private CustomButton back;

        // privacypolicy button
        [SerializeField]
        private CustomButton privacypolicy;

        //shop button
        [SerializeField]
        private CustomButton shop;
        [SerializeField]
        private CustomButton store;

        [SerializeField]
        private CustomButton retryButton;

        [SerializeField]
        private Button restorePurchase;

        [SerializeField]
        private Slider vibrationSlider;

        private MenuManager menuManager;

        private const string VibrationPrefKey = "VibrationLevel";

        private void OnEnable()
        {
            back.onClick.AddListener(BackToMain);
          //  store.onClick.AddListener(Store);
            retryButton.onClick.AddListener(Retry);

            // Load the saved vibration level
            LoadVibrationLevel();

            // Register the OnValueChanged event
            vibrationSlider.onValueChanged.AddListener(SaveVibrationLevel);
            menuManager = GetComponentInParent<MenuManager>();
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(BackToGame);
            privacypolicy?.gameObject.SetActive(false);
            restorePurchase?.gameObject.SetActive(false);
            shop?.gameObject.SetActive(false);
        }


        private void BackToGame()
        {
            DisablePause();
            Close();
        }

        private void OnDisable()
        {
            // Unregister the OnValueChanged event
            vibrationSlider.onValueChanged.RemoveListener(SaveVibrationLevel);
        }

        private void SaveVibrationLevel(float value)
        {
            PlayerPrefs.SetFloat(VibrationPrefKey, value);
            PlayerPrefs.Save();
        }

        private void LoadVibrationLevel()
        {
            if (PlayerPrefs.HasKey(VibrationPrefKey))
            {
                vibrationSlider.value = PlayerPrefs.GetFloat(VibrationPrefKey);
            }
            else
            {
                vibrationSlider.value = 1.0f;
                SaveVibrationLevel(1.0f);
            }
        }

        private void Retry()
        {
            GameManager.instance.RestartLevel();
            MenuManager.instance.FadeOut();
        }



        private void DisablePause()
        {
            if (StateManager.instance.CurrentState == EScreenStates.Game)
            {
                EventManager.GameStatus = EGameState.Playing;
            }
        }

        private void BackToMain()
        {
            StopInteration();

            Close();
            GameManager.instance.MainMenu();
        }
    }
}