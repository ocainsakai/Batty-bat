namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// Interface for objects that can be collected by a collector.
    /// Implement this on any object that should be collectible.
    /// </summary>
    public interface ICollectable
    {
        /// <summary>
        /// The definition/configuration of this collectable
        /// </summary>
        CollectibleDefinition Definition { get; }

        /// <summary>
        /// Called when this collectable is collected
        /// </summary>
        /// <param name="collector">The GameObject that collected this</param>
        void OnCollected(UnityEngine.GameObject collector);

        /// <summary>
        /// Check if this collectable can be collected by the given collector
        /// </summary>
        bool CanBeCollected(UnityEngine.GameObject collector);
    }
}
