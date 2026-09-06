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

using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.GUI
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

            // Booster buttons are optional in the stripped BlockBlast-only migration scene.
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