using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.System
{
    public class FPSCounter : MonoBehaviour
    {
        private readonly float updateInterval = 0.5f;
        private float accum;
        private int frames;
        private float timeLeft;
        private float fps;

        private readonly GUIStyle textStyle = new();

        private void Start()
        {
            timeLeft = updateInterval;

            // Set up the GUI style
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 24; // Adjust this value to change the text size
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            if (timeLeft <= 0f)
            {
                fps = accum / frames;
                timeLeft = updateInterval;
                accum = 0f;
                frames = 0;
            }
        }

        private void OnGUI()
        {
            // Display FPS in the top-left corner
            UnityEngine.GUI.Label(new Rect(10, 10, 100, 30), "FPS: " + fps.ToString("F2"), textStyle);
        }
    }
}