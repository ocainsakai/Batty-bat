using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles saving and loading inventory-related data locally (such as item upgrade levels),
    /// and optionally syncing specific items to the backend.
    /// </summary>
    public class InventorySave : MonoBehaviour
    {
        public static InventorySave Singleton;

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Attempts to change the upgrade level of a purchased item.  
        /// The backend validates the operation; on success the change is cached locally.
        /// </summary>
        public static async UniTask<RequestResult> UpgradeItemAsync(string uniqueItemGuid, InventoryItem inventorySO)
        {
            return await BackendManager.Service.UpgradeInventoryItemAsync(uniqueItemGuid,inventorySO);
        }

        /// <summary>
        /// Gets the item upgrade level from local data, returns 0 if not found.
        /// </summary>
        /// <param name="uniqueItemGuid">GUID of the purchased item.</param>
        /// <returns>The upgrade level, or 0 if not found.</returns>
        public static int GetItemUpgradeLevel(string uniqueItemGuid)
        {
            return PlayerSave.GetItemUpgradeLevel(uniqueItemGuid);
        }

        /// <summary>
        /// Equips an item (assigns a GUID) in the given character's slot, saving to Firestore if needed.
        /// If uniqueItemGuid is empty, it means unequip and calls RemoveCharacterItemAsync.
        /// </summary>
        public static async UniTask<RequestResult> SetEquippedItemForSlotAsync(int characterId, string slotName, string uniqueItemGuid)
        {
            RequestResult res = await BackendManager.Service.SetCharacterItemAsync(characterId, slotName, uniqueItemGuid);
            return res;
        }

        /// <summary>
        /// Returns the unique item GUID equipped in a specific slot for a given character,
        /// or empty string if none is equipped.
        /// </summary>
        public static string GetEquippedItemForSlot(int characterId, string slotName)
        {
            return PlayerSave.GetCharacterSlotItem(characterId, slotName) ?? "";
        }

        /// <summary>
        /// Deletes a purchased inventory item everywhere (backend first, then local cache).
        /// </summary>
        public static async UniTask<RequestResult> DeletePurchasedItemAsync(string uniqueItemGuid)
        {
            RequestResult res = await BackendManager.Service
                .DeletePurchasedInventoryItemAsync(uniqueItemGuid);

            if (res.Success)
            {
                UIInventoryMenu.Singleton?.UpdateInventoryUI();
            }
            return res;
        }
    }
}
