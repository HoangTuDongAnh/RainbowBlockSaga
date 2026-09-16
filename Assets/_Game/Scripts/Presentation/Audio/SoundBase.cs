using System.Collections;
using System.Collections.Generic;
using RainbowBlockSaga.Presentation.Scripts.System;
using UnityEngine;
using UnityEngine.Audio;

namespace RainbowBlockSaga.Presentation.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundBase : SingletonBehaviour<SoundBase>
    {
        [SerializeField]
        private AudioMixer mixer;

        [SerializeField]
        private string soundParameter = "soundVolume";

        public AudioClip click;
        public AudioClip[] swish;
        public AudioClip coins;
        public AudioClip coinsSpend;
        public AudioClip luckySpin;
        public AudioClip warningTime;
        public AudioClip placeShape;
        public AudioClip fillEmpty;
        public AudioClip alert;
        public AudioClip[] combo;

        public AudioClip selected;
        public AudioClip dropItem;
        private AudioSource audioSource;
        private readonly HashSet<AudioClip> clipsPlaying = new();

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (mixer != null)
                mixer.SetFloat(soundParameter, PlayerPrefs.GetInt("Sound", 1) == 0 ? -80 : 0);
            audioSource.playOnAwake = false;
            audioSource.volume = 1f;
            audioSource.ignoreListenerPause = true;
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        public void PlayPlacementSound()
        {
            PlayGameplayClip(placeShape);
        }

        public void PlayClearSound(int comboIndex)
        {
            if (combo == null || combo.Length == 0) return;
            PlayGameplayClip(combo[Mathf.Clamp(comboIndex, 0, combo.Length - 1)]);
        }

        void PlayGameplayClip(AudioClip clip)
        {
            if (clip != null)
                audioSource.PlayOneShot(clip, 1.35f);
        }

        public void PlayDelayed(AudioClip clip, float delay)
        {
            StartCoroutine(PlayDelayedCoroutine(clip, delay));
        }

        private IEnumerator PlayDelayedCoroutine(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlaySound(clip);
        }

        public void PlaySoundsRandom(AudioClip[] clip)
        {
            instance.PlaySound(clip[Random.Range(0, clip.Length)]);
        }

        public void PlayLimitSound(AudioClip clip)
        {
            if (clipsPlaying.Add(clip))
            {
                PlaySound(clip);
                StartCoroutine(WaitForCompleteSound(clip));
            }
        }

        private IEnumerator WaitForCompleteSound(AudioClip clip)
        {
            yield return new WaitForSeconds(0.1f);
            clipsPlaying.Remove(clip);
        }
    }
}
