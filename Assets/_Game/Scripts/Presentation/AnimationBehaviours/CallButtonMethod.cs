using RainbowBlockSaga.Presentation.Scripts.GUI;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class CallButtonMethod : StateMachineBehaviour
    {
        [SerializeField]
        private string exitMethod;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            var component = animator.gameObject.GetComponent<CustomButton>();
            if (component != null)
            {
                component.Invoke(exitMethod, 0.3f);
            }
        }
    }
}