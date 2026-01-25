using System.Collections.Generic;
using UnityEngine;
using Core.EventSystem;
using Core.Systems.CollectableSystem;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// UI component for displaying inventory grid.
    /// Automatically syncs with CollectableInventory via events.
    /// </summary>
    public class InventoryGridUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private CollectableInventory targetInventory;

        [Header("Settings")]
        [SerializeField] private int maxSlots = 20;
        [SerializeField] private bool autoCreateSlots = true;
        [SerializeField] private bool subscribeToEvents = true;

        private List<InventorySlotUI> _slots = new List<InventorySlotUI>();
        private Dictionary<string, InventorySlotUI> _itemToSlot = new Dictionary<string, InventorySlotUI>();
        private InventorySlotUI _selectedSlot;

        public InventorySlotUI SelectedSlot => _selectedSlot;
        public System.Action<InventorySlotUI> OnSlotSelected;
        public System.Action<string, int> OnItemClicked;

        private void Awake()
        {
            if (slotsContainer == null)
            {
                slotsContainer = transform;
            }

            // Get existing slots or create new ones
            if (autoCreateSlots)
            {
                InitializeSlots();
            }
            else
            {
                // Get existing slots from children
                _slots.AddRange(slotsContainer.GetComponentsInChildren<InventorySlotUI>());
            }
        }

        private void OnEnable()
        {
            if (subscribeToEvents)
            {
                EventBus.Subscribe<CollectableAddedEvent>(OnCollectableAdded);
                EventBus.Subscribe<CollectableRemovedEvent>(OnCollectableRemoved);
            }

            // Refresh from inventory if available
            if (targetInventory != null)
            {
                RefreshFromInventory();
            }
        }

        private void OnDisable()
        {
            if (subscribeToEvents)
            {
                EventBus.Unsubscribe<CollectableAddedEvent>(OnCollectableAdded);
                EventBus.Unsubscribe<CollectableRemovedEvent>(OnCollectableRemoved);
            }
        }

        private void InitializeSlots()
        {
            if (slotPrefab == null)
            {
                Debug.LogError("[InventoryGridUI] Slot prefab is not assigned!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            _slots.Clear();

            // Create slots
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = Instantiate(slotPrefab, slotsContainer);
                slot.name = $"Slot_{i}";
                slot.OnSlotClicked = HandleSlotClicked;
                slot.SetEmpty();
                _slots.Add(slot);
            }
        }

        /// <summary>
        /// Refresh UI from inventory data
        /// </summary>
        public void RefreshFromInventory()
        {
            if (targetInventory == null) return;

            // Clear all slots first
            ClearAllSlots();

            // Get inventory data
            var items = targetInventory.GetAll();
            int slotIndex = 0;

            foreach (var kvp in items)
            {
                if (slotIndex >= _slots.Count) break;

                var definition = targetInventory.GetDefinition(kvp.Key);
                if (definition != null)
                {
                    SetSlotItem(slotIndex, definition, kvp.Value);
                    slotIndex++;
                }
            }
        }

        /// <summary>
        /// Set item in a specific slot
        /// </summary>
        public void SetSlotItem(int slotIndex, CollectibleDefinition definition, int count)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return;

            var slot = _slots[slotIndex];
            slot.SetItem(
                definition.CollectibleID,
                definition.CollectibleSprite,
                count,
                definition.CollectibleName
            );

            _itemToSlot[definition.CollectibleID] = slot;
        }

        /// <summary>
        /// Update item in inventory (find or create slot)
        /// </summary>
        public void UpdateItem(CollectibleDefinition definition, int count)
        {
            if (definition == null) return;

            // Find existing slot for this item
            if (_itemToSlot.TryGetValue(definition.CollectibleID, out var existingSlot))
            {
                if (count > 0)
                {
                    existingSlot.UpdateCount(count);
                    existingSlot.PlayAddAnimation();
                }
                else
                {
                    existingSlot.PlayRemoveAnimation();
                    existingSlot.SetEmpty();
                    _itemToSlot.Remove(definition.CollectibleID);
                }
            }
            else if (count > 0)
            {
                // Find empty slot
                var emptySlot = FindEmptySlot();
                if (emptySlot != null)
                {
                    emptySlot.SetItem(
                        definition.CollectibleID,
                        definition.CollectibleSprite,
                        count,
                        definition.CollectibleName
                    );
                    emptySlot.PlayAddAnimation();
                    _itemToSlot[definition.CollectibleID] = emptySlot;
                }
            }
        }

        /// <summary>
        /// Remove item from UI
        /// </summary>
        public void RemoveItem(string itemId)
        {
            if (_itemToSlot.TryGetValue(itemId, out var slot))
            {
                slot.PlayRemoveAnimation();
                slot.SetEmpty();
                _itemToSlot.Remove(itemId);
            }
        }

        /// <summary>
        /// Find first empty slot
        /// </summary>
        public InventorySlotUI FindEmptySlot()
        {
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                    return slot;
            }
            return null;
        }

        /// <summary>
        /// Clear all slots
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var slot in _slots)
            {
                slot.SetEmpty();
            }
            _itemToSlot.Clear();
            _selectedSlot = null;
        }

        /// <summary>
        /// Select a slot
        /// </summary>
        public void SelectSlot(InventorySlotUI slot)
        {
            // Deselect previous
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(false);
            }

            // Select new
            _selectedSlot = slot;
            if (_selectedSlot != null)
            {
                _selectedSlot.SetSelected(true);
            }

            OnSlotSelected?.Invoke(_selectedSlot);
        }

        private void HandleSlotClicked(InventorySlotUI slot)
        {
            SelectSlot(slot);
            
            if (!slot.IsEmpty)
            {
                OnItemClicked?.Invoke(slot.ItemId, slot.Count);
            }
        }

        #region Event Handlers

        private void OnCollectableAdded(CollectableAddedEvent evt)
        {
            UpdateItem(evt.Definition, evt.NewCount);
        }

        private void OnCollectableRemoved(CollectableRemovedEvent evt)
        {
            if (evt.NewCount <= 0)
            {
                RemoveItem(evt.Definition.CollectibleID);
            }
            else
            {
                UpdateItem(evt.Definition, evt.NewCount);
            }
        }

        #endregion

        /// <summary>
        /// Get slot count
        /// </summary>
        public int SlotCount => _slots.Count;

        /// <summary>
        /// Get used slot count
        /// </summary>
        public int UsedSlotCount => _itemToSlot.Count;
    }
}
