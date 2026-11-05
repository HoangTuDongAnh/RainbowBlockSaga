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
            musicButton.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", PlayerPrefs.GetInt("Music", 1)));
            soundButton.SetValueWithoutNotify(PlayerPrefs.GetFloat("SoundVolume", PlayerPrefs.GetInt("Sound", 1)));
            musicButton.onValueChanged.AddListener(SetMusicVolume);
            soundButton.onValueChanged.AddListener(SetSoundVolume);
            ApplyVolume(musicParameter, musicButton.value);
            ApplyVolume(soundParameter, soundButton.value);
        }

        private void OnDestroy()
        {
            musicButton.onValueChanged.RemoveListener(SetMusicVolume);
            soundButton.onValueChanged.RemoveListener(SetSoundVolume);
        }

        private void SetMusicVolume(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            ApplyVolume(musicParameter, value);
        }

        private void SetSoundVolume(float value)
        {
            PlayerPrefs.SetFloat("SoundVolume", value);
            ApplyVolume(soundParameter, value);
        }

        private void ApplyVolume(string parameter, float value)
        {
            mixer.SetFloat(parameter, value <= 0f ? -80f : Mathf.Log10(value) * 20f);
        }
    }
}
