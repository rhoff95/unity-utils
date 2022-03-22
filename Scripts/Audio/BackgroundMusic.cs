using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Audio
{
    public class BackgroundMusic : MonoBehaviour
    {
        private const string TrackIndex = "TrackIndex";
        
        [SerializeField] private List<AudioClip> audioClips;
        [SerializeField] private bool playOnAwake = true;

        private static BackgroundMusic _instance;
        private AudioSource _audioSource;
        private int _audioClipIndex;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(transform.gameObject);
            _audioSource = GetComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            _audioClipIndex = PlayerPrefs.GetInt(TrackIndex, 0);

            if (_audioClipIndex >= audioClips.Count)
            {
                _audioClipIndex = 0;
                PlayerPrefs.SetInt(TrackIndex, _audioClipIndex);
            }
        }

        private void Start()
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

                PlayerPrefs.SetInt(TrackIndex, _audioClipIndex);

                yield return new WaitForSeconds(clip.length);
            }
        }

        public void Mute(bool value)
        {
            _audioSource.mute = value;
        }
    }
}
