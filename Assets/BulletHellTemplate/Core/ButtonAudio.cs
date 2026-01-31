using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Plays audio when a button or toggle is clicked. Uses the AudioManager to play the audio globally.
    /// </summary>
    public class ButtonAudio : MonoBehaviour
    {
        [Tooltip("The audio clip that plays when the button is clicked.")]
        public AudioClip buttonClickAudio;

        [Tooltip("The tag used for categorizing the audio (Master, VFX, Ambient, Custom).")]
        public string audioTag = "master";

        private Button button;
        private Toggle toggle;

        private void Awake()
        {
            // Get the button or toggle component
            button = GetComponent<Button>();
            toggle = GetComponent<Toggle>();

            // Add the event to the button if it exists
            if (button != null)
            {
                button.onClick.AddListener(PlayButtonClickAudio);
            }

            // Add the event to the toggle if it exists
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(delegate { PlayButtonClickAudio(); });
            }

            // Log a warning if neither button nor toggle is found
            if (button == null && toggle == null)
            {
                Debug.LogWarning("No Button or Toggle component found on " + gameObject.name);
            }
        }

        /// <summary>
        /// Plays the button click or toggle audio.
        /// </summary>
        public void PlayButtonClickAudio()
        {
            if (buttonClickAudio != null)
            {
                AudioManager.Singleton.PlayAudio(buttonClickAudio, audioTag);
            }
            else
            {
                Debug.LogWarning("Button click audio not assigned.");
            }
        }
    }
}
