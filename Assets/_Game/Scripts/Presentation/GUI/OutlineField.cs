using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class OutlineField : MonoBehaviour
    {
        [SerializeField]
        private Image imageToFade;

        [SerializeField]
        private Image imageToFade2;

        private void OnEnable()
        {
            imageToFade.DOFade(.5f, .5f).SetLoops(-1, LoopType.Yoyo);
            imageToFade2.DOFade(.5f, .5f).SetLoops(-1, LoopType.Yoyo);
        }
    }
}