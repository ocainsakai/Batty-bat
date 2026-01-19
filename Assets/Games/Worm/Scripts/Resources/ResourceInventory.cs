using System.Collections.Generic;
using UnityEngine;
using Core.EventSystem;
using Games.Worm.Events;
using Games.Worm.Data;

namespace Games.Worm.Resources
{
    /// <summary>
    /// Tracks all resources collected by the worm.
    /// Provides inventory management and statistics.
    /// </summary>
    public class ResourceInventory : MonoBehaviour
    {
        private Dictionary<string, int> _resources = new Dictionary<string, int>();
        private Dictionary<string, ResourceDefinition> _definitions = new Dictionary<string, ResourceDefinition>();

        public int TotalCollected { get; private set; }

        /// <summary>
        /// Add a resource to the inventory
        /// </summary>
        public void AddResource(ResourceDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogWarning("[ResourceInventory] Trying to add null resource definition");
                return;
            }

            string id = definition.ResourceID;

            // Initialize if first time
            if (!_resources.ContainsKey(id))
            {
                _resources[id] = 0;
                _definitions[id] = definition;
            }

            // Increment count
            _resources[id]++;
            TotalCollected++;

            // Publish event
            EventBus.Publish(new InventoryChangedEvent(
                definition,
                _resources[id],
                TotalCollected
            ));

            Debug.Log($"[ResourceInventory] Collected {definition.ResourceName}. Total: {_resources[id]}");
        }

        /// <summary>
        /// Get count of a specific resource type
        /// </summary>
        public int GetResourceCount(string resourceID)
        {
            return _resources.ContainsKey(resourceID) ? _resources[resourceID] : 0;
        }

        /// <summary>
        /// Get count of a specific resource type by definition
        /// </summary>
        public int GetResourceCount(ResourceDefinition definition)
        {
            return definition != null ? GetResourceCount(definition.ResourceID) : 0;
        }

        /// <summary>
        /// Get all resources in inventory
        /// </summary>
        public Dictionary<string, int> GetAllResources()
        {
            return new Dictionary<string, int>(_resources);
        }

        /// <summary>
        /// Get resource definition by ID
        /// </summary>
        public ResourceDefinition GetDefinition(string resourceID)
        {
            return _definitions.ContainsKey(resourceID) ? _definitions[resourceID] : null;
        }

        /// <summary>
        /// Consume resources (for crafting - future feature)
        /// </summary>
        public bool ConsumeResources(string resourceID, int amount)
        {
            if (!_resources.ContainsKey(resourceID) || _resources[resourceID] < amount)
            {
                return false;
            }

            _resources[resourceID] -= amount;
            TotalCollected -= amount;

            // Publish event
            var definition = GetDefinition(resourceID);
            if (definition != null)
            {
                EventBus.Publish(new InventoryChangedEvent(
                    definition,
                    _resources[resourceID],
                    TotalCollected
                ));
            }

            return true;
        }

        /// <summary>
        /// Clear all resources (for reset/new game)
        /// </summary>
        public void Clear()
        {
            _resources.Clear();
            _definitions.Clear();
            TotalCollected = 0;
        }

        /// <summary>
        /// Get inventory data for saving
        /// </summary>
        public InventoryData GetInventoryData()
        {
            return new InventoryData
            {
                Resources = new Dictionary<string, int>(_resources),
                TotalCollected = TotalCollected
            };
        }

        /// <summary>
        /// Load inventory data from save
        /// </summary>
        public void LoadInventoryData(InventoryData data)
        {
            if (data == null) return;

            _resources = new Dictionary<string, int>(data.Resources);
            TotalCollected = data.TotalCollected;
        }
    }

    [System.Serializable]
    public class InventoryData
    {
        public Dictionary<string, int> Resources;
        public int TotalCollected;
    }
}
