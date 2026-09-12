using UnityEngine;

namespace RainbowBlockSaga.Gameplay.Objectives
{
    public abstract class ObjectiveData : BaseData
    {
        public abstract IObjective CreateRuntime();
    }
}
