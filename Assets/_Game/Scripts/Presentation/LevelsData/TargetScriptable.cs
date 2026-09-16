using System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    public class TargetScriptable : ScriptableObject
    {
        public BonusItemTemplate bonusItem;
        public bool descending = true;
    }

    [Serializable]
    public class Target
    {
        public TargetScriptable targetScriptable;
        public int amount;
        public int totalAmount;

        public Target(TargetScriptable targetScriptableTemplate)
        {
            targetScriptable = targetScriptableTemplate;
        }

        public Target Clone()
        {
            return new Target(targetScriptable)
            {
                amount = amount
            };
        }

        public bool OnCompleted()
        {
            return amount <= 0;
        }
    }
}