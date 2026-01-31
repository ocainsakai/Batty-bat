using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles audio for a specific object with a list of audio clips that can be triggered.
    /// </summary>
    public class ObjectAudioController : MonoBehaviour
    {
        [Header("Audio Settings")]
        [Tooltip("The tag for categorizing the audio (e.g., VFX, Ambient, Custom).")]
        public string audioTag = "VFX";  // Default tag is VFX

        [Tooltip("Audio clip that plays when the object starts.")]
        public AudioClip startAudio;

        [Tooltip("List of audio clips that can be triggered by index.")]
        public List<AudioClip> audioClips;

        private void OnEnable()
        {
            // Play start audio if set
            if (startAudio != null)
            {
                AudioManager.Singleton.PlayAudio(startAudio, audioTag, gameObject.transform.position);
            }
        }

        /// <summary>
        /// Plays an audio clip from the list based on the index provided.
        /// </summary>
        public void PlayAudioByIndex(int index)
        {
            if (index >= 0 && index < audioClips.Count)
            {
                AudioManager.Singleton.PlayAudio(audioClips[index], audioTag);
            }
            else
            {
                Debug.LogWarning("Invalid audio clip index.");
            }
        }
    }
}
