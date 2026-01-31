using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LanguageManager
{
    /// <summary>
    /// Component that updates the attached Text or TextMeshProUGUI
    /// with a translated string from the LanguageManager based on the provided textID.
    /// It also updates the font asset, style (e.g., bold), and converts text to uppercase if configured.
    /// </summary>
    public class WarpText : MonoBehaviour
    {
        [Tooltip("The ID used to lookup the translation for this text.")]
        public string textID;

        private Text uiText;
        private TextMeshProUGUI tmpText;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Initializes the text components.
        /// </summary>
        private void Awake()
        {
            uiText = GetComponent<Text>();
            tmpText = GetComponent<TextMeshProUGUI>();

            if (uiText == null && tmpText == null)
            {
                Debug.LogWarning($"No Text or TextMeshProUGUI component found on {gameObject.name}");
            }
        }

        /// <summary>
        /// Called before the first frame update.
        /// Waits until the LanguageManager is initialized before updating the text.
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => LanguageManager.Instance != null);
            yield return new WaitUntil(() => !string.IsNullOrEmpty(LanguageManager.Instance.currentLanguageID));

            LanguageManager.onLanguageChanged += UpdateText;
            UpdateText();
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// Unsubscribes from the language changed event.
        /// </summary>
        private void OnDestroy()
        {
            LanguageManager.onLanguageChanged -= UpdateText;
        }

        /// <summary>
        /// Updates the text component with the translation for the provided textID,
        /// and applies font changes and style (e.g., bold) based on the current language settings.
        /// Additionally, converts the text to uppercase if the configuration is enabled.
        /// </summary>
        private void UpdateText()
        {
            // Get the translated text from the LanguageManager.
            string translatedText = LanguageManager.Instance.GetTextEntryByID(textID);

            // Retrieve the current language configuration.
            var currentLanguage = LanguageManager.Instance.Languages.Find(
                lang => lang.LanguageID == LanguageManager.Instance.currentLanguageID);

            if (currentLanguage == null)
            {
                Debug.LogWarning("Current language not found in LanguageManager.");
                return;
            }

            // Convert text to uppercase if required.
            if (currentLanguage.applyUppercase)
            {
                translatedText = translatedText.ToUpper();
            }

            // Update UnityEngine.UI.Text component.
            if (uiText != null)
            {
                // Update font asset if changeFontOnSwap is enabled.
                if (currentLanguage.changeFontOnSwap && currentLanguage.fontAsset != null)
                {
                    uiText.font = currentLanguage.fontAsset;
                }

                // Apply bold styling using rich text formatting if enabled.
                if (currentLanguage.applyBold)
                {
                    uiText.text = $"<b>{translatedText}</b>";
                }
                else
                {
                    uiText.text = translatedText;
                }
            }
            // Update TextMeshProUGUI component.
            else if (tmpText != null)
            {
                // Update font asset if changeFontOnSwap is enabled.
                if (currentLanguage.changeFontOnSwap && currentLanguage.fontAssetTextMesh != null)
                {
                    tmpText.font = currentLanguage.fontAssetTextMesh;
                }

                // Apply bold style based on the language configuration.
                tmpText.fontStyle = currentLanguage.applyBold ? FontStyles.Bold : FontStyles.Normal;
                tmpText.text = translatedText;
            }
            else
            {
                Debug.LogWarning($"No Text or TMP component found on {gameObject.name} to update.");
            }
        }
    }
}
