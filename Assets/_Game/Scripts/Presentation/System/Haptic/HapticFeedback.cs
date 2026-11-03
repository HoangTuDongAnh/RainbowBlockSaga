using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;

namespace RainbowBlockSaga.Presentation.Scripts.System.Haptic
{
    public class HapticFeedback : MonoBehaviour
    {
        public enum HapticForce
        {
            Light,
            Medium,
            Heavy
        }

        private const string VibrationPrefKey = "VibrationLevel";

        #if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _TriggerHapticFeedback(int force);
        #endif

        private static bool IsSystemSupported()
        {
            #if UNITY_IOS
            return SystemInfo.supportsVibration;
            #elif UNITY_ANDROID
            return SystemInfo.supportsVibration && Permission.HasUserAuthorizedPermission("android.permission.VIBRATE");
            #else
            return false;
            #endif
        }

        private static bool TryHapticFeedback(HapticForce force, float level)
        {
            if (!IsSystemSupported())
                return false;

            try
            {
                #if UNITY_IOS
                int strength = level < .34f ? 0 : level < .67f ? 1 : 2;
                _TriggerHapticFeedback(strength);
                #elif UNITY_ANDROID
                long duration = force switch
                {
                    HapticForce.Light => 50,
                    HapticForce.Medium => 100,
                    HapticForce.Heavy => 200,
                    _ => 50
                };
                Vibration.Vibrate(duration, Mathf.Clamp(Mathf.RoundToInt(255f * level), 1, 255));
                #endif
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Haptic feedback failed: {e.Message}");
                return false;
            }
        }

        public static void TriggerHapticFeedback(HapticForce force)
        {
            float level = PlayerPrefs.GetFloat(VibrationPrefKey, 1f);
            if (level <= 0f)
                return;

            #if UNITY_EDITOR
            return;
            #endif

            if (!TryHapticFeedback(force, level))
            {
                // Fallback - could implement audio feedback or other alternatives here
                Debug.Log("Haptic feedback not available - system unsupported or permission denied");
            }
        }

    }
}
