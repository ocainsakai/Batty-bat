using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents an in-app purchase item that can be bought in the store.
    /// Contains the necessary details for displaying and processing the item.
    /// </summary>
    [CreateAssetMenu(fileName = "New IAP Item", menuName = "IAP/IAP Item", order = 1)]
    public class IAPItem : ScriptableObject
    {
        [Header("IAP Item Details")]
        [Tooltip("The unique identifier for this IAP item.")]
        public string itemId; // Unique ID for the IAP item

        [Tooltip("The name of the IAP item.")]
        public string itemName; // Display name of the IAP item
        public NameTranslatedByLanguage[] itemNameTranslated;

        [Tooltip("The description of the IAP item.")]
        public string itemDescription; // Description for the IAP item

        public DescriptionTranslatedByLanguage[] itemDescriptionTranslated;

        [Tooltip("The icon for the IAP item.")]
        public Sprite itemIcon; // Icon to display in the shop

        [Tooltip("The price of the item in USD.")]
        public float priceInUSD; // Price in US dollars (used by the stores for automatic conversion)

        [Tooltip("The amount of currency the player will receive upon purchase.")]
        public int currencyAmount; // Amount of currency granted

        [Tooltip("The currency related to this IAP item.")]
        public Currency associatedCurrency; // The currency ScriptableObject linked to this item
    }
}
