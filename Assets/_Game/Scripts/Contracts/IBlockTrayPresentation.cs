using System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Contracts
{
    public interface IBlockTrayPresentation
    {
        int SlotCount { get; }

        Func<UnityEngine.Object[]> BatchProvider { get; set; }

        event Action<UnityEngine.Object[]> BatchPresented;
        event Action<UnityEngine.Object> ShapeAdded;
        event Action RecoveryRequested;

        UnityEngine.Object[] GetVisibleShapeHandles();
        void Clear();
    }
}
