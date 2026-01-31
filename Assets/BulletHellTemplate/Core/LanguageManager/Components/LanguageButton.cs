using UnityEngine;
using UnityEngine.UI;

namespace LanguageManager
{
    /// <summary>
    /// Component that switches the language when the button is clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LanguageButton : MonoBehaviour
    {
        /// <summary>
        /// The language ID to switch to when the button is clicked.
        /// </summary>
        public string languageID;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClicked);
        }

        /// <summary>
        /// Called when the button is clicked.
        /// </summary>
        private void OnButtonClicked()
        {
            if (LanguageManager.Instance != null)
            {
                LanguageManager.Instance.SetLanguage(languageID);
            }
            else
            {
                Debug.LogError("LanguageManager instance not found.");
            }
        }
    }
}
