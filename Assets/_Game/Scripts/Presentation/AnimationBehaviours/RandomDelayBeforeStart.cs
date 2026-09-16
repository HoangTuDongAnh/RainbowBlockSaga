using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class RandomDelayBeforeStart : StateMachineBehaviour
    {
        private float delay;
        private bool hasStartedOnce;

        [SerializeField]
        private float randomMax;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!hasStartedOnce)
            {
                delay = Random.Range(0.0f, randomMax);
                animator.speed = 0;
            }
            else
            {
                animator.speed = 1;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!hasStartedOnce)
            {
                delay -= Time.deltaTime;

                if (delay <= 0)
                {
                    animator.speed = 1;
                    hasStartedOnce = true;
                }
            }
        }
    }
}