using UnityEngine;

namespace RainbowBlockSaga.Presentation.Scripts.Audio
{
    public class UISoundSequence : MonoBehaviour
    {
        [SerializeField]
        private AudioClip[] clips;

        private int _index;

        public void PlaySound()
        {
            if (clips.Length == 0)
            {
                return;
            }

            SoundBase.instance.PlaySound(clips[_index]);
            _index++;
            if (_index >= clips.Length)
            {
                _index = 0;
            }
        }
    }
}