using RainbowBlockSaga.Presentation.Scripts.Gameplay;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.GUI
{
    public abstract class TargetPanelBase : MonoBehaviour
    {
        protected TargetManager targetManager;

        private void OnEnable()
        {
            targetManager = FindObjectOfType<TargetManager>(true);
            OnEnableInternal();
        }

        private void OnDisable()
        {
            OnDisableInternal();
        }

        protected abstract void OnEnableInternal();
        protected virtual void OnDisableInternal() { } 
    }
} 