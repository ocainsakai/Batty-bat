using UnityEngine;
using Core.EventSystem;
using Games.Worm.Data;

namespace Games.Worm.Events
{
    /// <summary>
    /// Worm-specific resource events.
    /// Note: Core provides generic events (CollectableCollectedEvent, CollectableAddedEvent)
    /// These Worm-specific events are kept for backward compatibility and Worm-specific needs.
    /// </summary>

    /// <summary>
    /// Fired when a resource is collected by the worm.
    /// Use this for Worm-specific logic (growth animations, score, etc.)
    /// </summary>
    public class ResourceCollectedEvent : IEvent
    {
        public ResourceDefinition Resource { get; set; }
        public Vector3 Position { get; set; }

        public ResourceCollectedEvent(ResourceDefinition resource, Vector3 position)
        {
            Resource = resource;
            Position = position;
        }
    }

    /// <summary>
    /// Fired when inventory contents change.
    /// Consider using Core's CollectableAddedEvent instead for new code.
    /// </summary>
    public class InventoryChangedEvent : IEvent
    {
        public ResourceDefinition Resource { get; set; }
        public int NewCount { get; set; }
        public int TotalCollected { get; set; }

        public InventoryChangedEvent(ResourceDefinition resource, int newCount, int totalCollected)
        {
            Resource = resource;
            NewCount = newCount;
            TotalCollected = totalCollected;
        }
    }

    /// <summary>
    /// Fired when a resource is spawned.
    /// </summary>
    public class ResourceSpawnedEvent : IEvent
    {
        public ResourceDefinition Resource { get; set; }
        public Vector3 Position { get; set; }

        public ResourceSpawnedEvent(ResourceDefinition resource, Vector3 position)
        {
            Resource = resource;
            Position = position;
        }
    }
}
