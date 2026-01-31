using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a shop item that can be purchased to unlock content.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShopItem", menuName = "BulletHellTemplate/Shop/ShopItem", order = 52)]
    public class ShopItem : ScriptableObject
    {
        public string itemId; // Unique identifier for the item
        public string itemTitle; // Title of the item
        public NameTranslatedByLanguage[] itemTitleTranslated;
        public string itemDescription; // Description of the item
        public DescriptionTranslatedByLanguage[] itemDescriptionTranslated;
        public int price; // Price of the item
        public string currency = "DM"; // Currency type for the item
        public Sprite itemIcon; // Icon of the item
        public string category;

        [Header("Package Settings")]
        public bool isCurrencyPackage; // Determines if the item is a package of currencies
        public List<CurrencyReward> currencyRewards; // List of currencies awarded if this is a currency package

        [Header("Icons unlocked upon purchase")]
        public IconItem[] icons; // Icons unlocked by purchasing this item

        [Header("Frames unlocked upon purchase")]
        public FrameItem[] frames; // Frames unlocked by purchasing this item

        [Header("Characters unlocked upon purchase")]
        public CharacterData[] characterData; // Characters unlocked by purchasing this item

        [Header("Items unlocked upon purchase")]
        public InventoryItem[] inventoryItems;

        /// <summary>
        /// Purchases the item and unlocks associated content if it has not been purchased.
        /// </summary>
        public async UniTask<bool> Buy()
        {
            if (PlayerSave.IsShopItemPurchased(itemId))
            {
                Debug.LogWarning("Item already purchased.");
                UIShopMenu.Singleton.FilterItemsByCategory(category);
                return false;
            }

            // Check if the player has enough currency
            int playerCurrency = MonetizationManager.GetCurrency(currency);
            if (playerCurrency < price)
            {
                Debug.LogWarning("Not enough currency to buy this item.");
                UIShopMenu.Singleton.ShowErrorMessage();
                return false;
            }

            RequestResult result = await MonetizationManager.Singleton.PurchaseShopItemAsync(this);
            if (result.Success)
            {
                UIShopMenu.Singleton.FilterItemsByCategory(category);
                return true;
            }
            UIShopMenu.Singleton.ShowErrorMessage();
            return false;
        }        
    }

    /// <summary>
    /// Represents a currency reward that includes the currency type and the amount to be awarded.
    /// </summary>
    [System.Serializable]
    public class CurrencyReward
    {
        public Currency currency; // The currency type
        public int amount; // The amount of currency to add
    }
}
