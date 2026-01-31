using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class ShopEntry : MonoBehaviour
    {
        public TextMeshProUGUI shopItemName;
        public TextMeshProUGUI price;
        public TextMeshProUGUI description;
        public Image shopItemIcon;
        public Image currency;
        private string shopId;

        public void SetShopItemID(string _shopId)
        {
            shopId = _shopId;
        }

        /// <summary>
        /// Abre o popup de confirmação de compra para o item selecionado.
        /// </summary>
        public void OpenConfirmBuyPopup()
        {
            UIShopMenu.Singleton.OpenBuyPopup(shopId, shopItemName.text, description.text, shopItemIcon.sprite);
        }
    }
}
