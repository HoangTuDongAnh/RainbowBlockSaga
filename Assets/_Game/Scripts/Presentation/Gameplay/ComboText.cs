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
