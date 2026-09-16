using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public class TargetGUIElement : MonoBehaviour
    {
        public TextMeshProUGUI countText;

        public virtual void UpdateCount(int newCount, bool isTargetCompleted)
        {
            countText.text = newCount.ToString();
        }
    }
}