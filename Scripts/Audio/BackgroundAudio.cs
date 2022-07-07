using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundAudio : MonoBehaviour
    {
        public const string PrefTrackIndex = "TRACK_INDEX";
        public const string PrefMusicEnabled = "MUSIC_ENABLED";
        
        [SerializeField] private List<AudioClip> audioClips;
        [SerializeField] private bool playOnAwake = true;
        [SerializeField] private bool ignoreListenerPause = false;

        private AudioSource _audioSource;
        private int _audioClipIndex;

        protected virtual void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.ignoreListenerPause = ignoreListenerPause;

            _audioClipIndex = PlayerPrefs.GetInt(PrefTrackIndex, 0);

            if (_audioClipIndex >= audioClips.Count)
            {
                _audioClipIndex = 0;
                PlayerPrefs.SetInt(PrefTrackIndex, _audioClipIndex);
            }

            var muted = PlayerPrefs.GetInt(PrefMusicEnabled, 1) == 0;
            _audioSource.mute = muted;
            Mute(muted);
        }

        protected virtual void Start()
        {
            if (playOnAwake)
            {
                StartCoroutine(LoopMusic());
            }
        }

        private IEnumerator LoopMusic()
        {
            while (true)
            {
                var clip = audioClips[_audioClipIndex];
                _audioSource.clip = clip;
                _audioSource.Play();

                _audioClipIndex++;
                _audioClipIndex %= audioClips.Count;

                PlayerPrefs.SetInt(PrefTrackIndex, _audioClipIndex);

                yield return new WaitForSeconds(clip.length);
            }
        }
        
        public void Mute(bool value)
        {
            if (_audioSource != null)
            {
                _audioSource.mute = value;
            }
        }
    }
}
