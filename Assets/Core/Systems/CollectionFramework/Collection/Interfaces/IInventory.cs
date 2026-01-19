using System.Collections.Generic;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// Interface for inventory systems that track collected items.
    /// </summary>
    public interface IInventory
    {
        /// <summary>
        /// Add a collectable to the inventory
        /// </summary>
        void Add(CollectibleDefinition definition, int amount = 1);

        /// <summary>
        /// Remove/consume collectables from inventory
        /// </summary>
        bool Remove(string collectibleID, int amount = 1);

        /// <summary>
        /// Get the count of a specific collectable type
        /// </summary>
        int GetCount(string collectibleID);

        /// <summary>
        /// Check if inventory has a specific amount of a collectable
        /// </summary>
        bool Has(string collectibleID, int amount = 1);

        /// <summary>
        /// Get all collectables in inventory
        /// </summary>
        Dictionary<string, int> GetAll();

        /// <summary>
        /// Clear all collectables from inventory
        /// </summary>
        void Clear();

        /// <summary>
        /// Total number of all collectables
        /// </summary>
        int TotalCount { get; }
    }
}
