using System.Collections.Generic;
using UnityEngine;

namespace LanguageManager
{
    /// <summary>
    /// Represents an audio clip localized for a specific language.
    /// </summary>
    [System.Serializable]
    public class LocalizedAudioClip
    {
        public string languageID;
        public AudioClip audioClip;
    }

    /// <summary>
    /// AudioWarper component that handles language-specific audio clips.
    /// </summary>
    public class AudioWarper : MonoBehaviour
    {
        /// <summary>
        /// List of audio clips for different languages.
        /// </summary>
        public List<LocalizedAudioClip> audioClips = new List<LocalizedAudioClip>();

        /// <summary>
        /// Change the AudioSource's clip based on the current language.
        /// </summary>
        [Header("Change AudioSource Clip based on Language")]
        public bool changeAudioSourceClip = false;

        /// <summary>
        /// The AudioSource to change the clip of.
        /// </summary>
        public AudioSource audioSource;

        private void Awake()
        {
            // Get the AudioSource component if not assigned
            if (changeAudioSourceClip && audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            // Subscribe to language change event
            LanguageManager.onLanguageChanged += UpdateAudioClip;
        }

        private void OnDestroy()
        {
            // Unsubscribe from language change event
            LanguageManager.onLanguageChanged -= UpdateAudioClip;
        }

        private void Start()
        {
            // Update the audio clip at the start
            UpdateAudioClip();
        }

        /// <summary>
        /// Updates the audio clip based on the current language.
        /// </summary>
        public void UpdateAudioClip()
        {
            string currentLanguageID = LanguageManager.Instance.currentLanguageID;
            AudioClip clip = GetAudioClipForLanguage(currentLanguageID);

            if (clip != null && changeAudioSourceClip && audioSource != null)
            {
                audioSource.clip = clip;
            }
        }

        /// <summary>
        /// Gets the audio clip for the specified language.
        /// </summary>
        /// <param name="languageID">The language ID.</param>
        /// <returns>The AudioClip for the language, or null if not found.</returns>
        public AudioClip GetAudioClipForLanguage(string languageID)
        {
            foreach (LocalizedAudioClip localizedClip in audioClips)
            {
                if (localizedClip.languageID == languageID)
                {
                    return localizedClip.audioClip;
                }
            }
            return null;
        }

        /// <summary>
        /// Plays the audio clip for the current language.
        /// </summary>
        public void PlayLocalizedAudio()
        {
            AudioClip clip = GetAudioClipForLanguage(LanguageManager.Instance.currentLanguageID);
            if (clip != null)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(clip);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position);
                }
            }
        }

        /// <summary>
        /// Stops the audio playback.
        /// </summary>
        public void StopAudio()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }
    }
}
