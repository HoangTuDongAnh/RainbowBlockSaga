using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups.Reward
{
    public class RewardVisual : MonoBehaviour
    {
        public TextMeshProUGUI countText;

        public void SetCount(int count)
        {
            countText.text = count.ToString();
        }
    }
}