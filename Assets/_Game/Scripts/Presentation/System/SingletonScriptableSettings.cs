using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    public abstract class SingletonScriptableSettings<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance;

        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<T>("Settings/" + typeof(T).Name);
                }

                return _instance;
            }
        }
    }
}