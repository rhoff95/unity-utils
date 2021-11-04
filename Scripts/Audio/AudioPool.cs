using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Audio
{
    public class AudioPool : MonoBehaviour
    {
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

                _audioSources.Add(audioSource);
                availableAudioSource = audioSource;
            }

            availableAudioSource.clip = audioClip;
            availableAudioSource.Play();
        }

        public void Play(AudioClip[] audioClips)
        {
            var audioClip = audioClips[Random.Range(0, audioClips.Length)];
            Play(audioClip);
        }
    }
}
