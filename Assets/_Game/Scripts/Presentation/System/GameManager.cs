// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Globalization;
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using RainbowBlockSaga.Presentation.Scripts.Popups.Daily;
using RainbowBlockSaga.Presentation.Scripts.Settings;
using DG.Tweening;
using UnityEngine;
using ResourceManager = RainbowBlockSaga.Presentation.Scripts.Data.ResourceManager;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public DebugSettings debugSettings;
        public DailyBonusSettings dailyBonusSettings;
        public GameSettings GameSettings;
        public SpinSettings luckySpinSettings;
        private int lastBackgroundIndex = -1;
        private bool isTutorialMode;
        private MainMenu mainMenu;
        private bool blockButtons;

        public int Score { get=> ResourceManager.instance.GetResource("Score").GetValue(); set => ResourceManager.instance.GetResource("Score").Set(value); }

        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
            Application.targetFrameRate = 60;
            DOTween.SetTweensCapacity(1250, 512);

            mainMenu = FindObjectOfType<MainMenu>();
            if (mainMenu != null)
            {
                mainMenu.OnAnimationEnded += OnMainMenuAnimationEnded;
            }
            
        }

        private void OnEnable()
        {
            if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {
                if (!GameDataManager.isTestPlay && CheckDailyBonusConditions())
                {
                    blockButtons = true;
                }
            }

            if (!IsTutorialShown() && !GameDataManager.isTestPlay)
            {
                SetTutorialMode(true);
            }
        }

        private void OnDisable()
        {
            if (mainMenu != null)
            {
                mainMenu.OnAnimationEnded -= OnMainMenuAnimationEnded;
            }
            GameDataManager.isTestPlay = false; // Reset isTestPlay
        }

        private bool IsTutorialShown()
        {
            return PlayerPrefs.GetInt("tutorial", 0) == 1;
        }

        public void SetTutorialCompleted()
        {
            PlayerPrefs.SetInt("tutorial", 1);
            PlayerPrefs.Save();
        }

        private void Start()
        {
            if (GameDataManager.isTestPlay)
                GameDataManager.SetLevel(GameDataManager.GetLevel());
        }


        private void HandleDailyBonus()
        {
            if (StateManager.instance.CurrentState != EScreenStates.MainMenu || !dailyBonusSettings.dailyBonusEnabled)
            {
                return;
            }

            var shouldShowDailyBonus = CheckDailyBonusConditions();

            if (shouldShowDailyBonus)
            {
                var daily = MenuManager.instance.ShowPopup<DailyBonus>(()=>
                {
                    blockButtons = false;
                });
            }
        }

        private bool CheckDailyBonusConditions()
        {
            var today = DateTime.Today;
            var lastRewardDate = DateTime.Parse(PlayerPrefs.GetString("DailyBonusDay", today.Subtract(TimeSpan.FromDays(1)).ToString(CultureInfo.CurrentCulture)));
            return today.Date > lastRewardDate.Date && dailyBonusSettings.dailyBonusEnabled;
        }

        public void RestartLevel()
        {
            DOTween.KillAll();
            MenuManager.instance.CloseAllPopups();
            EventManager.GetEvent(EGameEvent.RestartLevel).Invoke();
        }


        public void MainMenu()
        {
            DOTween.KillAll();
            if (StateManager.instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.instance.GoMain();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Adventure)
            {
                SceneLoader.instance.StartMapScene();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.Map)
            {
                SceneLoader.instance.GoMain();
            }
            else if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {
                MenuManager.instance.ShowPopup<Quit>();
            }
            else
            {
                SceneLoader.instance.GoMain();
            }
        }

        public void OpenMap()
        {
            if (blockButtons && StateManager.instance.CurrentState == EScreenStates.MainMenu)
                return;
            if (GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.instance.StartGameSceneClassic();
            }
            else if (GetGameMode() == EGameMode.Timed)
            {
                SceneLoader.instance.StartGameSceneTimed();
            }
            else
            {
                SceneLoader.instance.StartMapScene();
            }
        }

        public void OpenGame()
        {
            SceneLoader.instance.StartGameScene();
        }



        public void SetGameMode(EGameMode gameMode)
        {
            GameDataManager.SetGameMode(gameMode);
        }

        private EGameMode GetGameMode()
        {
            return GameDataManager.GetGameMode();
        }

        public int GetLastBackgroundIndex()
        {
            return lastBackgroundIndex;
        }

        public void SetLastBackgroundIndex(int index)
        {
            lastBackgroundIndex = index;
        }

        public void NextLevel()
        {
            GameDataManager.LevelNum++;
            OpenGame();
            RestartLevel();
        }

        public void SetTutorialMode(bool tutorial)
        {
            Debug.Log("Tutorial mode set to " + tutorial);
            isTutorialMode = tutorial;
        }

        public bool IsTutorialMode()
        {
            return isTutorialMode;
        }

        private void OnMainMenuAnimationEnded()
        {
            if (StateManager.instance.CurrentState == EScreenStates.MainMenu)
            {

                HandleDailyBonus();
            }
        }


    }
}