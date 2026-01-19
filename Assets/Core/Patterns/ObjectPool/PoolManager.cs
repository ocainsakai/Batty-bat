using System.Collections.Generic;
using UnityEngine;
using Core.Patterns;

namespace Core.Patterns.ObjectPool
{
    /// <summary>
    /// Centralized manager for all object pools in the game.
    /// Singleton that persists across scenes.
    /// </summary>
    public class PoolManager : PersistentSingleton<PoolManager>
    {
        private Dictionary<string, object> _pools = new Dictionary<string, object>();
        private Transform _poolsParent;

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                // Create parent transform for all pools
                _poolsParent = new GameObject("Pools").transform;
                _poolsParent.SetParent(transform);
            }
        }

        /// <summary>
        /// Create a new component pool
        /// </summary>
        public ComponentPool<T> CreatePool<T>(string poolName, GameObject prefab, int initialSize = 10, int maxSize = 100) where T : Component
        {
            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists");
                return GetPool<T>(poolName);
            }

            // Create parent transform for this pool
            Transform poolParent = new GameObject(poolName).transform;
            poolParent.SetParent(_poolsParent);

            var pool = new ComponentPool<T>(prefab, poolParent, initialSize, maxSize);
            _pools[poolName] = pool;

            // Warmup if initial size > 0
            if (initialSize > 0)
            {
                pool.Warmup(initialSize);
            }

            Debug.Log($"[PoolManager] Created pool '{poolName}' for type {typeof(T).Name}");
            return pool;
        }

        /// <summary>
        /// Get an existing pool
        /// </summary>
        public ComponentPool<T> GetPool<T>(string poolName) where T : Component
        {
            if (_pools.TryGetValue(poolName, out object pool))
            {
                return pool as ComponentPool<T>;
            }

            Debug.LogWarning($"[PoolManager] Pool '{poolName}' not found");
            return null;
        }

        /// <summary>
        /// Spawn an object from a pool
        /// </summary>
        public T Spawn<T>(string poolName, Vector3 position, Quaternion rotation) where T : Component
        {
            var pool = GetPool<T>(poolName);
            if (pool != null)
            {
                return pool.Spawn(position, rotation);
            }

            return null;
        }

        /// <summary>
        /// Spawn an object from a pool at default position
        /// </summary>
        public T Spawn<T>(string poolName) where T : Component
        {
            return Spawn<T>(poolName, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Despawn an object back to its pool
        /// </summary>
        public void Despawn<T>(string poolName, T component) where T : Component
        {
            var pool = GetPool<T>(poolName);
            if (pool != null)
            {
                pool.Despawn(component);
            }
        }

        /// <summary>
        /// Despawn an object after a delay
        /// </summary>
        public void DespawnAfter<T>(string poolName, T component, float delay) where T : Component
        {
            var pool = GetPool<T>(poolName);
            if (pool != null)
            {
                pool.DespawnAfter(component, delay);
            }
        }

        /// <summary>
        /// Clear a specific pool
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (_pools.TryGetValue(poolName, out object pool))
            {
                if (pool is ComponentPool<Component> componentPool)
                {
                    componentPool.Clear();
                }
                _pools.Remove(poolName);
                Debug.Log($"[PoolManager] Cleared pool '{poolName}'");
            }
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var kvp in _pools)
            {
                if (kvp.Value is ComponentPool<Component> pool)
                {
                    pool.Clear();
                }
            }

            _pools.Clear();
            Debug.Log("[PoolManager] Cleared all pools");
        }

        /// <summary>
        /// Get pool statistics
        /// </summary>
        public void PrintPoolStats()
        {
            Debug.Log($"[PoolManager] Total pools: {_pools.Count}");
            foreach (var kvp in _pools)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value}");
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearAllPools();
        }
    }
}
