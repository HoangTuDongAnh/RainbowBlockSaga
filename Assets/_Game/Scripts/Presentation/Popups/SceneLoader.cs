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
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class SceneLoader : SingletonBehaviour<SceneLoader>
    {
        public static Action<Scene> OnSceneLoadedCallback;
        private Scene previouseScene;

        private void Start()
        {
            CheckEvent(SceneManager.GetActiveScene());
        }

        public void StartGameSceneTimed()
        {
            StartLevel(EGameMode.Timed, Resources.Load<Level>("Misc/TimeLevel"));
        }

        public void StartGameSceneClassic()
        {
            StartLevel(EGameMode.Classic, Resources.Load<Level>("Misc/ClassicLevel"));
        }

        public void StartGameScene(int levelNumber = 0)
        {
            StartLevel(EGameMode.Adventure, ArcadeLevelCatalog.Find(levelNumber > 0 ? levelNumber : GameDataManager.GetLevelNum()));
        }

        private void StartLevel(EGameMode mode, Level level)
        {
            if (level == null || level.levelType == null)
            {
                Debug.LogError("Cannot start a level with missing data or Level Type.");
                return;
            }

            var alreadyPlaying = StateManager.instance.CurrentState == EScreenStates.Game;
            GameDataManager.SetGameMode(mode);
            GameDataManager.SetLevel(level);
            if (mode == EGameMode.Adventure)
                GameDataManager.SetLevelNum(level.Number);
            if (alreadyPlaying)
                GameManager.instance.RestartLevel();
            else
                StateManager.instance.CurrentState = EScreenStates.Game;
        }

        public void GoMain()
        {
            StateManager.instance.CurrentState = EScreenStates.MainMenu;
        }



        private void CheckEvent(Scene scene)
        {
            if (previouseScene != scene)
            {
                OnSceneLoadedCallback?.Invoke(scene);
                previouseScene = scene;
            }
        }

        public void StartMapScene()
        {
            StateManager.instance.CurrentState = EScreenStates.Map;
        }

    }
}
