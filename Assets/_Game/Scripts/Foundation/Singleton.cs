using UnityEngine;

namespace RainbowBlockSaga.Foundation
{
    /// <summary>
    /// Minimal scene-owned singleton for project-global systems.
    /// Required references are expected to be configured explicitly; no scene searching.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Ins { get; private set; }

        protected virtual void Awake()
        {
            Ins = (T)this;
            SetUp();
        }

        protected virtual void OnDestroy()
        {
            if (Ins == this)
                Ins = null;
        }

        protected virtual void SetUp()
        {
        }
    }
}
