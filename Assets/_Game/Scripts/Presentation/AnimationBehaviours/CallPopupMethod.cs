using RainbowBlockSaga.Presentation.Scripts.Popups;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class CallPopupMethod : StateMachineBehaviour
    {
        [SerializeField]
        private string methodName;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var component = animator.gameObject.GetComponent<Popup>();
            if (component != null)
            {
                component.Invoke(methodName, 0f);
            }
        }
    }
}