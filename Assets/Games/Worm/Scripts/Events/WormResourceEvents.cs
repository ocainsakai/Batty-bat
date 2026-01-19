using UnityEngine;
using Core.EventSystem;
using Games.Worm.Data;

namespace Games.Worm.Events
{
    /// <summary>
    /// Resource-related events
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
