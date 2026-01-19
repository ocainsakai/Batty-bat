using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.EventSystem;

namespace Core.Systems.CollectionFramework
{
    /// <summary>
    /// Core inventory system for managing items.
    /// Supports stacking, capacity limits, and item management.
    /// </summary>
    public class InventorySystem : MonoBehaviour, IInventory
    {
        [Header("Settings")]
        [SerializeField] private int capacity = 50;
        [SerializeField] private bool autoSort = false;
        [SerializeField] private bool allowOverflow = false;

        private List<InventorySlot> _slots;

        public int Capacity => capacity;
        public int UsedSlots => _slots?.Count(s => !s.IsEmpty) ?? 0;
        public bool IsFull => UsedSlots >= Capacity && !allowOverflow;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _slots = new List<InventorySlot>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                _slots.Add(new InventorySlot());
            }
        }

        public bool Add(ItemDefinition item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
            {
                Debug.LogWarning("[InventorySystem] Cannot add null item or invalid quantity");
                return false;
            }

            int remaining = quantity;

            // Try to stack first
            if (item.MaxStackSize > 1)
            {
                foreach (var slot in _slots)
                {
                    if (!slot.IsEmpty && slot.Definition == item && slot.CanAddMore(remaining))
                    {
                        int added = slot.Add(remaining);
                        remaining -= added;

                        if (remaining <= 0)
                        {
                            OnInventoryChanged(item, quantity);
                            return true;
                        }
                    }
                }
            }

            // Create new slots for remaining items
            while (remaining > 0)
            {
                var emptySlot = FindEmptySlot();
                if (emptySlot == null)
                {
                    if (!allowOverflow)
                    {
                        Debug.LogWarning($"[InventorySystem] Inventory full! Could not add {remaining}x {item.ItemName}");
                        return false;
                    }
                    else
                    {
                        // Create overflow slot
                        emptySlot = new InventorySlot();
                        _slots.Add(emptySlot);
                    }
                }

                int toAdd = Mathf.Min(remaining, item.MaxStackSize);
                emptySlot.SetItem(item, toAdd);
                remaining -= toAdd;
            }

            OnInventoryChanged(item, quantity);
            return true;
        }

        public bool Remove(ItemDefinition item, int quantity = 1)
        {
            if (item == null || quantity <= 0)
                return false;

            // Check if we have enough
            if (!Has(item, quantity))
                return false;

            int remaining = quantity;

            // Remove from slots
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Definition == item)
                {
                    int removed = slot.Remove(remaining);
                    remaining -= removed;

                    if (remaining <= 0)
                        break;
                }
            }

            OnInventoryChanged(item, -quantity);
            return true;
        }

        public bool Has(ItemDefinition item, int quantity = 1)
        {
            return GetCount(item) >= quantity;
        }

        public int GetCount(ItemDefinition item)
        {
            if (item == null) return 0;

            return _slots
                .Where(s => !s.IsEmpty && s.Definition == item)
                .Sum(s => s.Quantity);
        }

        /// <summary>
        /// Get all items in inventory
        /// </summary>
        public List<InventorySlot> GetAllSlots()
        {
            return _slots.Where(s => !s.IsEmpty).ToList();
        }

        /// <summary>
        /// Get items by category
        /// </summary>
        public List<InventorySlot> GetByCategory(ItemCategory category)
        {
            return _slots
                .Where(s => !s.IsEmpty && s.Definition.Category == category)
                .ToList();
        }

        /// <summary>
        /// Get items by type
        /// </summary>
        public List<InventorySlot> GetByType(ItemType type)
        {
            return _slots
                .Where(s => !s.IsEmpty && s.Definition.Type == type)
                .ToList();
        }

        /// <summary>
        /// Clear entire inventory
        /// </summary>
        public void Clear()
        {
            foreach (var slot in _slots)
            {
                slot.Clear();
            }
            EventBus.Publish(new InventoryClearedEvent());
        }

        /// <summary>
        /// Sort inventory (if auto-sort enabled)
        /// </summary>
        public void Sort()
        {
            if (!autoSort) return;

            // Sort by: Type > Rarity > Name
            _slots = _slots
                .OrderBy(s => s.IsEmpty)
                .ThenBy(s => s.Definition?.Type)
                .ThenByDescending(s => s.Definition?.Rarity)
                .ThenBy(s => s.Definition?.ItemName)
                .ToList();
        }

        private InventorySlot FindEmptySlot()
        {
            return _slots.FirstOrDefault(s => s.IsEmpty);
        }

        private void OnInventoryChanged(ItemDefinition item, int quantityChange)
        {
            EventBus.Publish(new InventoryChangedEvent(item, GetCount(item), quantityChange));

            if (autoSort)
            {
                Sort();
            }
        }
    }

    #region Events

    public class InventoryChangedEvent : IEvent
    {
        public ItemDefinition Item { get; set; }
        public int NewCount { get; set; }
        public int QuantityChange { get; set; }

        public InventoryChangedEvent(ItemDefinition item, int newCount, int quantityChange)
        {
            Item = item;
            NewCount = newCount;
            QuantityChange = quantityChange;
        }
    }

    public class InventoryClearedEvent : IEvent { }

    #endregion
}
