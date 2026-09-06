using BlockPuzzleGameToolkit.Scripts.Gameplay;
using UnityEngine;

namespace RainbowBlockSaga.Integration.Toolkit
{
    /// <summary>
    /// Temporary scene-local bridge for toolkit presentation dependencies used by migrated block drag.
    /// Attach this to the same GameObject as HighlightManager.
    /// </summary>
    public class ToolkitBlockMigrationContext : MonoBehaviour
    {
        public static ToolkitBlockMigrationContext Current { get; private set; }
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
