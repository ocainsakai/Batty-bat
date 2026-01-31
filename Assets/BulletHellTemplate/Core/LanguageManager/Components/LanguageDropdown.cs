using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LanguageManager
{
    /// <summary>
    /// Component that creates a dropdown of available languages and changes the language when selected.
    /// Supports both Unity UI Dropdown and TextMeshPro TMP_Dropdown.
    /// </summary>
    public class LanguageDropdown : MonoBehaviour
    {
        private Dropdown dropdown;
        private TMP_Dropdown tmpDropdown;
        private bool isTMPDropdown = false;

        private void Awake()
        {
            // Try to get the Unity UI Dropdown
            dropdown = GetComponent<Dropdown>();
            if (dropdown != null)
            {
                isTMPDropdown = false;
                // Start the coroutine to wait for the LanguageManager
                StartCoroutine(PopulateDropdownRoutine());
                dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
            else
            {
                // Try to get the TMP_Dropdown from TextMeshPro
                tmpDropdown = GetComponent<TMP_Dropdown>();
                if (tmpDropdown != null)
                {
                    isTMPDropdown = true;
                    StartCoroutine(PopulateDropdownRoutine());
                    tmpDropdown.onValueChanged.AddListener(OnTMPDropdownValueChanged);
                }
                else
                {
                    Debug.LogError("No Dropdown or TMP_Dropdown component found on this GameObject.");
                }
            }
        }

        /// <summary>
        /// Coroutine that waits until the LanguageManager instance is available, then populates the dropdown.
        /// </summary>
        private IEnumerator PopulateDropdownRoutine()
        {
            // Wait until LanguageManager instance is available
            yield return new WaitUntil(() => LanguageManager.Instance != null);
            PopulateDropdown();
        }

        /// <summary>
        /// Populates the dropdown with available languages from the LanguageManager.
        /// </summary>
        private void PopulateDropdown()
        {
            if (LanguageManager.Instance != null)
            {
                List<string> options = new List<string>();
                foreach (Language language in LanguageManager.Instance.Languages)
                {
                    options.Add(language.LanguageTitle);
                }

                int currentIndex = LanguageManager.Instance.Languages.FindIndex(
                    lang => lang.LanguageID == LanguageManager.Instance.currentLanguageID);

                if (isTMPDropdown && tmpDropdown != null)
                {
                    tmpDropdown.ClearOptions();
                    tmpDropdown.AddOptions(options);

                    // Set the dropdown value to the current language
                    if (currentIndex >= 0)
                    {
                        tmpDropdown.value = currentIndex;
                    }
                }
                else if (dropdown != null)
                {
                    dropdown.ClearOptions();
                    dropdown.AddOptions(options);

                    // Set the dropdown value to the current language
                    if (currentIndex >= 0)
                    {
                        dropdown.value = currentIndex;
                    }
                }
            }
            else
            {
                Debug.LogError("LanguageManager instance not found.");
            }
        }

        /// <summary>
        /// Called when the Unity UI Dropdown value changes.
        /// </summary>
        /// <param name="index">The index of the selected option.</param>
        private void OnDropdownValueChanged(int index)
        {
            if (LanguageManager.Instance != null && index >= 0 && index < LanguageManager.Instance.Languages.Count)
            {
                string selectedLanguageID = LanguageManager.Instance.Languages[index].LanguageID;
                LanguageManager.Instance.SetLanguage(selectedLanguageID);
            }
            else
            {
                Debug.LogError("Invalid language index selected.");
            }
        }

        /// <summary>
        /// Called when the TextMeshPro TMP_Dropdown value changes.
        /// </summary>
        /// <param name="index">The index of the selected option.</param>
        private void OnTMPDropdownValueChanged(int index)
        {
            if (LanguageManager.Instance != null && index >= 0 && index < LanguageManager.Instance.Languages.Count)
            {
                string selectedLanguageID = LanguageManager.Instance.Languages[index].LanguageID;
                LanguageManager.Instance.SetLanguage(selectedLanguageID);
            }
            else
            {
                Debug.LogError("Invalid language index selected.");
            }
        }
    }
}
