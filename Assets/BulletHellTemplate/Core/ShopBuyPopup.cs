using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class ShopBuyPopup : MonoBehaviour
    {
        public Image shopItemIcon;
        public TextMeshProUGUI shopItemName;
        public TextMeshProUGUI shopItemDescription;
        private string itemId;

        /// <summary>
        /// Sets the information for the purchase confirmation popup.
        /// This method configures the popup with the item ID, name, description, and icon.
        /// </summary>
        /// <param name="_itemId">The ID of the item to be purchased.</param>
        /// <param name="_itemName">The name of the item to be displayed.</param>
        /// <param name="_itemDescription">The description of the item to be displayed.</param>
        /// <param name="itemIcon">The icon of the item to be displayed.</param>
        public void SetPopupInfo(string _itemId, string _itemName, string _itemDescription, Sprite itemIcon)
        {
            itemId = _itemId;
            shopItemName.text = _itemName;
            shopItemDescription.text = _itemDescription;
            shopItemIcon.sprite = itemIcon;
        }

        /// <summary>
        /// Confirms the purchase of the item.
        /// This method triggers the item purchase confirmation process and deactivates the popup.
        /// </summary>
        public void ConfirmBuy()
        {
            UIShopMenu.Singleton.BuyItemConfirm(itemId);
            gameObject.SetActive(false);
        }
    }
}
