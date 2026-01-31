using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LanguageManager
{
    /// <summary>
    /// Component that updates the attached InputField or TMP_InputField
    /// with the appropriate font, bold styling, and uppercase conversion based
    /// on the current language settings from the LanguageManager.
    /// This version updates the placeholder font without altering its text content.
    /// </summary>
    public class WarpInputField : MonoBehaviour
    {
        [Tooltip("Reference to the Unity UI InputField component.")]
        private InputField uiInputField;

        [Tooltip("Reference to the TextMeshPro InputField component.")]
        private TMP_InputField tmpInputField;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Attempts to initialize the InputField components.
        /// </summary>
        private void Awake()
        {
            if (uiInputField == null)
                uiInputField = GetComponent<InputField>();

            if (tmpInputField == null)
                tmpInputField = GetComponent<TMP_InputField>();

            if (uiInputField == null && tmpInputField == null)
            {
                Debug.LogWarning($"No InputField or TMP_InputField component found on {gameObject.name}");
            }
        }

        /// <summary>
        /// Called before the first frame update.
        /// Waits until the LanguageManager is initialized and a language is set,
        /// then subscribes to the language change event and updates the input field.
        /// </summary>
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => LanguageManager.Instance != null);
            yield return new WaitUntil(() => !string.IsNullOrEmpty(LanguageManager.Instance.currentLanguageID));

            LanguageManager.onLanguageChanged += UpdateInputField;
            UpdateInputField();
        }

        /// <summary>
        /// Called when the object is destroyed.
        /// Unsubscribes from the language change event.
        /// </summary>
        private void OnDestroy()
        {
            LanguageManager.onLanguageChanged -= UpdateInputField;
        }

        /// <summary>
        /// Updates the InputField's font, bold styling, and uppercase conversion based on the current language settings.
        /// The update is performed according to the LanguageManager configuration:
        /// - If changeFontOnSwap is enabled, the font asset is swapped.
        /// - If applyBold is enabled, bold styling is applied.
        /// - If applyUppercase is enabled, the text is converted to uppercase.
        /// The placeholder text is not modified.
        /// </summary>
        private void UpdateInputField()
        {
            var currentLanguage = LanguageManager.Instance.Languages.Find(
                lang => lang.LanguageID == LanguageManager.Instance.currentLanguageID);

            if (currentLanguage == null)
            {
                Debug.LogWarning("Current language not found in LanguageManager.");
                return;
            }

            if (tmpInputField != null)
            {
                // Swap font asset if required.
                if (currentLanguage.changeFontOnSwap && currentLanguage.fontAssetTextMesh != null)
                {
                    tmpInputField.textComponent.font = currentLanguage.fontAssetTextMesh;

                    if (tmpInputField.placeholder is TMP_Text placeholderTMP)
                    {
                        placeholderTMP.font = currentLanguage.fontAssetTextMesh;
                    }
                }

                // Apply bold styling.
                tmpInputField.textComponent.fontStyle = currentLanguage.applyBold ? FontStyles.Bold : FontStyles.Normal;
                if (tmpInputField.placeholder is TMP_Text placeholderTMP2)
                {
                    placeholderTMP2.fontStyle = currentLanguage.applyBold ? FontStyles.Bold : FontStyles.Normal;
                }

                // Update the input field's text if there is content.
                if (!string.IsNullOrEmpty(tmpInputField.text))
                {
                    string updatedText = tmpInputField.text;
                    if (currentLanguage.applyUppercase)
                    {
                        updatedText = updatedText.ToUpper();
                    }
                    tmpInputField.textComponent.text = updatedText;
                    tmpInputField.text = updatedText;
                }
            }

            else if (uiInputField != null)
            {
                // Swap font if required.
                if (currentLanguage.changeFontOnSwap && currentLanguage.fontAsset != null)
                {
                    uiInputField.textComponent.font = currentLanguage.fontAsset;

                    if (uiInputField.placeholder is Text placeholderUI)
                    {
                        placeholderUI.font = currentLanguage.fontAsset;
                    }
                }

                // Update the input field's text if there is content.
                if (!string.IsNullOrEmpty(uiInputField.text))
                {
                    string updatedText = uiInputField.text;
                    updatedText = updatedText.Replace("<b>", "").Replace("</b>", "");

                    if (currentLanguage.applyBold)
                    {
                        updatedText = $"<b>{updatedText}</b>";
                    }

                    if (currentLanguage.applyUppercase)
                    {
                        updatedText = updatedText.ToUpper();
                    }

                    uiInputField.text = updatedText;
                }
            }
            else
            {
                Debug.LogWarning($"No InputField or TMP_InputField component found on {gameObject.name} to update.");
            }
        }
    }
}
