using System.Collections;
using TMPro;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the In-App Purchase (IAP) shop, displaying purchasable items and handling purchase confirmations.
    /// </summary>
    public class UIShopIAP : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Popup used to confirm the purchase of an IAP item.")]
        public ShopIAPPopup buyPopup; // Popup for confirming the purchase

        [Tooltip("Prefab for each IAP item entry in the shop.")]
        public ShopIAPEntry shopEntryPrefab; // Prefab for each IAP item

        [Tooltip("Container to hold the IAP item entries.")]
        public Transform containerItems; // Container for IAP item entries

        public static UIShopIAP Singleton; // Singleton instance for easy access

        [Tooltip("Text used to display error messages.")]
        public TextMeshProUGUI errorMessage; // Error message text element
        private string currentLang;
        private void Awake()
        {
            // Ensure Singleton pattern
            if (Singleton == null)
            {
                Singleton = this;

            }
        }

        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
        }

        private void Start()
        {
            // Load and display available IAP items from IAPManager when the shop starts
            LoadIAPItems();
        }

        /// <summary>
        /// Loads the IAP items from IAPManager and instantiates their entries in the UI.
        /// </summary>
        private void LoadIAPItems()
        {
            // Clear existing entries before loading new ones
            ClearItems();

            // Access the purchasable items directly from IAPManager
            foreach (IAPItem iapItem in IAPManager.Singleton.purchasableItems)
            {
                ShopIAPEntry shopEntry = Instantiate(shopEntryPrefab, containerItems);
                string itemNameTranslated = GetTranslatedString(iapItem.itemNameTranslated, iapItem.itemName,currentLang);
                string itemDescriptionTranslated = GetTranslatedString(iapItem.itemDescriptionTranslated, iapItem.itemDescription,currentLang);
                shopEntry.SetItemInfo(iapItem,itemNameTranslated,itemDescriptionTranslated); // Set IAP item info
            }
        }

        /// <summary>
        /// Clears all IAP item entries from the UI containerPassItems.
        /// </summary>
        private void ClearItems()
        {
            foreach (Transform child in containerItems)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Opens the purchase confirmation popup with the selected IAP item's details.
        /// </summary>
        /// <param name="iapItem">The IAP item selected for purchase.</param>
        public void OpenBuyPopup(IAPItem iapItem)
        {
            buyPopup.SetPopupInfo(iapItem);
            buyPopup.gameObject.SetActive(true);
        }

        public void ShowErrorMessage()
        {

        }

        /// <summary>
        /// Coroutine to clear the error message after a specified delay.
        /// </summary>
        /// <param name="delay">The delay in seconds before clearing the message.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator ClearErrorMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            errorMessage.text = "";
        }

        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        private string GetTranslatedString(DescriptionTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }
    }
}
