using UnityEngine;
using UnityEngine.Purchasing;
using System;
using UnityEngine.Purchasing.Extension;

namespace BulletHellTemplate
{
    /// <summary>
    /// Enum to switch between Test Mode and Production Mode.
    /// In Test Mode, purchases will be simulated without the need for a Google Developer account.
    /// In Production Mode, real purchases will be processed through the Google Play or Apple App Store.
    /// </summary>
    public enum PurchaseMode
    {
        TestMode,
        ProductionMode
    }

    /// <summary>
    /// Manages the In-App Purchasing system for the game, handling both Test and Production modes.
    /// Supports purchasing virtual currencies and other in-game items.
    /// </summary>
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public static IAPManager Singleton;

        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;

        [Header("IAP Configuration")]
        [Tooltip("List of IAP items that can be purchased.")]
        public IAPItem[] purchasableItems; // List of IAP items as ScriptableObjects

        [Tooltip("Set whether the game should run in Test Mode or Production Mode.")]
        public PurchaseMode purchaseMode = PurchaseMode.ProductionMode;

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializePurchasing();
        }

        /// <summary>
        /// Initializes the purchasing system for IAP, based on the selected PurchaseMode.
        /// If TestMode is selected, purchases will be simulated.
        /// </summary>
        public void InitializePurchasing()
        {
            if (IsInitialized())
                return;

            // Choose the appropriate module based on the purchase mode
            var module = (purchaseMode == PurchaseMode.TestMode)
                ? StandardPurchasingModule.Instance(AppStore.NotSpecified)
                : StandardPurchasingModule.Instance();

            var builder = ConfigurationBuilder.Instance(module);

            // Add purchasable IAP items to the IAP system using their USD price
            foreach (var iapItem in purchasableItems)
            {
                builder.AddProduct(iapItem.itemId, ProductType.Consumable);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        /// <summary>
        /// Checks whether the IAP system is initialized.
        /// </summary>
        /// <returns>Returns true if the IAP system is initialized, otherwise false.</returns>
        private bool IsInitialized()
        {
            return storeController != null && storeExtensionProvider != null;
        }

        /// <summary>
        /// Initiates the purchase of an IAP item, based on the provided itemId.
        /// If in TestMode, the purchase will be simulated.
        /// </summary>
        /// <param name="itemId">The unique ID of the IAP item to purchase.</param>
        public void BuyCurrency(string itemId)
        {
            if (IsInitialized())
            {
                Product product = storeController.products.WithID(itemId);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log($"Attempting to buy product: {product.definition.id}");
                    storeController.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyCurrency: Product not found or not available for purchase.");
                }
            }
            else
            {
                Debug.Log("BuyCurrency: Not initialized.");
            }
        }

        /// <summary>
        /// Called when the purchase of a product is complete.
        /// Processes the purchased item and grants it to the player.
        /// </summary>
        /// <param name="product">The product that was purchased.</param>
        public void OnPurchaseComplete(Product product)
        {
            foreach (var iapItem in purchasableItems)
            {
                if (product.definition.id == iapItem.itemId)
                {
                    int currentAmount = MonetizationManager.GetCurrency(iapItem.associatedCurrency.coinID);
                    int newAmount = currentAmount + iapItem.currencyAmount;

                    // Ensure the purchase is securely processed before updating the currency
                    MonetizationManager.SetCurrency(iapItem.associatedCurrency.coinID, newAmount);

                    Debug.Log($"Purchase complete: {iapItem.itemName}, new amount: {newAmount}");
                }
            }
        }

        /// <summary>
        /// Called when a purchase fails, providing the reason for the failure.
        /// </summary>
        /// <param name="product">The product that failed to purchase.</param>
        /// <param name="failureReason">The reason for the purchase failure.</param>
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"Purchase failed: {product.definition.id}, reason: {failureReason}");
        }

        /// <summary>
        /// Required by IDetailedStoreListener to handle initialization failure with detailed information.
        /// </summary>
        /// <param name="error">The type of initialization failure.</param>
        /// <param name="message">Additional message regarding the failure.</param>
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"IAP Initialization Failed: {error}, Message: {message}");
        }

        /// <summary>
        /// Called when a purchase fails, providing detailed information about the failure.
        /// </summary>
        /// <param name="product">The product that failed to purchase.</param>
        /// <param name="failureDescription">The detailed description of the failure.</param>
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.LogError($"Purchase failed: {product.definition.id}, reason: {failureDescription.reason}, message: {failureDescription.message}");
        }

        /// <summary>
        /// Processes the purchase and delivers the purchased item to the player.
        /// In TestMode, the process is simulated.
        /// </summary>
        /// <param name="purchaseEvent">The purchase event arguments.</param>
        /// <returns>A PurchaseProcessingResult indicating the result of the processing.</returns>
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            // Validate the purchased product before processing
            Product purchasedProduct = purchaseEvent.purchasedProduct;

            if (purchasedProduct != null && !string.IsNullOrEmpty(purchasedProduct.definition.id))
            {
                Debug.Log($"Processing purchase for {purchasedProduct.definition.id}");

                // Ensure the purchased product is one of the defined IAP items
                foreach (var iapItem in purchasableItems)
                {
                    if (purchasedProduct.definition.id == iapItem.itemId)
                    {
                        int currentAmount = MonetizationManager.GetCurrency(iapItem.associatedCurrency.coinID);
                        int newAmount = currentAmount + iapItem.currencyAmount;

                        // Apply the purchased currency to the player account
                        MonetizationManager.SetCurrency(iapItem.associatedCurrency.coinID, newAmount);

                        Debug.Log($"Purchase processed: {iapItem.itemName}, new total: {newAmount}");
                        return PurchaseProcessingResult.Complete;
                    }
                }
            }

            Debug.LogError("Purchase failed: Invalid product or product definition.");
            return PurchaseProcessingResult.Pending; // Mark as pending if there are issues
        }

        /// <summary>
        /// Called when the IAP system is successfully initialized.
        /// This method sets the internal store controller and extension provider.
        /// </summary>
        /// <param name="controller">The store controller for managing IAP products.</param>
        /// <param name="extensions">The store extension provider.</param>
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensionProvider = extensions;
        }

        /// <summary>
        /// Called when the IAP initialization fails.
        /// </summary>
        /// <param name="error">The reason for the initialization failure.</param>
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("IAP Initialization Failed: " + error);
        }
    }
}
