using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();
                }

                return _instance;
            }
            private set => _instance = value;
        }

        public virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                enabled = false;
                Destroy(gameObject);
            }
            else
            {
                instance = (T)this;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
