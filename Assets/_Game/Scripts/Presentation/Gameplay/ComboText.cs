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
    public class ComboText : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public Animator animator;

        private void SetText(int comboCount)
        {
            text.text = "COMBO  x" + comboCount;
            text.alignment = TextAlignmentOptions.Center;
        }

        public void Show(int comboCount)
        {
            SetText(comboCount);
            text.alpha = 1f;
            text.transform.DOKill();
            text.transform.localScale = Vector3.one * .7f;
            text.transform.DOScale(1.2f, .18f).SetEase(Ease.OutBack)
                .OnComplete(() => text.transform.DOScale(1f, .25f).SetEase(Ease.OutQuad));
            text.DOColor(comboCount >= 5 ? new Color(1f, .45f, .9f) : new Color(.45f, .9f, 1f), .12f);
        }

        void OnDisable()
        {
            if (text != null) text.transform.DOKill();
        }
    }
}
