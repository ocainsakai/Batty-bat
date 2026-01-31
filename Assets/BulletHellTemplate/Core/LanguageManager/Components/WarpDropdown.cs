using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LanguageManager
{
    /// <summary>
    /// Component that updates the attached Dropdown or TMP_Dropdown
    /// with the appropriate font, bold styling, and uppercase conversion
    /// based on the current language settings from the LanguageManager.
    /// This is intended for system dropdown texts that are filled by the system.
    /// </summary>
    public class WarpDropdown : MonoBehaviour
    {
        private Dropdown uiDropdown;
        private TMP_Dropdown tmpDropdown;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Attempts to initialize the Dropdown components.
        /// </summary>
        private void Awake()
        {
            uiDropdown = GetComponent<Dropdown>();
            tmpDropdown = GetComponent<TMP_Dropdown>();

            if (uiDropdown == null && tmpDropdown == null)
            {
                Debug.LogWarning($"No Dropdown or TMP_Dropdown component found on {gameObject.name}");
            }
        }

        /// <summary>
        /// Called before the first frame update.
        /// Waits until the LanguageManager is initialized and a language is set,
        /// then subscribes to the language change event and updates the dropdown.
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => LanguageManager.Instance != null);
            yield return new WaitUntil(() => !string.IsNullOrEmpty(LanguageManager.Instance.currentLanguageID));

            LanguageManager.onLanguageChanged += UpdateDropdown;
            UpdateDropdown();
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// Unsubscribes from the language change event.
        /// </summary>
        private void OnDestroy()
        {
            LanguageManager.onLanguageChanged -= UpdateDropdown;
        }

        /// <summary>
        /// Updates the Dropdown's caption text with the appropriate font, bold styling, and uppercase conversion
        /// based on the current language configuration.
        /// </summary>
        private void UpdateDropdown()
        {
            var currentLanguage = LanguageManager.Instance.Languages.Find(
                lang => lang.LanguageID == LanguageManager.Instance.currentLanguageID);

            if (currentLanguage == null)
            {
                Debug.LogWarning("Current language not found in LanguageManager.");
                return;
            }

            // For Unity UI Dropdown.
            if (uiDropdown != null)
            {
                if (uiDropdown.captionText != null)
                {
                    // Swap font if required.
                    if (currentLanguage.changeFontOnSwap && currentLanguage.fontAsset != null)
                    {
                        uiDropdown.captionText.font = currentLanguage.fontAsset;
                    }

                    // Get current text and remove any previous bold formatting.
                    string textContent = uiDropdown.captionText.text;
                    textContent = textContent.Replace("<b>", "").Replace("</b>", "");

                    // Convert text to uppercase if required.
                    if (currentLanguage.applyUppercase)
                    {
                        textContent = textContent.ToUpper();
                    }

                    // Apply bold formatting using rich text if enabled.
                    if (currentLanguage.applyBold)
                    {
                        textContent = $"<b>{textContent}</b>";
                    }

                    uiDropdown.captionText.text = textContent;
                }
            }
            // For TextMeshPro Dropdown.
            else if (tmpDropdown != null)
            {
                if (tmpDropdown.captionText != null)
                {
                    // Swap font asset if required.
                    if (currentLanguage.changeFontOnSwap && currentLanguage.fontAssetTextMesh != null)
                    {
                        tmpDropdown.captionText.font = currentLanguage.fontAssetTextMesh;
                    }

                    // Get current text.
                    string textContent = tmpDropdown.captionText.text;

                    // Convert text to uppercase if required.
                    if (currentLanguage.applyUppercase)
                    {
                        textContent = textContent.ToUpper();
                    }

                    // Set font style for bold.
                    tmpDropdown.captionText.fontStyle = currentLanguage.applyBold ? FontStyles.Bold : FontStyles.Normal;
                    tmpDropdown.captionText.text = textContent;
                }
            }
        }
    }
}
