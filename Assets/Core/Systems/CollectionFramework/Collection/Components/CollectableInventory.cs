using System.Collections.Generic;
using UnityEngine;
using Core.EventSystem;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// Inventory system for tracking collected items.
    /// Implements IInventory interface.
    /// </summary>
    public class CollectableInventory : MonoBehaviour, IInventory
    {
        private Dictionary<string, int> _collectables = new Dictionary<string, int>();
        private Dictionary<string, CollectibleDefinition> _definitions = new Dictionary<string, CollectibleDefinition>();

        public int TotalCount { get; private set; }

        public void Add(CollectibleDefinition definition, int amount = 1)
        {
            if (definition == null)
            {
                Debug.LogWarning("[CollectableInventory] Trying to add null definition");
                return;
            }

            string id = definition.CollectibleID;

            // Initialize if first time
            if (!_collectables.ContainsKey(id))
            {
                _collectables[id] = 0;
                _definitions[id] = definition;
            }

            // Add amount
            _collectables[id] += amount;
            TotalCount += amount;

            // Publish event
            EventBus.Publish(new CollectableAddedEvent(definition, _collectables[id], amount));

            Debug.Log($"[CollectableInventory] Added {amount}x {definition.CollectibleName}. Total: {_collectables[id]}");
        }

        public bool Remove(string collectibleID, int amount = 1)
        {
            if (!_collectables.ContainsKey(collectibleID) || _collectables[collectibleID] < amount)
            {
                return false;
            }

            _collectables[collectibleID] -= amount;
            TotalCount -= amount;

            // Publish event
            var definition = GetDefinition(collectibleID);
            if (definition != null)
            {
                EventBus.Publish(new CollectableRemovedEvent(definition, _collectables[collectibleID], amount));
            }

            return true;
        }

        public int GetCount(string collectibleID)
        {
            return _collectables.ContainsKey(collectibleID) ? _collectables[collectibleID] : 0;
        }

        public bool Has(string collectibleID, int amount = 1)
        {
            return GetCount(collectibleID) >= amount;
        }

        public Dictionary<string, int> GetAll()
        {
            return new Dictionary<string, int>(_collectables);
        }

        public void Clear()
        {
            _collectables.Clear();
            _definitions.Clear();
            TotalCount = 0;
        }

        /// <summary>
        /// Get definition for a collectable ID
        /// </summary>
        public CollectibleDefinition GetDefinition(string collectibleID)
        {
            return _definitions.ContainsKey(collectibleID) ? _definitions[collectibleID] : null;
        }
    }

    #region Events

    public class CollectableAddedEvent : IEvent
    {
        public CollectibleDefinition Definition { get; set; }
        public int NewCount { get; set; }
        public int AmountAdded { get; set; }

        public CollectableAddedEvent(CollectibleDefinition definition, int newCount, int amountAdded)
        {
            Definition = definition;
            NewCount = newCount;
            AmountAdded = amountAdded;
        }
    }

    public class CollectableRemovedEvent : IEvent
    {
        public CollectibleDefinition Definition { get; set; }
        public int NewCount { get; set; }
        public int AmountRemoved { get; set; }

        public CollectableRemovedEvent(CollectibleDefinition definition, int newCount, int amountRemoved)
        {
            Definition = definition;
            NewCount = newCount;
            AmountRemoved = amountRemoved;
        }
    }

    #endregion
}
