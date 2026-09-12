using RainbowBlockSaga.Runtime;
using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.RuntimeAdapters
{
    /// <summary>
    /// Scene-local runtime context for block presentation dependencies.
    /// Attach this to the same GameObject as HighlightManager.
    /// </summary>
    public class BlockRuntimeContext : MonoBehaviour
    {
        public static BlockRuntimeContext Current { get; private set; }
        public HighlightManager HighlightManager { get; private set; }

        void Awake()
        {
            Current = this;
            HighlightManager = GetComponent<HighlightManager>();
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;
        }
    }
}
