// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

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
        AudioClip fallbackPlace;
        AudioClip fallbackClear;

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
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.volume = 1f;
                audioSource.ignoreListenerPause = true;
            }
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public void PlayPlacementSound()
        {
            PlayGameplayClip(placeShape != null ? placeShape : selected != null ? selected : click != null ? click : fallbackPlace ??= CreateTone("RBS_Place", 640, .08f, .18f));
        }

        public void PlayClearSound(int comboIndex)
        {
            AudioClip clip = null;
            if (combo != null && combo.Length > 0)
                clip = combo[Mathf.Clamp(comboIndex, 0, combo.Length - 1)];
            PlayGameplayClip(clip != null ? clip : fillEmpty != null ? fillEmpty : coins != null ? coins : fallbackClear ??= CreateTone("RBS_Clear", 920, .16f, .24f));
        }

        void PlayGameplayClip(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip, 1.35f);
        }

        static AudioClip CreateTone(string name, int frequency, float duration, float volume)
        {
            const int rate = 44100;
            int length = Mathf.CeilToInt(rate * duration);
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float envelope = 1f - i / (float)length;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / rate) * envelope * volume;
            }
            var clip = AudioClip.Create(name, length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
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
