using RainbowBlockSaga.Presentation.Scripts.Enums;
using RainbowBlockSaga.Presentation.Scripts.Popups;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.LevelsData
{
    [CreateAssetMenu(fileName = "LevelTypeScriptable", menuName = "Rainbow Blocks Saga/Levels/LevelTypeScriptable", order = 1)]
    public class LevelTypeScriptable : ScriptableObject
    {
        public ELevelType levelType;

        public TargetScriptable[] targets;
        public Popup prePlayPopup;
        public Popup preFailedPopup;
        public Popup failedPopup;
        public Popup preWinPopup;
        public Popup winPopup;

        public bool selectable = true;

        public bool singleColorMode = true;
        public LevelStateHandler stateHandler;
    }
}
