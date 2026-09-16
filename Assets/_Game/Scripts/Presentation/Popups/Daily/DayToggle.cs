using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups.Daily
{
    public class DayToggle : MonoBehaviour
    {
        public Image panelImage;
        public Color colorCurrent;

        [SerializeField]
        public GameObject current;

        public void SetStatus(EDailyStatus eDailyStatus)
        {
            if (eDailyStatus == EDailyStatus.current)
            {
                current.SetActive(true);
                panelImage.color = colorCurrent;
            }
        }
    }

    public enum EDailyStatus
    {
        locked = 0,
        passed = 1,
        current = 2
    }
}