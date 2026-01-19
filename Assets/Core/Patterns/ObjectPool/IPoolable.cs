namespace Core.Patterns.ObjectPool
{
    /// <summary>
    /// Interface for objects that can be pooled.
    /// Implement this to get callbacks when object is spawned/despawned from pool.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Called when object is taken from pool (spawned).
        /// Use this to reset object state, enable components, etc.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when object is returned to pool (despawned).
        /// Use this to cleanup, disable components, stop coroutines, etc.
        /// </summary>
        void OnDespawn();
    }
}
