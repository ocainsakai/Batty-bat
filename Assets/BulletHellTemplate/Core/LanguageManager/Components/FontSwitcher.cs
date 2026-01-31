using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LanguageManager
{
    /// <summary>
    /// Component that updates the attached Text or TMP_Text
    /// with the appropriate font, bold styling, and uppercase conversion
    /// based on the current language settings.
    /// This is intended for system texts that are set via script.
    /// </summary>
    public class FontSwitcher : MonoBehaviour
    {
        [Tooltip("Optional default text if none is set on the Text component.")]
        private string defaultText;

        private Text uiText;
        private TMP_Text tmpText;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the text components.
        /// </summary>
        private void Awake()
        {
            uiText = GetComponent<Text>();
            tmpText = GetComponent<TMP_Text>();

            if (uiText == null && tmpText == null)
            {
                Debug.LogWarning($"No Text or TMP_Text component found on {gameObject.name}");
            }

            // If a default text is provided and the current text is empty, assign it.
            if (!string.IsNullOrEmpty(defaultText))
            {
                if (uiText != null && string.IsNullOrEmpty(uiText.text))
                    uiText.text = defaultText;
                else if (tmpText != null && string.IsNullOrEmpty(tmpText.text))
                    tmpText.text = defaultText;
            }
        }

        /// <summary>
        /// Called before the first frame update.
        /// Waits until the LanguageManager is initialized, then subscribes to its event.
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => LanguageManager.Instance != null);
            yield return new WaitUntil(() => !string.IsNullOrEmpty(LanguageManager.Instance.currentLanguageID));

            LanguageManager.onLanguageChanged += UpdateFont;
            UpdateFont();
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// Unsubscribes from the language changed event.
        /// </summary>
        private void OnDestroy()
        {
            LanguageManager.onLanguageChanged -= UpdateFont;
        }

        /// <summary>
        /// Updates the text component's font, bold styling, and uppercase conversion
        /// based on the current language configuration, ensuring the font is changed
        /// before setting the final text content to avoid missing glyph warnings.
        /// </summary>
        private void UpdateFont()
        {
            var currentLanguage = LanguageManager.Instance.Languages.Find(
                lang => lang.LanguageID == LanguageManager.Instance.currentLanguageID);

            if (currentLanguage == null)
            {
                Debug.LogWarning("Current language not found in LanguageManager.");
                return;
            }

            // Update for Unity UI Text.
            if (uiText != null)
            {
                // Grab the raw text, removing any rich text formatting we might have applied earlier.
                string textContent = uiText.text.Replace("<b>", "").Replace("</b>", "");

                // Convert to uppercase if required.
                if (currentLanguage.applyUppercase)
                {
                    textContent = textContent.ToUpper();
                }

                // Step 1: Swap the font first, if needed.
                if (currentLanguage.changeFontOnSwap && currentLanguage.fontAsset != null)
                {
                    uiText.font = currentLanguage.fontAsset;
                }

                // Step 2: Now apply bold styling (if any) after the font is assigned.
                if (currentLanguage.applyBold)
                {
                    uiText.text = $"<b>{textContent}</b>";
                }
                else
                {
                    uiText.text = textContent;
                }
            }
            // Update for TextMeshPro Text.
            else if (tmpText != null)
            {
                // Get current text (no need to strip TMP formatting for bold).
                string textContent = tmpText.text;

                // Convert to uppercase if required.
                if (currentLanguage.applyUppercase)
                {
                    textContent = textContent.ToUpper();
                }

                // Step 1: Swap the font first, if needed.
                if (currentLanguage.changeFontOnSwap && currentLanguage.fontAssetTextMesh != null)
                {
                    tmpText.font = currentLanguage.fontAssetTextMesh;
                }

                // Step 2: Apply bold style and update the text.
                tmpText.fontStyle = currentLanguage.applyBold ? FontStyles.Bold : FontStyles.Normal;
                tmpText.text = textContent;
            }
        }
    }
}
