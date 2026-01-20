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
        [SerializeField] private CollectableInventory _inventory;

        protected virtual void Awake()
        {
            Debug.Log("[WormCollector] Awake called");
            if (_growth == null)
            _growth = GetComponent<WormGrowth>();
            if (_inventory == null)
            _inventory = GetComponent<CollectableInventory>();
            
            Debug.Log($"[WormCollector] WormGrowth found: {_growth != null}");
            Debug.Log($"[WormCollector] CollectableInventory found: {_inventory != null}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[WormCollector] OnTriggerEnter2D with: {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
            
            // Try to get collectable
            var collectable = other.GetComponent<ICollectable>();
            if (collectable == null)
            {
                // Also try Collectable directly
                var collectableComponent = other.GetComponent<Collectable>();
                Debug.Log($"[WormCollector] ICollectable found: {collectable != null}, Collectable component: {collectableComponent != null}");
                
                if (collectableComponent != null)
                {
                    collectable = collectableComponent;
                }
            }
            
            if (collectable != null)
            {
                Debug.Log($"[WormCollector] Found collectable: {collectable.Definition?.CollectibleName ?? "null definition"}");
                
                if (CanCollect(collectable))
                {
                    Debug.Log("[WormCollector] CanCollect = true, calling Collect");
                    CollectResource(collectable);
                }
                else
                {
                    Debug.Log("[WormCollector] CanCollect = false");
                }
            }
            else
            {
                Debug.Log("[WormCollector] No ICollectable found on object");
            }
        }

        /// <summary>
        /// Custom collect method for Worm with growth logic
        /// </summary>
        private void CollectResource(ICollectable collectable)
        {
            if (collectable == null || collectable.Definition == null)
            {
                Debug.LogWarning("[WormCollector] CollectResource: collectable or definition is null");
                return;
            }

            var definition = collectable.Definition;
            Debug.Log($"[WormCollector] Collecting: {definition.CollectibleName}");

            // Add to inventory
            if (_inventory != null)
            {
                _inventory.Add(definition);
                Debug.Log($"[WormCollector] Added to inventory");
            }
            else
            {
                Debug.LogWarning("[WormCollector] No inventory component found!");
            }

            // Worm-specific: Add growth based on resource
            if (_growth != null && definition is ResourceDefinition resourceDef)
            {
                _growth.AddGrowth(resourceDef.GrowthValue);
                Debug.Log($"[WormCollector] Added growth: {resourceDef.GrowthValue}");
            }
            else
            {
                Debug.Log($"[WormCollector] Growth not added. _growth null: {_growth == null}, is ResourceDefinition: {definition is ResourceDefinition}");
            }

            // Notify collectable
            collectable.OnCollected(gameObject);

            // Publish Worm-specific event
            if (definition is ResourceDefinition resDef)
            {
                EventBus.Publish(new ResourceCollectedEvent(resDef, transform.position));
                Debug.Log("[WormCollector] Published ResourceCollectedEvent");
            }

            // Publish Core event
            EventBus.Publish(new CollectableCollectedEvent(definition, transform.position, gameObject));

            // Despawn and schedule respawn using Core's CollectableSpawner
            if (collectable is Collectable collectableComponent)
            {
                Debug.Log($"[WormCollector] CollectableSpawner.HasInstance: {CollectableSpawner.HasInstance}");
                if (CollectableSpawner.HasInstance)
                {
                    CollectableSpawner.Instance.Despawn(collectableComponent);
                    CollectableSpawner.Instance.ScheduleRespawn(definition);
                    Debug.Log("[WormCollector] Despawned and scheduled respawn");
                }
                else
                {
                    // Fallback: just deactivate
                    collectableComponent.gameObject.SetActive(false);
                    Debug.Log("[WormCollector] No spawner, deactivated object");
                }
            }
        }
    }
}
