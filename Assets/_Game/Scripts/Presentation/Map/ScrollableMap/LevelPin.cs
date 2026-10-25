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
        private bool isLocked;
        private static readonly Color PassedFill = new(.38f, .89f, .77f);
        private static readonly Color LockedFill = new(.52f, .49f, .65f);
        private static readonly Color CurrentFill = new(1f, .73f, .30f);
        private static readonly Color PassedText = new(.12f, .37f, .43f);
        private static readonly Color LockedText = new(.36f, .28f, .47f);
        private static readonly Color CurrentText = new(.36f, .17f, .39f);

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
            numberLabel.gameObject.SetActive(true);
            numberLabel.color = LockedText;
            numberLabel.fontSizeMin = 38;
            lockObj.SetActive(true);
            openedObj.SetActive(false);
            currentObj.SetActive(false);
            lockObj.GetComponent<Image>().color = LockedFill;
        }
        
        public void UnLock()
        {
            isLocked = false;
            numberLabel.color = PassedText;
            numberLabel.fontSizeMin = 38;
            numberLabel.gameObject.SetActive(true);
            lockObj.SetActive(false);
            openedObj.SetActive(true);
            currentObj.SetActive(false);
            openedObj.GetComponent<Image>().color = PassedFill;
        }
        
        public void SetCurrent(bool isCurrent)
        {
            if (isLocked)
                return;
                
            currentObj.SetActive(isCurrent);
            numberLabel.color = isCurrent ? CurrentText : PassedText;
            currentObj.GetComponent<Image>().color = CurrentFill;
        }

        public void MouseDown()
        {
            if (isLocked)
                return;
            ScrollableMapManager.instance.OpenLevel(number);
        }
    }
}
