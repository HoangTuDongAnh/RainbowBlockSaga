using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap
{
    public class MapObjectAppearance : MonoBehaviour
    {
        [SerializeField]
        private RectTransform animateTransform;

        private Canvas canvas;
        private Animator animator;
        private bool _isVisible;

        void Awake()
        {
            // Get the components
            animateTransform = GetComponent<RectTransform>();
            animator = GetComponent<Animator>();
            canvas = GetComponentInParent<Canvas>();
        }

        void Update()
        {
            if (animator != null && !_isVisible && IsVisible())
            {
                _isVisible = true;
                animator.Play("appear");
            }
        }

        private bool IsVisible() {
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, animateTransform.position);
            return screenPoint.x >= 0 && screenPoint.x <= Screen.width &&
                   screenPoint.y >= 0 && screenPoint.y <= Screen.height;
        }
    }
}