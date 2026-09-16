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

using TMPro;
using DG.Tweening;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay
{
    public class ScoreText : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;

        public void ShowScore(int value, Vector3 transformPosition)
        {
            // Ignore the transform position and use the center field position
            scoreText.transform.position = transformPosition;
            scoreText.text = "+" + value;
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.alpha = 1f;
            scoreText.transform.DOKill();
            scoreText.transform.localScale = Vector3.one * .72f;
            scoreText.transform.DOScale(1.18f, .16f).SetEase(Ease.OutBack)
                .OnComplete(() => scoreText.transform.DOScale(1f, .24f).SetEase(Ease.OutQuad));
            scoreText.DOColor(new Color(1f, .92f, .35f), .12f)
                .OnComplete(() => scoreText.DOColor(Color.white, .22f));
        }

        void OnDisable()
        {
            if (scoreText != null) scoreText.transform.DOKill();
        }
    }
}
