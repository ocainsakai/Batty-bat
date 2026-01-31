using System.Collections.Generic;
using UnityEngine;

namespace LanguageManager
{
    /// <summary>
    /// Singleton class that manages audio clips for different languages based on audio IDs.
    /// </summary>
    public class LanguageAudioManager : MonoBehaviour
    {
        public static LanguageAudioManager Instance;

        /// <summary>
        /// Audio structure that holds a list of audio clips for each language.
        /// </summary>
        [System.Serializable]
        public class AudioEntry
        {
            public string audioID; // Unique identifier for the audio
            public List<LanguageAudio> languageAudios = new List<LanguageAudio>(); // List of language-specific audio clips
        }

        /// <summary>
        /// Structure that associates a language ID with its corresponding audio clip.
        /// </summary>
        [System.Serializable]
        public class LanguageAudio
        {
            public string languageID; // The ID of the language (e.g., "en", "pt-BR", etc.)
            public AudioClip audioClip; // The audio clip associated with the language
        }

        public List<AudioEntry> audioEntries = new List<AudioEntry>(); // List of audio entries to configure

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Returns the audio clip for the given audio ID and the current language.
        /// </summary>
        /// <param name="audioID">The ID of the audio to retrieve.</param>
        /// <returns>The AudioClip associated with the audioID and current language, or null if not found.</returns>
        public AudioClip GetAudioByID(string audioID)
        {
            string currentLanguage = LanguageManager.Instance.GetCurrentLanguage();

            foreach (AudioEntry entry in audioEntries)
            {
                if (entry.audioID == audioID)
                {
                    foreach (LanguageAudio languageAudio in entry.languageAudios)
                    {
                        if (languageAudio.languageID == currentLanguage)
                        {
                            return languageAudio.audioClip;
                        }
                    }
                }
            }

            Debug.LogError($"Audio for ID {audioID} and language {currentLanguage} not found.");
            return null;
        }

        /// <summary>
        /// Checks if a specific audio clip exists for the given audio ID and current language.
        /// </summary>
        /// <param name="audioID">The ID of the audio to check.</param>
        /// <returns>True if the audio clip exists for the current language, otherwise false.</returns>
        public bool AudioExists(string audioID)
        {
            return GetAudioByID(audioID) != null;
        }

        /// <summary>
        /// Plays the audio clip for the given audio ID using the provided AudioSource.
        /// </summary>
        /// <param name="audioID">The ID of the audio to play.</param>
        /// <param name="audioSource">The AudioSource where the audio will be played.</param>
        public void PlayAudioByID(string audioID, AudioSource audioSource)
        {
            AudioClip clip = GetAudioByID(audioID);
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Retrieves all available languages for a specific audio ID.
        /// </summary>
        /// <param name="audioID">The ID of the audio to retrieve the languages for.</param>
        /// <returns>A list of language IDs that have audio clips for the given audioID.</returns>
        public List<string> GetAvailableLanguagesForAudio(string audioID)
        {
            List<string> languages = new List<string>();

            foreach (AudioEntry entry in audioEntries)
            {
                if (entry.audioID == audioID)
                {
                    foreach (LanguageAudio languageAudio in entry.languageAudios)
                    {
                        languages.Add(languageAudio.languageID);
                    }
                }
            }

            return languages;
        }
    }
}
