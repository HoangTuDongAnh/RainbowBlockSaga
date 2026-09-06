using UnityEngine;

namespace RainbowBlockSaga.Modes.Objectives
{
    public abstract class ObjectiveData : BaseData
    {
        public abstract IObjective CreateRuntime();
    }
}
