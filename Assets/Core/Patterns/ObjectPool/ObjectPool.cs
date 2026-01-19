using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Patterns.ObjectPool
{
    /// <summary>
    /// Generic object pool for any class type.
    /// Thread-safe implementation with configurable size and callbacks.
    /// </summary>
    /// <typeparam name="T">Type of objects to pool</typeparam>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _pool;
        private readonly int _maxSize;
        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;
        private readonly Action<T> _onDestroy;
        private readonly object _lock = new object();

        private int _countActive;

        /// <summary>
        /// Number of objects currently in the pool (inactive)
        /// </summary>
        public int CountInactive
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// Number of objects currently active (taken from pool)
        /// </summary>
        public int CountActive => _countActive;

        /// <summary>
        /// Total number of objects created by this pool
        /// </summary>
        public int CountAll => CountInactive + CountActive;

        /// <summary>
        /// Creates a new object pool
        /// </summary>
        /// <param name="createFunc">Function to create new objects</param>
        /// <param name="onGet">Called when object is taken from pool (optional)</param>
        /// <param name="onReturn">Called when object is returned to pool (optional)</param>
        /// <param name="onDestroy">Called when object is destroyed (optional)</param>
        /// <param name="defaultCapacity">Initial capacity of pool</param>
        /// <param name="maxSize">Maximum size of pool (0 = unlimited)</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onReturn = null,
            Action<T> onDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _onGet = onGet;
            _onReturn = onReturn;
            _onDestroy = onDestroy;
            _maxSize = maxSize;
            _pool = new Stack<T>(defaultCapacity);
            _countActive = 0;
        }

        /// <summary>
        /// Get an object from the pool. Creates new one if pool is empty.
        /// </summary>
        public T Get()
        {
            T obj;

            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    obj = _pool.Pop();
                }
                else
                {
                    obj = _createFunc();
                }

                _countActive++;
            }

            try
            {
                _onGet?.Invoke(obj);

                // Call IPoolable.OnSpawn if implemented
                if (obj is IPoolable poolable)
                {
                    poolable.OnSpawn();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ObjectPool] Error in OnGet callback: {ex.Message}");
            }

            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPool] Trying to return null object to pool");
                return;
            }

            try
            {
                // Call IPoolable.OnDespawn if implemented
                if (obj is IPoolable poolable)
                {
                    poolable.OnDespawn();
                }

                _onReturn?.Invoke(obj);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ObjectPool] Error in OnReturn callback: {ex.Message}");
            }

            lock (_lock)
            {
                if (_maxSize > 0 && _pool.Count >= _maxSize)
                {
                    // Pool is full, destroy the object
                    try
                    {
                        _onDestroy?.Invoke(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ObjectPool] Error in OnDestroy callback: {ex.Message}");
                    }
                }
                else
                {
                    _pool.Push(obj);
                }

                _countActive--;
            }
        }

        /// <summary>
        /// Pre-instantiate objects to warm up the pool
        /// </summary>
        public void Warmup(int count)
        {
            lock (_lock)
            {
                for (int i = 0; i < count; i++)
                {
                    T obj = _createFunc();
                    _pool.Push(obj);
                }
            }

            Debug.Log($"[ObjectPool] Warmed up pool with {count} objects. Total in pool: {CountInactive}");
        }

        /// <summary>
        /// Clear all objects from the pool
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                while (_pool.Count > 0)
                {
                    T obj = _pool.Pop();
                    try
                    {
                        _onDestroy?.Invoke(obj);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ObjectPool] Error destroying object: {ex.Message}");
                    }
                }
            }

            Debug.Log("[ObjectPool] Pool cleared");
        }
    }
}
