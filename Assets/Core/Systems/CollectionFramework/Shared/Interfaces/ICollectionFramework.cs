using UnityEngine;

namespace Core.Systems.CollectionFramework
{
    /// <summary>
    /// Interface for objects that can be collected.
    /// </summary>
    public interface ICollectible
    {
        /// <summary>
        /// The item definition for this collectible
        /// </summary>
        ItemDefinition Definition { get; }

        /// <summary>
        /// Called when this collectible is collected
        /// </summary>
        void OnCollected(GameObject collector);

        /// <summary>
        /// Check if this collectible can be collected by the given collector
        /// </summary>
        bool CanBeCollected(GameObject collector);

        /// <summary>
        /// Get the world position of this collectible
        /// </summary>
        Vector3 Position { get; }
    }

    /// <summary>
    /// Interface for objects that can collect collectibles.
    /// </summary>
    public interface ICollector
    {
        /// <summary>
        /// Collect a collectible
        /// </summary>
        void Collect(ICollectible collectible);

        /// <summary>
        /// Check if this collector can collect the given collectible
        /// </summary>
        bool CanCollect(ICollectible collectible);

        /// <summary>
        /// Get the GameObject of this collector
        /// </summary>
        GameObject GameObject { get; }
    }

    /// <summary>
    /// Interface for items that can be stored in inventory.
    /// </summary>
    public interface IInventoryItem
    {
        /// <summary>
        /// The item definition
        /// </summary>
        ItemDefinition Definition { get; }

        /// <summary>
        /// Current quantity/stack size
        /// </summary>
        int Quantity { get; set; }

        /// <summary>
        /// Can this item be stacked?
        /// </summary>
        bool IsStackable { get; }

        /// <summary>
        /// Maximum stack size for this item
        /// </summary>
        int MaxStackSize { get; }
    }

    /// <summary>
    /// Interface for inventory systems.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// Add an item to inventory
        /// </summary>
        bool Add(ItemDefinition item, int quantity = 1);

        /// <summary>
        /// Remove an item from inventory
        /// </summary>
        bool Remove(ItemDefinition item, int quantity = 1);

        /// <summary>
        /// Check if inventory has an item
        /// </summary>
        bool Has(ItemDefinition item, int quantity = 1);

        /// <summary>
        /// Get count of a specific item
        /// </summary>
        int GetCount(ItemDefinition item);

        /// <summary>
        /// Total capacity of inventory
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Number of used slots
        /// </summary>
        int UsedSlots { get; }

        /// <summary>
        /// Is inventory full?
        /// </summary>
        bool IsFull { get; }
    }

    /// <summary>
    /// Interface for gallery/collection book items.
    /// </summary>
    public interface IGalleryItem
    {
        /// <summary>
        /// The item definition
        /// </summary>
        ItemDefinition Definition { get; }

        /// <summary>
        /// Is this item unlocked in the gallery?
        /// </summary>
        bool IsUnlocked { get; }

        /// <summary>
        /// When was this item first unlocked?
        /// </summary>
        System.DateTime UnlockedDate { get; }

        /// <summary>
        /// How many times has this item been collected?
        /// </summary>
        int TimesCollected { get; }

        /// <summary>
        /// Unlock this item
        /// </summary>
        void Unlock();

        /// <summary>
        /// Increment collection count
        /// </summary>
        void IncrementCollected();
    }

    /// <summary>
    /// Interface for gallery/collection systems.
    /// </summary>
    public interface IGallery
    {
        /// <summary>
        /// Register an item in the gallery
        /// </summary>
        void RegisterItem(ItemDefinition item);

        /// <summary>
        /// Unlock an item in the gallery
        /// </summary>
        void UnlockItem(ItemDefinition item);

        /// <summary>
        /// Check if an item is unlocked
        /// </summary>
        bool IsUnlocked(ItemDefinition item);

        /// <summary>
        /// Get completion percentage (0-100)
        /// </summary>
        float GetCompletionPercentage();

        /// <summary>
        /// Get completion percentage for a specific category
        /// </summary>
        float GetCategoryCompletion(ItemCategory category);

        /// <summary>
        /// Get completion percentage for a specific rarity
        /// </summary>
        float GetRarityCompletion(ItemRarity rarity);

        /// <summary>
        /// Get total number of items in gallery
        /// </summary>
        int TotalItems { get; }

        /// <summary>
        /// Get number of unlocked items
        /// </summary>
        int UnlockedItems { get; }
    }
}
