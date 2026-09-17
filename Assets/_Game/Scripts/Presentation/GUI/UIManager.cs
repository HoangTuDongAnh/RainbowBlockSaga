using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class UIManager : MonoBehaviour
    {
        public CustomButton pauseButton;

        public GameObject renewButton;
        public GameObject explosionButton;


        private bool uiLocked = false;

        private void Awake()
        {
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }
        private void Start()
        {
            UpdateBoosterVisibility();

            EventManager.GetEvent(EGameEvent.TutorialCompleted).Subscribe(OnTutorialCompleted);
        }
        private void UpdateBoosterVisibility()
        {
            bool isTutorial = GameManager.instance.IsTutorialMode();

            // Booster buttons are optional in this game mode.
            if (renewButton != null)
                renewButton.SetActive(!isTutorial);

            if (explosionButton != null)
                explosionButton.SetActive(!isTutorial);
        }
        private void OnTutorialCompleted()
        {
            UpdateBoosterVisibility();
        }

        private void OnEnable()
        {
            EventManager.GetEvent(EGameEvent.UILocked).Subscribe(OnUILocked);
            EventManager.GetEvent(EGameEvent.UIUnlocked).Subscribe(OnUIUnlocked);
        }

        private void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.UILocked).Unsubscribe(OnUILocked);
            EventManager.GetEvent(EGameEvent.UIUnlocked).Unsubscribe(OnUIUnlocked);
        }

        private void OnUILocked()
        {
            uiLocked = true;
        }

        private void OnUIUnlocked()
        {
            uiLocked = false;
        }

        private void OnPauseButtonClicked()
        {
            if(uiLocked) return;
            EventManager.GameStatus = EGameState.Pause;
            if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {
                MenuManager.instance.ShowPopup<Popups.Settings>();
            }
            else
            {
                MenuManager.instance.ShowPopup("Popups/SettingsGame");
            }
        }
    }
}
