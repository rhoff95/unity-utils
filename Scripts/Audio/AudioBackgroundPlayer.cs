using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioBackgroundPlayer : MonoBehaviour
    {
       
        public AudioClip[] audioClips;
        
        private AudioSource _audioSource;
        private int _audioClipIndex = -1;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            StartCoroutine(PlayAudio());
        }

        private IEnumerator PlayAudio()
        {
            while (true)
            {
                if (!_audioSource.isPlaying)
                {
                    var newAudioClipIndex = Random.Range(0, audioClips.Length);
                    while (newAudioClipIndex == _audioClipIndex)
                    {
                        newAudioClipIndex = Random.Range(0, audioClips.Length);
                    }

                    _audioClipIndex = newAudioClipIndex;
                    
                    _audioSource.clip = audioClips[_audioClipIndex];
                    _audioSource.Play();
                }

                yield return new WaitForSeconds(1f);
            }
        }
    }
}
