using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace LanguageManager
{
    public class LocalizedText : MonoBehaviour
    {
        public string textID;
        public List<Translation> translations = new List<Translation>();

        private Text uiText;
        private TextMeshProUGUI tmpText;

        private void Awake()
        {
            uiText = GetComponent<Text>();
            tmpText = GetComponent<TextMeshProUGUI>();

            LanguageManager.onLanguageChanged += UpdateText;
        }

        private void OnDestroy()
        {
            LanguageManager.onLanguageChanged -= UpdateText;
        }

        private void Start()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            string currentLanguageID = LanguageManager.Instance.currentLanguageID;
            foreach (Translation translation in translations)
            {
                if (translation.LanguageID == currentLanguageID)
                {
                    if (uiText != null)
                    {
                        uiText.text = translation.TranslatedText;
                    }
                    else if (tmpText != null)
                    {
                        tmpText.text = translation.TranslatedText;
                    }
                    break;
                }
            }
        }
    }
}
