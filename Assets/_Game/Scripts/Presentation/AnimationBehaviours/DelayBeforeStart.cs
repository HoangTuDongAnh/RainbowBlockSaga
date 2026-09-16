using UnityEngine;
using UnityEngine.Serialization;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class DelayBeforeStart : StateMachineBehaviour
    {
        private float delay;
        private bool hasStartedOnce;

        [FormerlySerializedAs("randomMax")]
        [SerializeField]
        private float delayMax;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!hasStartedOnce)
            {
                delay = delayMax;
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