using UnityEngine;
using UnityEngine.InputSystem;

namespace RainbowBlockSaga.Presentation.Scripts.Settings
{
    public class DebugSettings : SettingsBase
    {
        [Header("Debug hotkeys")]
        public bool enableHotkeys = true;

        [Tooltip("press to win")]
        public Key Win = Key.W;

        [Tooltip("press to lose")]
        public Key Lose = Key.L;
        
        [Tooltip("Update deck")]
        public Key UpdateDeck = Key.D;

        [Tooltip("android's back button")]
        public Key Back = Key.Escape;

        [Header("")]
        [Tooltip("Test language, only for editor")]
        public SystemLanguage TestLanguage = SystemLanguage.English;
    }
}