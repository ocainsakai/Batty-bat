using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
namespace BulletHellTemplate
{
    public class UIShopMenu : MonoBehaviour
    {       
        public ShopBuyPopup buyPopup; // Popup for item purchase confirmation
        public ShopEntry shopEntryPrefab; // Prefab for shop entries
        public Transform containerShopItems; // Container to hold shop entries        
        public TextMeshProUGUI errorMessage;

        public string notEnoughCurrency = "Not enough currency to buy this item.";
        public NameTranslatedByLanguage[] notEnoughCurrencyTranslated;

        public UnityEvent OnOpenShop;
        [Header("On open Menu filter category")]
        public string category = "character";
        private string currentLang;
        public static UIShopMenu Singleton; // Singleton Singleton of the shop menu
        private void Awake()
        {
            // Ensure singleton Singleton
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
            InitialFilter();
        }
       
        /// <summary>
        /// Opens the purchase confirmation popup with the selected item's details.
        /// </summary>
        /// <param name="itemId">The ID of the item.</param>
        /// <param name="itemName">The name of the item.</param>
        /// <param name="itemDescription">The description of the item.</param>
        /// <param name="itemIcon">The icon of the item.</param>
        public void OpenBuyPopup(string itemId, string itemName, string itemDescription, Sprite itemIcon)
        {
            buyPopup.SetPopupInfo(itemId, itemName, itemDescription, itemIcon);
            buyPopup.gameObject.SetActive(true);
        }

        /// <summary>
        /// Confirms the purchase of an item if the player has sufficient currency.
        /// </summary>
        /// <param name="itemId">The ID of the item to buy.</param>
        public async void BuyItemConfirm(string itemId)
        {
            foreach (ShopItem item in GameInstance.Singleton.shopData)
            {
                if (item.itemId == itemId)
                {
                    string purchasedItemCategory = item.category;
                    bool result = await item.Buy();
                    if (result)
                        FilterItemsByCategory(purchasedItemCategory);
                    return;
                }
            }
        }

        /// <summary>
        /// Clears all shop items from the containerPassItems.
        /// </summary>
        public void ClearShopItems()
        {
            foreach (Transform child in containerShopItems)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Loads and displays shop items that have not been purchased.
        /// </summary>
        public void LoadShopItems()
        {
            ClearShopItems();

            // Display items that are not purchased
            foreach (ShopItem item in GameInstance.Singleton.shopData)
            {
                if (!PlayerSave.IsShopItemPurchased(item.itemId))
                {
                    ShopEntry shopEntry = Instantiate(shopEntryPrefab, containerShopItems);
                    string itemTitleTranslated = GetTranslatedString(item.itemTitleTranslated, item.itemTitle, currentLang);
                    shopEntry.shopItemName.text = itemTitleTranslated;
                    shopEntry.price.text = item.price.ToString();
                    string itemDescriptionTranslated = GetTranslatedString(item.itemDescriptionTranslated, item.itemDescription, currentLang);
                    shopEntry.description.text = itemDescriptionTranslated;
                    shopEntry.shopItemIcon.sprite = item.itemIcon;
                    shopEntry.currency.sprite = MonetizationManager.Singleton.GetCurrencyIcon(item.currency);
                    shopEntry.SetShopItemID(item.itemId);
                }
            }
        }

        /// <summary>
        /// Filters shop items by a specific category.
        /// </summary>
        /// <param name="filterCategory">The category to filter shop items by.</param>
        public void FilterItemsByCategory(string filterCategory)
        {
            ClearShopItems();

            foreach (ShopItem item in GameInstance.Singleton.shopData)
            {
                if (item.category.Equals(filterCategory, System.StringComparison.OrdinalIgnoreCase) && !PlayerSave.IsShopItemPurchased(item.itemId))
                {
                    ShopEntry shopEntry = Instantiate(shopEntryPrefab, containerShopItems);
                    shopEntry.shopItemName.text = GetTranslatedString(item.itemTitleTranslated, item.itemTitle, currentLang);
                    shopEntry.price.text = item.price.ToString();
                    shopEntry.description.text = GetTranslatedString(item.itemDescriptionTranslated, item.itemDescription, currentLang);
                    shopEntry.shopItemIcon.sprite = item.itemIcon;
                    shopEntry.currency.sprite = MonetizationManager.Singleton.GetCurrencyIcon(item.currency);
                    shopEntry.SetShopItemID(item.itemId);
                }
            }
        }

        /// <summary>
        /// Filters shop items based on the category specified in the script's category field.
        /// </summary>
        public void InitialFilter()
        {
            FilterItemsByCategory(category);
            OnOpenShop.Invoke();
        }

        public void ShowErrorMessage()
        {
            string translatedError = GetTranslatedString(notEnoughCurrencyTranslated,notEnoughCurrency,currentLang);
            errorMessage.text = translatedError;
            StartCoroutine(ClearErrorMessageAfterDelay(2.0f));
        }

        /// <summary>
        /// Coroutine to clear the error message after a delay.
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
