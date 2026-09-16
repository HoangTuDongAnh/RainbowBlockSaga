using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Gameplay.Pool
{
    internal class InitialAmountPool : PoolObject
    {
        [SerializeField]
        private int initialCapacity;

        public override void Awake()
        {
            base.Awake();
            for (var i = 0; i < initialCapacity; i++)
            {
                var item = Create();
                item.SetActive(false);
                pool.Enqueue(item);
            }
            //Debug.Log("Xong khởi tạo shape");
        }
    }
}