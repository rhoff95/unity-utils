using UnityEngine;

namespace Scripts.Audio
{
    public class PersistentBackgroundAudio : BackgroundAudio
    {
        public const string PrefVolume = "VOLUME";
        private static PersistentBackgroundAudio _instance;

        protected override void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(transform.gameObject);
            
            UpdateVolume();

            base.Awake();
        }

        public static void UpdateVolume()
        {
            var audioVolume = PlayerPrefs.GetFloat(PrefVolume, 1f);
            AudioListener.volume = audioVolume;
            
            Debug.Log($"Setting audio listener volume to {audioVolume}");
        }
    }
}
