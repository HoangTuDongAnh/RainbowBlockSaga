using System;
using RainbowBlockSaga.Presentation.Scripts.Attributes;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    public abstract class ScriptableData : ScriptableObject
    {
        [IconPreview]
        public FillAndPreview prefab;

        public virtual void OnValidate()
        {
            OnChange?.Invoke();
        }

        public event Action OnChange;
    }
}