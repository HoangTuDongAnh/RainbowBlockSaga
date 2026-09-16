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
            mixer.SetFloat(musicParameter, PlayerPrefs.GetInt("Music", 1) == 0 ? -80 : 0);
        }
    }
}