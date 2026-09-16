using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System.Haptic
{
    public static class Vibration
    {
        public static void Vibrate(long[] pattern, int repeat)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                        {
                            vibrator.Call("vibrate", pattern, repeat);
                        }
                    }
                }
            }
        }
    }
}