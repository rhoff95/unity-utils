using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Audio
{
    public class AudioPool : MonoBehaviour
    {
        [Header("Audio Source Defaults")]
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] [Range(-3f, 3f)] private float pitch = 1f;
        
        private readonly List<AudioSource> _audioSources = new List<AudioSource>();

        public void Play(AudioClip audioClip)
        {
            var availableAudioSource =
                _audioSources.FirstOrDefault(audioSource => !audioSource.isPlaying);

            if (availableAudioSource == null)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.volume = volume;
                audioSource.pitch = pitch;

                _audioSources.Add(audioSource);
                availableAudioSource = audioSource;
            }

            availableAudioSource.clip = audioClip;
            availableAudioSource.Play();
        }

        public void Play(AudioClip[] audioClips)
        {
            if (audioClips == null || audioClips.Length == 0)
            {
                return;
            }
            var audioClip = audioClips[Random.Range(0, audioClips.Length)];
            Play(audioClip);
        }
        
        public void Play(List<AudioClip> audioClips)
        {
            if (audioClips == null || audioClips.Count == 0)
            {
                return;
            }
            var audioClip = audioClips[Random.Range(0, audioClips.Count)];
            Play(audioClip);
        }
    }
}
