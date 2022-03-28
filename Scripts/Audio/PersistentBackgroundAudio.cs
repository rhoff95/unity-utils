namespace Scripts.Audio
{
    public class PersistentBackgroundAudio : BackgroundAudio
    {
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

            base.Awake();
        }
    }
}
