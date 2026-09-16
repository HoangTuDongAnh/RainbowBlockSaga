using RainbowBlockSaga.Presentation.Scripts.LevelsData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Map.ScrollableMap
{
    public class LevelPin : MonoBehaviour
    {
        [SerializeField]
        public int number = 1;
        [SerializeField]
        private GameObject lockObj;
        [SerializeField]
        private GameObject openedObj;
        [SerializeField]
        private GameObject currentObj;

        [SerializeField]
        private TextMeshProUGUI numberLabel;
        [SerializeField] 
        private Color normalTextColor = Color.white;
        [SerializeField]
        private Color currentTextColor = Color.yellow;
        private bool isLocked;

        private void OnValidate()
        {
            name = "Level_" + number;
            if (numberLabel != null) numberLabel.text = number.ToString();
        }

        public void SetNumber(int number)
        {
            this.number = number;
            numberLabel.text = number.ToString();
        }

        public void Lock()
        {
            isLocked = true;
            numberLabel.color = normalTextColor;
            lockObj.SetActive(true);
            openedObj.SetActive(false);
            currentObj.SetActive(false);
        }
        
        public void UnLock()
        {
            isLocked = false;
            numberLabel.color = normalTextColor;
            numberLabel.gameObject.SetActive(true);
            lockObj.SetActive(false);
            openedObj.SetActive(true);
            currentObj.SetActive(false);
        }
        
        public void SetCurrent(bool isCurrent)
        {
            if (isLocked)
                return;
                
            currentObj.SetActive(isCurrent);
            numberLabel.color = isCurrent ? currentTextColor : normalTextColor;
        }

        public void MouseDown()
        {
            if (isLocked)
                return;
            ScrollableMapManager.instance.OpenLevel(number);
        }
    }
}
