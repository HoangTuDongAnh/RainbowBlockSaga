using TMPro;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI.Labels
{
    public class Label : MonoBehaviour
    {
        public TextMeshProUGUI label;

        private void Awake()
        {
            if (label == null)
            {
                label = GetComponent<TextMeshProUGUI>();
            }
        }
    }
}