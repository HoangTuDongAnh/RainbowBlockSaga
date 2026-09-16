using TMPro;
using UnityEngine;
using DG.Tweening;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class PreWin : Banner
    {
        [SerializeField] private TextMeshProUGUI messageText;

        protected virtual void OnEnable()
        {
            messageText.transform.localScale = Vector3.zero;
        }
        public override void AfterShowAnimation()
        {
            if (messageText != null)
            {
                messageText.transform.DOScale(Vector3.one, 0.2f);
            }
            base.AfterShowAnimation();
        }
    }
    
}