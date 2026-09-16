using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay.FX
{
    public class Outline : MonoBehaviour
    {
        private Image image;
        private Image image1;
        private RectTransform _rectTransform;

        private void Awake()
        {
            image = GetComponent<Image>();
            image1 = GetComponentInChildren<Image>();
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Init(Vector3 center, Vector2 sizeInLocalSpace, Color color)
        {
            _rectTransform.anchoredPosition = center;
            _rectTransform.sizeDelta = sizeInLocalSpace;
            image.color = color;
        }

        public void Play(Vector2 center, Vector2 size, Color white)
        {
            Init(center, size, white);
            image.DOFade(.5f, .5f).SetLoops(-1, LoopType.Yoyo);
            image1.DOFade(.5f, .5f).SetLoops(-1, LoopType.Yoyo);
        }
    }
}