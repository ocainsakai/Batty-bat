using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages ambient audio for each scene, playing a specific audio clip when the scene starts.
    /// </summary>
    public class SceneAmbientAudio : MonoBehaviour
    {
        [Header("Ambient Audio Settings")]
        [Tooltip("The ambient audio clip to be played in this scene.")]
        public AudioClip ambientClip;

        [Tooltip("The tag used for the ambient audio. Default is 'ambient'.")]
        public string ambientTag = "ambient"; // Default tag for ambient sounds

        private void Start()
        {
            // Start coroutine to check when loading is complete and play ambient audio
            StartCoroutine(PlayAmbientWhenLoadingCompletes());
        }

        /// <summary>
        /// Waits for the loading process to complete before playing the ambient audio.
        /// </summary>
        private IEnumerator PlayAmbientWhenLoadingCompletes()
        {
            // Wait until both AudioManager and LoadingManager are initialized and loading is complete
            while (AudioManager.Singleton == null || LoadingManager.Singleton == null || LoadingManager.Singleton.isLoading)
            {
                yield return null; // Wait for the next frame and check again
            }

            // Once loading is complete and AudioManager is available, play the ambient audio
            if (ambientClip != null && AudioManager.Singleton != null)
            {
                AudioManager.Singleton.PlayAmbientAudio(ambientClip, ambientTag);
            }
            else
            {
                Debug.LogWarning("Ambient clip or AudioManager is missing.");
            }
        }

    }
}
