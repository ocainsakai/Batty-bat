using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Core.Patterns.ObjectPool
{
    /// <summary>
    /// Object pool specifically for Unity Components/MonoBehaviours.
    /// Handles GameObject instantiation, positioning, and parenting.
    /// </summary>
    /// <typeparam name="T">Component type to pool</typeparam>
    public class ComponentPool<T> where T : Component
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _pool;
        private readonly int _maxSize;
        private readonly bool _setActiveOnSpawn;
        private readonly bool _setInactiveOnDespawn;

        private int _countActive;

        /// <summary>
        /// Number of components currently in the pool (inactive)
        /// </summary>
        public int CountInactive => _pool.Count;

        /// <summary>
        /// Number of components currently active (spawned)
        /// </summary>
        public int CountActive => _countActive;

        /// <summary>
        /// Total number of components created by this pool
        /// </summary>
        public int CountAll => CountInactive + CountActive;

        /// <summary>
        /// Creates a new component pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="parent">Parent transform for pooled objects (optional)</param>
        /// <param name="defaultCapacity">Initial capacity</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        /// <param name="setActiveOnSpawn">Auto SetActive(true) on spawn</param>
        /// <param name="setInactiveOnDespawn">Auto SetActive(false) on despawn</param>
        public ComponentPool(
            GameObject prefab,
            Transform parent = null,
            int defaultCapacity = 10,
            int maxSize = 100,
            bool setActiveOnSpawn = true,
            bool setInactiveOnDespawn = true)
        {
            _prefab = prefab ? prefab : throw new System.ArgumentNullException(nameof(prefab));
            _parent = parent;
            _maxSize = maxSize;
            _setActiveOnSpawn = setActiveOnSpawn;
            _setInactiveOnDespawn = setInactiveOnDespawn;
            _pool = new Stack<T>(defaultCapacity);
            _countActive = 0;
        }

        /// <summary>
        /// Spawn a component from the pool at specified position and rotation
        /// </summary>
        public T Spawn(Vector3 position, Quaternion rotation)
        {
            T component;

            if (_pool.Count > 0)
            {
                component = _pool.Pop();
            }
            else
            {
                GameObject go = Object.Instantiate(_prefab, position, rotation, _parent);
                component = go.GetComponent<T>();

                if (component == null)
                {
                    Debug.LogError($"[ComponentPool] Prefab {_prefab.name} does not have component {typeof(T).Name}");
                    Object.Destroy(go);
                    return null;
                }
            }

            // Set position and rotation
            component.transform.position = position;
            component.transform.rotation = rotation;

            // Activate if needed
            if (_setActiveOnSpawn)
            {
                component.gameObject.SetActive(true);
            }

            // Call IPoolable.OnSpawn if implemented
            if (component is IPoolable poolable)
            {
                poolable.OnSpawn();
            }

            _countActive++;

            return component;
        }

        /// <summary>
        /// Spawn a component from the pool at default position
        /// </summary>
        public T Spawn()
        {
            return Spawn(Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Return a component to the pool
        /// </summary>
        public void Despawn(T component)
        {
            if (component == null)
            {
                Debug.LogWarning("[ComponentPool] Trying to despawn null component");
                return;
            }

            // Call IPoolable.OnDespawn if implemented
            if (component is IPoolable poolable)
            {
                poolable.OnDespawn();
            }

            // Deactivate if needed
            if (_setInactiveOnDespawn)
            {
                component.gameObject.SetActive(false);
            }

            // Parent to pool parent
            if (_parent != null)
            {
                component.transform.SetParent(_parent);
            }

            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                // Pool is full, destroy the object
                Object.Destroy(component.gameObject);
            }
            else
            {
                _pool.Push(component);
            }

            _countActive--;
        }

        /// <summary>
        /// Despawn a component after a delay using UniTask
        /// </summary>
        public async void DespawnAfter(T component, float delay)
        {
            if (component == null || component.gameObject == null)
                return;

            try
            {
                await Cysharp.Threading.Tasks.UniTask.Delay(
                    System.TimeSpan.FromSeconds(delay),
                    cancellationToken: component.GetCancellationTokenOnDestroy()
                );

                // Check if component still exists after delay
                if (component != null && component.gameObject != null)
                {
                    Despawn(component);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Component was destroyed during delay - this is expected
            }
        }

        /// <summary>
        /// Pre-instantiate components to warm up the pool
        /// </summary>
        public void Warmup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = Object.Instantiate(_prefab, _parent);
                T component = go.GetComponent<T>();

                if (component != null)
                {
                    go.SetActive(false);
                    _pool.Push(component);
                }
                else
                {
                    Debug.LogError($"[ComponentPool] Prefab {_prefab.name} does not have component {typeof(T).Name}");
                    Object.Destroy(go);
                }
            }

            Debug.Log($"[ComponentPool] Warmed up pool '{typeof(T).Name}' with {count} objects. Total in pool: {CountInactive}");
        }

        /// <summary>
        /// Clear all components from the pool
        /// </summary>
        public void Clear()
        {
            while (_pool.Count > 0)
            {
                T component = _pool.Pop();
                if (component != null)
                {
                    Object.Destroy(component.gameObject);
                }
            }

            Debug.Log($"[ComponentPool] Pool '{typeof(T).Name}' cleared");
        }
    }
}
