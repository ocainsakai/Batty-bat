using UnityEngine;

namespace Core.Systems.CollectionFramework
{
    /// <summary>
    /// Represents a single slot in an inventory.
    /// Can hold one item type with quantity (stacking).
    /// </summary>
    [System.Serializable]
    public class InventorySlot : IInventoryItem
    {
        [SerializeField] private ItemDefinition _definition;
        [SerializeField] private int _quantity;

        public ItemDefinition Definition
        {
            get => _definition;
            private set => _definition = value;
        }

        public int Quantity
        {
            get => _quantity;
            set => _quantity = Mathf.Max(0, value);
        }

        public bool IsEmpty => Definition == null || Quantity <= 0;
        public bool IsStackable => Definition != null && Definition.MaxStackSize > 1;
        public int MaxStackSize => Definition != null ? Definition.MaxStackSize : 1;

        public InventorySlot()
        {
            Clear();
        }

        public InventorySlot(ItemDefinition definition, int quantity)
        {
            SetItem(definition, quantity);
        }

        /// <summary>
        /// Set the item in this slot
        /// </summary>
        public void SetItem(ItemDefinition item, int quantity)
        {
            Definition = item;
            Quantity = Mathf.Clamp(quantity, 0, item != null ? item.MaxStackSize : 0);
        }

        /// <summary>
        /// Check if more items can be added to this slot
        /// </summary>
        public bool CanAddMore(int amount)
        {
            if (IsEmpty) return true;
            return Quantity + amount <= MaxStackSize;
        }

        /// <summary>
        /// Add items to this slot
        /// </summary>
        public int Add(int amount)
        {
            if (IsEmpty) return 0;

            int maxCanAdd = MaxStackSize - Quantity;
            int actualAdd = Mathf.Min(amount, maxCanAdd);
            Quantity += actualAdd;
            return actualAdd;
        }

        /// <summary>
        /// Remove items from this slot
        /// </summary>
        public int Remove(int amount)
        {
            int actualRemove = Mathf.Min(amount, Quantity);
            Quantity -= actualRemove;

            if (Quantity <= 0)
            {
                Clear();
            }

            return actualRemove;
        }

        /// <summary>
        /// Clear this slot
        /// </summary>
        public void Clear()
        {
            Definition = null;
            Quantity = 0;
        }

        /// <summary>
        /// Clone this slot
        /// </summary>
        public InventorySlot Clone()
        {
            return new InventorySlot(Definition, Quantity);
        }
    }
}
