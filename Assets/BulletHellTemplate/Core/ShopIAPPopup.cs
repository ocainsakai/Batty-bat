using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the confirmation popup for purchasing an IAP item with real money.
    /// Displays the selected item's details and confirms the purchase.
    /// </summary>
    public class ShopIAPPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Image displaying the selected item's icon.")]
        public Image itemIcon; // Image for the item icon

        [Tooltip("Text displaying the selected item's name.")]
        public TextMeshProUGUI itemName; // Text for the item name

        [Tooltip("Text displaying the description for the selected item.")]
        public TextMeshProUGUI itemDescription; // Text for the item description

        private IAPItem iapItem; // Reference to the selected IAP item

        /// <summary>
        /// Sets the information in the confirmation popup with the selected item's details.
        /// </summary>
        /// <param name="iapItem">The IAP item to confirm purchase for.</param>
        public void SetPopupInfo(IAPItem iapItem)
        {
            this.iapItem = iapItem;
            itemName.text = iapItem.itemName;
            itemDescription.text = iapItem.itemDescription; // Display the item description
            itemIcon.sprite = iapItem.itemIcon;
        }

        /// <summary>
        /// Confirms the purchase of the selected item and closes the popup.
        /// </summary>
        public void ConfirmBuy()
        {
            IAPManager.Singleton.BuyCurrency(iapItem.itemId); // Use itemId to process purchase
            gameObject.SetActive(false);
        }
    }
}
