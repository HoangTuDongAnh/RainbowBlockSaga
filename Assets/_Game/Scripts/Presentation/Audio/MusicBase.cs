using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.Audio;

namespace RainbowBlockSaga.Presentation.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicBase : SingletonBehaviour<MusicBase>
    {
        [SerializeField]
        private AudioMixer mixer;

        [SerializeField]
        private string musicParameter = "musicVolume";

        private void Start()
        {
            float volume = PlayerPrefs.GetFloat("MusicVolume", PlayerPrefs.GetInt("Music", 1));
            mixer.SetFloat(musicParameter, volume <= 0f ? -80f : Mathf.Log10(volume) * 20f);
        }
    }
}
