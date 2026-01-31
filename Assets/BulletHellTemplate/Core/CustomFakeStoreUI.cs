using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class CustomFakeStoreUI : MonoBehaviour
    {
        public Button confirmButton;
        public Button cancelButton;
        public TextMeshProUGUI productTitle;
        public TextMeshProUGUI productPrice;

        private Action<Product, bool> onCompleteCallback;
        private Product currentProduct;

        public void Show(Product product, Action<Product, bool> callback)
        {
            gameObject.SetActive(true);
            productTitle.text = product.metadata.localizedTitle;
            productPrice.text = product.metadata.localizedPriceString;
            onCompleteCallback = callback;
            currentProduct = product;

            confirmButton.onClick.AddListener(() => OnConfirm(true));
            cancelButton.onClick.AddListener(() => OnConfirm(false));
        }

        private void OnConfirm(bool confirmed)
        {
            confirmButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();

            onCompleteCallback?.Invoke(currentProduct, confirmed);
            gameObject.SetActive(false);
        }
    }
}
