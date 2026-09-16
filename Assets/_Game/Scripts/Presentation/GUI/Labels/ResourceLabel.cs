using RainbowBlockSaga.Presentation.Scripts.Data;
using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI.Labels
{
    public class ResourceLabel : MonoBehaviour
    {
        public ResourceObject resourceObject;

        [SerializeField]
        private TextMeshProUGUI text;

        protected virtual void OnEnable()
        {
            resourceObject.OnResourceUpdate += UpdateValue;
            UpdateValue(resourceObject.GetValue());
        }

        protected virtual void OnDisable()
        {
            resourceObject.OnResourceUpdate -= UpdateValue;
        }

        protected virtual void UpdateValue(int count)
        {
            text.text = count.ToString();
        }
    }
}