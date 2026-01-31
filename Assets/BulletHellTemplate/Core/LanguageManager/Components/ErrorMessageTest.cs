using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace LanguageManager
{
    /// <summary>
    /// This script is used to display an error message from the LanguageManager system
    /// on a Text or TextMeshProUGUI component for 2 seconds, then hides the message.
    /// </summary>
    public class ErrorMessageTest : MonoBehaviour
    {
        public TextMeshProUGUI textMeshProUGUI; // The TextMeshPro component to display the error message
        public UnityEngine.UI.Text text; // The Unity Text component to display the error message
        public string textId; // The ID of the text in the LanguageManager

        private Coroutine hideMessageCoroutine;

        /// <summary>
        /// Retrieves the error message from LanguageManager using the provided textId
        /// and displays it on the specified Text or TextMeshProUGUI component in red color.
        /// The message is displayed for 2 seconds before being hidden.
        /// </summary>
        public void ShowErrorMessage()
        {
            try
            {
                // Get the error message from the LanguageManager using the textId
                string errorMessage = LanguageManager.Instance.GetTextEntryByID(textId);

                // Check if TextMeshProUGUI is assigned and set the error message
                if (textMeshProUGUI != null)
                {
                    textMeshProUGUI.text = errorMessage;
                    textMeshProUGUI.color = Color.red; // Set the text color to red
                }
                // If not, check if Unity Text is assigned and set the error message
                else if (text != null)
                {
                    text.text = errorMessage;
                    text.color = Color.red; // Set the text color to red
                }
                else
                {
                    Debug.LogError("No Text or TextMeshProUGUI component is assigned to display the error message.");
                    return;
                }

                // Start a coroutine to hide the error message after 2 seconds
                if (hideMessageCoroutine != null)
                {
                    StopCoroutine(hideMessageCoroutine); // Ensure no overlapping coroutines
                }
                hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay(2.0f));
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to retrieve or display the error message: " + ex.Message);
            }
        }

        /// <summary>
        /// Coroutine that hides the error message after a specified delay.
        /// </summary>
        /// <param name="delay">Time in seconds before hiding the message.</param>
        /// <returns>IEnumerator to allow coroutine execution.</returns>
        private IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Hide the message by clearing the text
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = "";
            }
            else if (text != null)
            {
                text.text = "";
            }
        }
    }
}
