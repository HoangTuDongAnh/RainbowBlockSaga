using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class RandomWaitTime : StateMachineBehaviour
    {
        public float minWaitTime = 2f;
        public float maxWaitTime = 5f;
        private float nextChangeTime;
        private bool isPlaying;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            nextChangeTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Time.time >= nextChangeTime)
            {
                isPlaying = !isPlaying;
                animator.speed = isPlaying ? 1 : 0;
                nextChangeTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
            }
        }
    }
}