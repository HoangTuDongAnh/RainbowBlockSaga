using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Localization
{
    // Compatibility component for existing prefabs. The game ships English text only.
    public sealed class LocalizationManager : SingletonBehaviour<LocalizationManager>
    {
        public override void Awake()
        {
            base.Awake();
        }

        public static void InitializeLocalization() { }
        public static void LoadLanguage(SystemLanguage language) { }
        public static SystemLanguage GetSystemLanguage() => SystemLanguage.English;
        public static SystemLanguage GetCurrentLanguage() => SystemLanguage.English;
        public static string GetText(string key, string defaultText) => defaultText;
    }
}
