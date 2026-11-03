using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System.Haptic
{
    public static class Vibration
    {
        public static void Vibrate(long durationMs, int amplitude)
        {
            if (Application.platform != RuntimePlatform.Android) return;
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                if (version.GetStatic<int>("SDK_INT") >= 26)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude))
                        vibrator.Call("vibrate", effect);
                }
                else vibrator.Call("vibrate", durationMs);
            }
        }

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
