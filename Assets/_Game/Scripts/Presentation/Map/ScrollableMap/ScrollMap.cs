using UnityEngine;
using UnityEngine.UI;
namespace RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap
{
    public class ScrollMap : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        public void ScrollToAvatar(Vector2 position)
        {
            if (scrollRect == null || scrollRect.content == null) return;
            var viewport = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
            Canvas.ForceUpdateCanvases();
            scrollRect.StopMovement();
            var avatar = (Vector2)viewport.InverseTransformPoint(position);
            var targetY = viewport.rect.yMin + viewport.rect.height * .2f;
            scrollRect.content.anchoredPosition += new Vector2(0, targetY - avatar.y);
        }
    }
}
