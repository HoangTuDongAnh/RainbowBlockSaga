using UnityEngine;
using System.Collections;
using RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap;
using UnityEngine.UI;
using System.Threading.Tasks;

[RequireComponent(typeof(RectTransform))]
public class ContentStretchController : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    public void HandleLastLevelPositionUpdate(Vector2 lastLevelPos)
    {
        // Keep the original bottom edge fixed while extending the scrollable
        // content upward to include level pins instantiated after level 11.
        float oldHeight = rectTransform.rect.height;
        float bottomOffset = rectTransform.anchoredPosition.y - oldHeight;
        float lastPinY = rectTransform.InverseTransformPoint(lastLevelPos).y;
        float requiredHeight = Mathf.Max(oldHeight, oldHeight + lastPinY + 220f);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x, requiredHeight + bottomOffset);
    }
}
