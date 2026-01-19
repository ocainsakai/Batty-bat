using UnityEngine;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// Interface for objects that can collect collectables.
    /// Implement this on player, enemies, or any entity that collects items.
    /// </summary>
    public interface ICollector
    {
        /// <summary>
        /// Collect a collectable
        /// </summary>
        void Collect(ICollectable collectable);

        /// <summary>
        /// Check if this collector can collect the given collectable
        /// </summary>
        bool CanCollect(ICollectable collectable);

        /// <summary>
        /// Get the GameObject of this collector
        /// </summary>
        GameObject GameObject { get; }
    }
}
