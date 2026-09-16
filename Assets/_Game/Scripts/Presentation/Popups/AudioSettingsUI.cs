using RainbowBlockSaga.Presentation.Scripts.Audio;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace RainbowBlockSaga.Presentation.Scripts.Popups
{
    public class AudioSettingsUI : MonoBehaviour
    {
        [SerializeField]
        private Slider musicButton;

        [SerializeField]
        private Slider soundButton;

        [SerializeField]
        private AudioMixer mixer;

        [SerializeField]
        private string musicParameter = "musicVolume";

        [SerializeField]
        private string soundParameter = "soundVolume";

        private void Start()
        {
            musicButton.onValueChanged.AddListener(ToggleMusic);
            soundButton.onValueChanged.AddListener(ToggleSound);
            OnEnable();
        }

        private void OnEnable()
        {
            UpdateButtonState("Music", musicParameter);
            UpdateButtonState("Sound", soundParameter);
        }

        private void UpdateButtonState(string playerPrefKey, string volumeParameter)
        {
            var enabledState = PlayerPrefs.GetInt(playerPrefKey, 1) != 0f;
            float volumeValue = enabledState ? 0 : -80;

            mixer.SetFloat(volumeParameter, volumeValue);
            if (playerPrefKey == "Sound")
            {
                soundButton.value = enabledState ? 1 : 0;
            }
            else
            {
                musicButton.value = enabledState ? 1 : 0;
            }
        }

        private void ToggleMusic(float arg0)
        {
            SoundBase.instance.PlaySound(SoundBase.instance.click);
            PlayerPrefs.SetInt("Music", (int)arg0);
            OnEnable();
        }

        private void ToggleSound(float arg0)
        {
            SoundBase.instance.PlaySound(SoundBase.instance.click);
            PlayerPrefs.SetInt("Sound", (int)arg0);
            OnEnable();
        }
    }
}