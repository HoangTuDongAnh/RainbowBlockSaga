using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.AnimationBehaviours
{
    public class RandomizedState : StateMachineBehaviour
    {
        [Header("Add RandomFinished trigger to transition to next state")]
        [SerializeField]
        private float minTime = 2f;

        [SerializeField]
        private float maxTime = 4f;

        [SerializeField]
        private string randomfinishedStr = "RandomFinished";

        private float currentTimeBeforeBlink;
        private float timeElapsed;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Randomize();
        }

        private void Randomize()
        {
            currentTimeBeforeBlink = Random.Range(minTime, maxTime);
            timeElapsed = 0f;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= currentTimeBeforeBlink)
            {
                animator.SetTrigger(Animator.StringToHash(randomfinishedStr));
            }
        }
    }
}