using UnityEngine;
using UnityEngine.Playables;

namespace RainbowBlockSaga.Presentation.Scripts.Settings
{
    public class ClearDirectorBindingsAfterStop : MonoBehaviour
    {
        private PlayableDirector timelineDirector;
        private bool timelineFinished;

        private void Awake()
        {
            timelineDirector = GetComponent<PlayableDirector>();
        }

        private void Update()
        {
            if (!timelineFinished && timelineDirector.time >= timelineDirector.duration)
            {
                timelineFinished = true;
                // clear all bindings
                timelineDirector.playableAsset = null;
                timelineDirector.Stop();
            }
        }
    }
}