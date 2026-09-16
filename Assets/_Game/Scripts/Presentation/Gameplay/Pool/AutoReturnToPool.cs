using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay.Pool
{
    public class AutoReturnToPool : MonoBehaviour
    {
        public float timeToReturn;

        private void OnEnable()
        {
            if (timeToReturn > 0)
            {
                Invoke(nameof(ReturnToPool), timeToReturn);
            }
        }

        private void ReturnToPool()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            PoolObject.Return(gameObject);
        }
    }
}