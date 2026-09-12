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

using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    [CreateAssetMenu(fileName = "LevelTypeScriptable", menuName = "Rainbow Blocks Saga/Levels/LevelTypeScriptable", order = 1)]
    public class LevelTypeScriptable : ScriptableObject
    {
        [FormerlySerializedAs("levelType")]
        public ELevelType elevelType;

        public TargetScriptable[] targets;
        public Popup prePlayPopup;
        public Popup preFailedPopup;
        public Popup failedPopup;
        public Popup preWinPopup;
        public Popup winPopup;

        public bool selectable = true;

        public bool singleColorMode = true;
        public LevelStateHandler stateHandler;
    }
}