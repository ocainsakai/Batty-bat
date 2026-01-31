using UnityEngine;

namespace LanguageManager
{
    /// <summary>
    /// Custom script to demonstrate how to use LanguageAudioManager to play language-specific audio clips.
    /// </summary>
    public class CustomAudioImport : MonoBehaviour
    {
        [Header("Audio Configuration")]
        [Tooltip("The ID of the audio to play.")]
        public string audioID;  // The unique identifier for the audio clip to play

        public AudioSource audioSource;

        private void Start()
        {
            
            

            if (audioSource == null)
            {
                Debug.LogError("AudioSource component not found on this GameObject.");
                return;
            }

            // Play the audio based on the current language and the provided audioID
           // PlayAudio();
        }

        /// <summary>
        /// Plays the audio clip associated with the audioID and the current language.
        /// </summary>
        private void PlayAudio()
        {
            if (LanguageAudioManager.Instance != null)
            {
                // Use the LanguageAudioManager to play the audio clip
                LanguageAudioManager.Instance.PlayAudioByID(audioID, audioSource);
            }
            else
            {
                Debug.LogError("LanguageAudioManager instance not found.");
            }
        }

        /// <summary>
        /// Optionally, you can trigger this function to play the audio manually.
        /// For example, it could be called when a specific event happens.
        /// </summary>
        public void PlayAudioManually()
        {
            PlayAudio();
        }
    }
}
