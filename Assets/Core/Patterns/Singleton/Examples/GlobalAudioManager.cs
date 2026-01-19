using UnityEngine;

namespace Core.Patterns.Examples
{
    /// <summary>
    /// Example of Persistent MonoBehaviour Singleton pattern.
    /// This manager persists across all scenes using DontDestroyOnLoad.
    /// Perfect for game-wide functionality like audio, save system, or game state.
    /// </summary>
    public class GlobalAudioManager : PersistentSingleton<GlobalAudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Settings")]
        [SerializeField] private float defaultMusicVolume = 0.7f;
        [SerializeField] private float defaultSFXVolume = 1f;

        private float _musicVolume;
        private float _sfxVolume;

        protected override void Awake()
        {
            base.Awake();

            // Only initialize if this is the singleton instance
            if (Instance == this)
            {
                InitializeAudioSources();
                LoadSettings();
                Debug.Log("[GlobalAudioManager] Initialized and will persist across scenes");
            }
        }

        private void InitializeAudioSources()
        {
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void LoadSettings()
        {
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
            _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);

            if (musicSource != null) musicSource.volume = _musicVolume;
            if (sfxSource != null) sfxSource.volume = _sfxVolume;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
                Debug.Log($"[GlobalAudioManager] Playing music: {clip.name}");
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                Debug.Log("[GlobalAudioManager] Music stopped");
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null) musicSource.volume = _musicVolume;
            PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            if (sfxSource != null) sfxSource.volume = _sfxVolume;
            PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
            PlayerPrefs.Save();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log("[GlobalAudioManager] Destroyed");
        }
    }
}
