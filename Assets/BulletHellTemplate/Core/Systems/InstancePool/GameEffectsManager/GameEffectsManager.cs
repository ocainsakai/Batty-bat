using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate.VFX
{
    /// <summary>
    ///     Central manager that spawns and reuses visual effects.
    ///     Each passEntryPrefab owns its own queue-based pool (created lazily).
    /// </summary>
    [AddComponentMenu("Bullet Hell Template/VFX/Game Effects Manager")]
    public sealed class GameEffectsManager : MonoBehaviour
    {
        /// <summary>Singleton accessor (auto-creates if absent).</summary>
        public static GameEffectsManager Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_6000_0_OR_NEWER
                    _instance = FindFirstObjectByType<GameEffectsManager>();
#else
                    _instance = FindObjectOfType<GameEffectsManager>();
#endif

                    if (_instance == null)
                    {
                        var go = new GameObject(nameof(GameEffectsManager));
                        _instance = go.AddComponent<GameEffectsManager>();
                    }
                }
                return _instance;
            }
        }
        private static GameEffectsManager _instance;

        /* ───── Internal State ───── */
        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        #region Public API ----------------------------------------------------

        /// <summary>
        /// Spawns (or re-uses) an effect passEntryPrefab at the given position/rotation.
        /// </summary>
        /// <param name="prefab">Effect passEntryPrefab to spawn.</param>
        /// <param name="pos">World position.</param>
        /// <param name="rot">World rotation.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <returns>The spawned (or recycled) GameObject.</returns>
        public static GameObject SpawnEffect(
            GameObject prefab,
            Vector3 pos,
            Quaternion rot,
            Transform parent = null)
        {
            if (prefab == null) return null;

            var keyPrefab = Instance.GetPrefabKey(prefab);
            GameObject go = Instance.GetFromPool(keyPrefab);

            go.transform.SetPositionAndRotation(pos, rot);
            go.transform.SetParent(parent, worldPositionStays: true);
            go.SetActive(true);

            return go;
        }

        /// <summary>
        /// Releases an effect back to its pool.
        /// Call this when the effect finishes playing.
        /// </summary>
        /// <param name="instance">The spawned instance to recycle.</param>
        public static void ReleaseEffect(GameObject instance)
        {
            if (instance == null) return;
            Instance.ReturnToPool(instance);
        }

        #endregion

        #region Pool Logic ----------------------------------------------------

        private GameObject GetPrefabKey(GameObject passed)
        {
            return _instanceToPrefab.TryGetValue(passed, out var original)
                ? original
                : passed;
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools.Add(prefab, queue);
            }

            GameObject go = queue.Count > 0 ? queue.Dequeue()
                                            : Instantiate(prefab);

            _instanceToPrefab[go] = prefab;
            return go;
        }

        private void ReturnToPool(GameObject instance)
        {
            if (!_instanceToPrefab.TryGetValue(instance, out var prefab) ||
                !_pools.TryGetValue(prefab, out var queue))
            {
                // Instance not tracked: destroy to avoid leaks.
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            queue.Enqueue(instance);
        }

        #endregion
    }
}
