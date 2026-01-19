using UnityEngine;

namespace Core.Patterns.Examples
{
    /// <summary>
    /// Example of Regulator Singleton pattern.
    /// This audio manager allows newer instances to replace older ones.
    /// Useful when loading new scenes with their own audio managers.
    /// </summary>
    public class RegulatorAudioManager : RegulatorSingleton<RegulatorAudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Scene-Specific Music")]
        [SerializeField] private AudioClip sceneMusic;
        [SerializeField] private bool autoPlaySceneMusic = true;

        protected override void Awake()
        {
            base.Awake();

            // Only initialize if this is the current singleton instance
            if (IsCurrentInstance)
            {
                InitializeAudioSources();
                
                if (autoPlaySceneMusic && sceneMusic != null)
                {
                    PlayMusic(sceneMusic);
                }
                
                Debug.Log($"[RegulatorAudioManager] Initialized for scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
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

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
                Debug.Log($"[RegulatorAudioManager] Playing music: {clip.name}");
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
                Debug.Log("[RegulatorAudioManager] Music stopped");
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
            if (musicSource != null)
            {
                musicSource.volume = Mathf.Clamp01(volume);
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null)
            {
                sfxSource.volume = Mathf.Clamp01(volume);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (IsCurrentInstance)
            {
                Debug.Log("[RegulatorAudioManager] Current instance destroyed");
            }
        }
    }
}
