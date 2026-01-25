using UnityEngine;
using Core.EventSystem;
using Core.Systems.CollectableSystem;
using Games.Worm.Events;
using Games.Worm.Data;

namespace Games.Worm.Player
{
    /// <summary>
    /// Handles resource collection when worm collides with resources.
    /// Extends CollectorComponent to add Worm-specific growth logic.
    /// </summary>
    public class WormCollector : CollectorComponent
    {
        [SerializeField] private WormGrowth _growth;

        protected override void Awake()
        {
            if (_growth == null)
            _growth = GetComponent<WormGrowth>();
            if (_inventory == null)
            _inventory = GetComponent<CollectableInventory>();
            
            Debug.Log($"[WormCollector] WormGrowth found: {_growth != null}");
            Debug.Log($"[WormCollector] CollectableInventory found: {_inventory != null}");
        }

    }
}
