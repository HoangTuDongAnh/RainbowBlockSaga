using System.Collections.Generic;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class RandomTransitionBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // Get all parameters of the animator
            var parameters = animator.parameters;

            // Filter and store trigger parameters
            var triggerParams = new List<AnimatorControllerParameter>();
            foreach (var param in parameters)
            {
                if (param.type == AnimatorControllerParameterType.Trigger)
                {
                    triggerParams.Add(param);
                }
            }

            // Check if there are any trigger parameters
            if (triggerParams.Count > 0)
            {
                // Select a random trigger
                var randomIndex = Random.Range(0, triggerParams.Count);
                var randomTriggerName = triggerParams[randomIndex].name;

                // Set the random trigger
                animator.SetTrigger(randomTriggerName);
            }
        }
    }
}