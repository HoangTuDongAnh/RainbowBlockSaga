using RainbowBlockSaga.Presentation.Scripts.GUI;
using RainbowBlockSaga.Presentation.Scripts.GUI.Labels;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class FailedScore : Failed
    {
        public Transform scorePosition;

        private void Start()
        {
            var scoreLabel = FindObjectOfType<TargetsUIHandler>().ScoreLabel;
            scoreLabel.GetComponent<TargetScoreGUIElement>().enabled = false;
            var scoreObject = Instantiate(scoreLabel, transform);
            scoreObject.transform.position = scorePosition.position;
        }
    }
}