using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the application (equip/unequip) of purchased items and runes to specific character slots.
    /// Uses arrays of pre-instantiated slots for items and runes and opens separate popups for each type.
    /// Also provides UnityEvents for Add/Remove operations.
    /// </summary>
    public class UIItemsApply : MonoBehaviour
    {
        [Header("Item Slots")]
        [Tooltip("Array of already-instantiated slots for regular items (e.g. weapon, armor).")]
        public ItemSlotEntry[] itemsSlots;

        [Header("Rune Slots")]
        [Tooltip("Array of already-instantiated slots for runes (category 'Runes').")]
        public ItemSlotEntry[] runesSlots;

        [Header("Popups")]
        [Tooltip("Popup GameObject that displays the list of available items (normal items).")]
        public GameObject itemPopup;

        [Tooltip("Popup GameObject that displays the list of available runes.")]
        public GameObject runePopup;

        [Tooltip("Optional UI placeholder to show if no rune is selected (not mandatory).")]
        public GameObject runeNotSelected;

        [Header("UI Containers (Instantiated Entries)")]
        [Tooltip("Container within the item popup that holds ItemApplyEntry instances for normal items.")]
        public Transform itemContainer;

        [Tooltip("Container within the rune popup that holds ItemApplyEntry instances for runes.")]
        public Transform runeContainer;

        [Header("Prefabs for Entry")]
        [Tooltip("Prefab for the item entries in the item popup.")]
        public ItemApplyEntry itemPrefab;

        [Tooltip("Prefab for the rune entries in the rune popup.")]
        public ItemApplyEntry runePrefab;

        [Header("Rune Category Filter")]
        [Tooltip("Category name used to identify runes (e.g., 'Runes').")]
        public string runeCategoryFilter = "Runes";

        [Header("Events")]
        [Tooltip("Invoked when an item or rune is successfully equipped (added).")]
        public UnityEvent OnAddItem;

        [Tooltip("Invoked when an item or rune is removed (unequipped).")]
        public UnityEvent OnRemoveItem;

        private CharacterData currentCharacter;

        /// <summary>
        /// Stores the equipped items or runes for quick reference.
        /// Key: slot name, Value: uniqueItemGuid of the equipped item or rune.
        /// </summary>
        private Dictionary<string, string> equippedItems = new Dictionary<string, string>();

        /// <summary>
        /// Holds a reference to the currently selected slot entry, so we can deselect it on popup close.
        /// </summary>
        private ItemSlotEntry currentSelectedSlotEntry = null;

        private void OnEnable()
        {
            // Called when this GameObject becomes enabled
        }

        /// <summary>
        /// Initializes both item slots and rune slots for the currently selected character.
        /// Activates or deactivates slots based on the character's available slot arrays.
        /// </summary>
        public void SetupSlotsForCurrentCharacter(int _currentSelectedCharacter)
        {
            int selectedCharacterId = _currentSelectedCharacter;
            CharacterData selectedCharacter = FindCharacterData(selectedCharacterId);

            if (selectedCharacter == null)
            {
                Debug.LogError($"No CharacterData found for ID {selectedCharacterId}.");
                return;
            }
            currentCharacter = selectedCharacter;
            equippedItems.Clear();

            // Setup normal item slots
            ConfigureItemSlots(selectedCharacter);
        }

        /// <summary>
        /// Configures the array of item slots (itemsSlots) based on the character's available itemSlots array.
        /// Each slot is activated or deactivated accordingly and the equipped icon is shown if any.
        /// </summary>
        private void ConfigureItemSlots(CharacterData charData)
        {
            if (charData.itemSlots == null) return;

            for (int i = 0; i < itemsSlots.Length; i++)
            {
                if (i < charData.itemSlots.Length)
                {
                    itemsSlots[i].gameObject.SetActive(true);
                    string slotName = charData.itemSlots[i];
                    itemsSlots[i].Setup(slotName, this, false); // 'false' = normal item
                    // Check which item is equipped in this slot
                    string uniqueGuid = InventorySave.GetEquippedItemForSlot(charData.characterId, slotName);
                    if (!string.IsNullOrEmpty(uniqueGuid))
                    {
                        equippedItems[slotName] = uniqueGuid;
                        // Update icon
                        var purchasedItem = PlayerSave.GetInventoryItems()
                            .Find(pi => pi.uniqueItemGuid == uniqueGuid);
                        if (purchasedItem != null)
                        {
                            var soItem = FindItemById(purchasedItem.itemId);
                            if (soItem != null)
                                itemsSlots[i].SetItemIcon(soItem.itemIcon, soItem.rarity);
                        }
                        else
                        {
                            itemsSlots[i].SetItemIcon(null);
                        }
                    }
                    else
                    {
                        itemsSlots[i].SetItemIcon(null);
                    }
                }
                else
                {
                    itemsSlots[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Configures the array of rune slots (runesSlots) based on the character's rune slots array.
        /// Each slot is activated or deactivated accordingly and the equipped icon is shown if any.
        /// </summary>
        public void ConfigureRuneSlots()
        {
            if (currentCharacter == null) return;
            if (currentCharacter.runeSlots == null) return;

            for (int i = 0; i < runesSlots.Length; i++)
            {
                if (i < currentCharacter.runeSlots.Length)
                {
                    runesSlots[i].gameObject.SetActive(true);
                    string slotName = currentCharacter.runeSlots[i];
                    runesSlots[i].Setup(slotName, this, true); // 'true' = is rune
                    // Check which rune is equipped
                    string uniqueGuid = InventorySave.GetEquippedItemForSlot(currentCharacter.characterId, slotName);
                    if (!string.IsNullOrEmpty(uniqueGuid))
                    {
                        equippedItems[slotName] = uniqueGuid;
                        var purchasedItem = PlayerSave.GetInventoryItems()
                            .Find(pi => pi.uniqueItemGuid == uniqueGuid);
                        if (purchasedItem != null)
                        {
                            var soRune = FindItemById(purchasedItem.itemId);
                            if (soRune != null)
                                runesSlots[i].SetItemIcon(soRune.itemIcon, soRune.rarity);
                        }
                        else
                        {
                            runesSlots[i].SetItemIcon(null);
                        }
                    }
                    else
                    {
                        runesSlots[i].SetItemIcon(null);
                    }
                }
                else
                {
                    runesSlots[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Finds the CharacterData for the given characterId.
        /// </summary>
        private CharacterData FindCharacterData(int charId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null) return null;
            foreach (var cd in GameInstance.Singleton.characterData)
            {
                if (cd.characterId == charId) return cd;
            }
            return null;
        }

        /// <summary>
        /// Finds the scriptable InventoryItem by itemId.
        /// </summary>
        private InventoryItem FindItemById(string itemId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.inventoryItems == null) return null;
            foreach (InventoryItem it in GameInstance.Singleton.inventoryItems)
            {
                if (it.itemId == itemId) return it;
            }
            return null;
        }

        /// <summary>
        /// Called by ItemSlotEntry when clicking the slot. Decides which popup (item or rune) to open.
        /// Also stores a reference to the clicked slot entry.
        /// </summary>
        /// <param name="slotName">The slot name to equip an item/rune in.</param>
        /// <param name="isRune">If true, open rune popup; otherwise item popup.</param>
        /// <param name="clickedSlotEntry">The slot entry that was clicked.</param>
        public void OpenSlotPopup(string slotName, bool isRune, ItemSlotEntry clickedSlotEntry)
        {
            currentSelectedSlotEntry = clickedSlotEntry;

            if (isRune)
            {
                if (runePopup != null) runePopup.SetActive(true);
                if (runeContainer != null)
                {
                    foreach (Transform child in runeContainer) Destroy(child.gameObject);
                }
                ShowRunesInPopup(slotName);
            }
            else
            {
                if (itemPopup != null) itemPopup.SetActive(true);
                if (itemContainer != null)
                {
                    foreach (Transform child in itemContainer) Destroy(child.gameObject);
                }
                ShowItemsInPopup(slotName);
            }
        }

        /// <summary>
        /// Populates the item popup with valid purchased items that match the given slotName (excluding runes).
        /// </summary>
        private void ShowItemsInPopup(string slotName)
        {
            var purchasedItems = PlayerSave.GetInventoryItems();
            foreach (var pi in purchasedItems)
            {
                var soItem = FindItemById(pi.itemId);
                if (soItem == null) continue;
                if (soItem.category == runeCategoryFilter) continue; // skip runes
                if (soItem.slot != slotName) continue;

                bool equipElsewhere = IsEquippedByAnotherCharacter(pi.uniqueItemGuid, currentCharacter.characterId, slotName);
                if (!equipElsewhere)
                {
                    var entry = Instantiate(itemPrefab, itemContainer);
                    entry.Setup(soItem, pi.uniqueItemGuid, slotName, this, false);
                    string eqHere = InventorySave.GetEquippedItemForSlot(currentCharacter.characterId, slotName);
                    entry.ShowAsEquipped(eqHere == pi.uniqueItemGuid);
                }
            }
        }

        /// <summary>
        /// Populates the rune popup with valid purchased runes that match the given slotName (matching runeCategoryFilter).
        /// </summary>
        private void ShowRunesInPopup(string slotName)
        {
            if (runeNotSelected != null) runeNotSelected.gameObject.SetActive(false);

            var purchasedItems = PlayerSave.GetInventoryItems();
            foreach (var pi in purchasedItems)
            {
                var soItem = FindItemById(pi.itemId);
                if (soItem == null) continue;
                if (soItem.category != runeCategoryFilter) continue;
                if (soItem.slot != slotName) continue;

                bool equipElsewhere = IsEquippedByAnotherCharacter(pi.uniqueItemGuid, currentCharacter.characterId, slotName);
                if (!equipElsewhere)
                {
                    var entry = Instantiate(runePrefab, runeContainer);
                    entry.Setup(soItem, pi.uniqueItemGuid, slotName, this, true);
                    string eqHere = InventorySave.GetEquippedItemForSlot(currentCharacter.characterId, slotName);
                    entry.ShowAsEquipped(eqHere == pi.uniqueItemGuid);
                }
            }
        }

        /// <summary>
        /// Checks if an item/rune is equipped by a different character in the same slot.
        /// </summary>
        private bool IsEquippedByAnotherCharacter(string uniqueItemGuid, int currentCharId, string slotName)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null) return false;
            foreach (var cd in GameInstance.Singleton.characterData)
            {
                if (cd.characterId == currentCharId) continue;
                string eq = InventorySave.GetEquippedItemForSlot(cd.characterId, slotName);
                if (eq == uniqueItemGuid) return true;
            }
            return false;
        }

        /// <summary>
        /// Equips or unequips an item/rune in the specified slot. If already equipped, unequips. Otherwise, replaces any existing item.
        /// Invokes OnAddItem or OnRemoveItem events accordingly.
        /// </summary>
        /// <param name="uniqueItemGuid">The purchased item/rune GUID.</param>
        /// <param name="slotName">The slot name (e.g., "WeaponSlot" or "DefenseRuneSlot").</param>
        /// <param name="isRune">If true, indicates a rune slot.</param>
        public async void ApplyItemToSlot(string uniqueItemGuid, string slotName, bool isRune)
        {;

            RequestResult r;
            if (equippedItems.ContainsKey(slotName) && equippedItems[slotName] == uniqueItemGuid)
            {
                r = await InventorySave.SetEquippedItemForSlotAsync(
                        currentCharacter.characterId, slotName, "");
                if (r.Success) equippedItems.Remove(slotName);
                OnRemoveItem?.Invoke();
            }
            else
            {
                if (equippedItems.ContainsKey(slotName))
                    await InventorySave.SetEquippedItemForSlotAsync(
                        currentCharacter.characterId, slotName, "");

                r = await InventorySave.SetEquippedItemForSlotAsync(
                        currentCharacter.characterId, slotName, uniqueItemGuid);

                if (r.Success) equippedItems[slotName] = uniqueItemGuid;
                OnAddItem?.Invoke();
            }

            if (!r.Success) Debug.LogWarning($"Equip failed: {r.Reason}");

            ClosePopup(isRune);
            RefreshSlotIcon(slotName, isRune);
            UICharacterMenu.Singleton.UpdateDetailPanel(currentCharacter);
        }

        /// <summary>
        /// Closes the appropriate popup (item or rune) after the operation completes,
        /// and deselects the current slot.
        /// </summary>
        public void ClosePopup(bool isRune)
        {
            if (currentSelectedSlotEntry != null)
            {
                currentSelectedSlotEntry.DeselectSlot();
                currentSelectedSlotEntry = null;
            }

            if (runeNotSelected != null) runeNotSelected.gameObject.SetActive(true);

            if (isRune && runePopup != null) runePopup.SetActive(false);
            if (!isRune && itemPopup != null) itemPopup.SetActive(false);
        }

        /// <summary>
        /// Refreshes the slot icon for the given slot after equipping or unequipping.
        /// </summary>
        private void RefreshSlotIcon(string slotName, bool isRune)
        {
            string guidEquipped = equippedItems.ContainsKey(slotName) ? equippedItems[slotName] : "";
            InventoryItem soItem = null;
            if (!string.IsNullOrEmpty(guidEquipped))
            {
                var purchased = PlayerSave.GetInventoryItems()
                    .Find(pi => pi.uniqueItemGuid == guidEquipped);
                if (purchased != null) soItem = FindItemById(purchased.itemId);
            }

            ItemSlotEntry[] targetSlots = isRune ? runesSlots : itemsSlots;
            for (int i = 0; i < targetSlots.Length; i++)
            {
                if (!targetSlots[i].gameObject.activeSelf) continue;
                if (targetSlots[i].GetSlotName() == slotName)
                {
                    if (soItem == null) targetSlots[i].SetItemIcon(null);
                    else targetSlots[i].SetItemIcon(soItem.itemIcon, soItem.rarity);
                    break;
                }
            }
        }
    }
}
