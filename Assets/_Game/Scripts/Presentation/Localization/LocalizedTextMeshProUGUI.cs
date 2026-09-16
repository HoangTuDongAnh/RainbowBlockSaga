using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Localization
{
    // Kept for prefab compatibility. Rainbow Block Saga ships English text only.
    public class LocalizedTextMeshProUGUI : TextMeshProUGUI
    {
        [SerializeField]
        public string instanceID;

        private string originalText;

        protected override void OnEnable()
        {
            base.OnEnable();
            originalText = text;
            UpdateText();
        }

        public void UpdateText()
        {
            text = originalText;
        }
    }
}
