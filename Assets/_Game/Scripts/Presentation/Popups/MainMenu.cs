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
using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class MainMenu : Popup
    {
        [FormerlySerializedAs("classicMode")]
        public CustomButton endlessMode;
        [FormerlySerializedAs("adventureMode")]
        public CustomButton arcadeMode;
        public CustomButton settingsButton;
        public CustomButton luckySpin;
        public GameObject playObject;

        [SerializeField]
        private GameObject freeSpinMarker;

        [SerializeField]
        private Image background;

        public Action OnAnimationEnded;

        private const string LastFreeSpinTimeKey = "LastFreeSpinTime";

        private void Start()
        {
            endlessMode.onClick.AddListener(PlayEndlessMode);
            arcadeMode.onClick.AddListener(PlayArcadeMode);
            settingsButton.onClick.AddListener(SettingsButtonClicked);
            luckySpin.onClick.AddListener(LuckySpinButtonClicked);
            UpdateFreeSpinMarker();
            GameDataManager.LevelNum = PlayerPrefs.GetInt("Level", 1);
            luckySpin.gameObject.SetActive(GameManager.instance.GameSettings.enableLuckySpin);
        }
        private bool CanUseFreeSpinToday()
        {
            if (!PlayerPrefs.HasKey(LastFreeSpinTimeKey))
            {
                return true;
            }

            var lastFreeSpinTimeStr = PlayerPrefs.GetString(LastFreeSpinTimeKey);
            var lastFreeSpinTime = DateTime.Parse(lastFreeSpinTimeStr);
            return DateTime.Now.Date > lastFreeSpinTime.Date;
        }

        private void UpdateFreeSpinMarker()
        {
            var isFreeSpinAvailable = CanUseFreeSpinToday();
            freeSpinMarker.SetActive(isFreeSpinAvailable);
        }

        private void PlayEndlessMode()
        {
            // Keep the existing mode ID so Endless resumes saved Classic games.
            PlayModeWithResumeCheck(EGameMode.Classic);
        }

        private void PlayArcadeMode()
        {
            GameManager.instance.SetGameMode(EGameMode.Adventure);
            GameManager.instance.OpenMap();
        }

        private void PlayModeWithResumeCheck(EGameMode mode)
        {
            GameManager.instance.SetGameMode(mode);

            if (!GameState.HasMeaningfulState(mode))
            {
                GameManager.instance.OpenMap();
                return;
            }

            MenuManager.instance.ShowPopup<ContinueGamePopup>(
                null,
                result =>
                {
                    if (result == EPopupResult.Restart)
                        GameState.Delete(mode);

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
