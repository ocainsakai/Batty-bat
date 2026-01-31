using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents an entry in the IAP shop for purchasing an item.
    /// Displays the item information and allows the user to initiate a purchase.
    /// </summary>
    public class ShopIAPEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Text for displaying the item's name.")]
        public TextMeshProUGUI itemName; // Text for the item name

        [Tooltip("Text for displaying the price of the item.")]
        public TextMeshProUGUI price; // Text for the item price

        public TextMeshProUGUI description;

        [Tooltip("Image for displaying the item's icon.")]
        public Image itemIcon; // Image for the item icon

        private IAPItem iapItem; // Reference to the IAP item

        /// <summary>
        /// Sets the UI elements with the information of the given IAP item.
        /// </summary>
        /// <param name="iapItem">The IAP item to display in this entry.</param>
        public void SetItemInfo(IAPItem iapItem , string _itemName, string item_description)
        {
            this.iapItem = iapItem;
            itemName.text = _itemName;
            description.text = item_description;
            price.text = "$" + iapItem.priceInUSD.ToString("F2"); // Display price in USD with two decimal places
            itemIcon.sprite = iapItem.itemIcon;
        }

        /// <summary>
        /// Opens the purchase confirmation popup for the selected item.
        /// </summary>
        public void OpenConfirmBuyPopup()
        {
            UIShopIAP.Singleton.OpenBuyPopup(iapItem);
        }
    }
}
