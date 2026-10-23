using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    // Authored on the prefab. Scales its content within the popup canvas and safe area.
    public sealed class PopupContentFit : MonoBehaviour
    {
        [SerializeField] RectTransform content;
        [SerializeField] Vector2 designSize = new Vector2(1000, 1100);
        public void SetDesignSize(Vector2 size) => designSize = size;
        void LateUpdate()
        {
            var parent = (RectTransform)transform;
            var safe = Screen.safeArea;
            float width = parent.rect.width * safe.width / Mathf.Max(1, Screen.width);
            float height = parent.rect.height * safe.height / Mathf.Max(1, Screen.height);
            content.localScale = Vector3.one * Mathf.Min(width * .9f / designSize.x, height * .88f / designSize.y);
        }
    }
}
