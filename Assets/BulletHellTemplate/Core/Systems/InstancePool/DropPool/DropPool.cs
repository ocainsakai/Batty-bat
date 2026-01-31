using System.Collections.Generic;
using UnityEngine;
#if FUSION_NETWORK
using Fusion;
#endif

namespace BulletHellTemplate.VFX
{
    [AddComponentMenu("Bullet Hell Template/Pooling/Drop Pool")]
    public sealed class DropPool : MonoBehaviour
    {
        public static DropPool Instance { get; private set; }

#if FUSION_NETWORK
        private NetworkObjectPool _net;
#else
        /* passEntryPrefab ➜ Queue */
        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private readonly Dictionary<GameObject, GameObject> _map = new();
#endif

        /* ───────── Init ───────── */
        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

#if FUSION_NETWORK
            _net = GetComponent<NetworkObjectPool>() ??
                   gameObject.AddComponent<NetworkObjectPool>();
#endif
        }

        /* ───────── Spawn ───────── */
        public static DropEntity Spawn(
            DropEntity prefab,
            Vector3 pos,
            Quaternion rot = default)
        {
#if FUSION_NETWORK
            var go = Instance._net.GetInstance(prefab.gameObject, pos, rot);
            return go.GetComponent<DropEntity>();
#else
            if (!Instance._pools.TryGetValue(prefab.gameObject, out var q))
                q = Instance._pools[prefab.gameObject] = new();

            GameObject go = null;
            while (q.Count > 0 && !go)
            {
                var candidate = q.Dequeue();
                if (candidate) go = candidate;      
            }

            if (!go)
                go = Instantiate(prefab.gameObject);

            Instance._map[go] = prefab.gameObject; 

            go.transform.SetPositionAndRotation(pos, rot);
            go.SetActive(true);
            return go.GetComponent<DropEntity>();
#endif
        }

        /* ───────── Release ─────── */
        public static void Release(DropEntity de)
        {
            if (!de) return;
#if FUSION_NETWORK
            Instance._net.ReturnInstance(de.gameObject);
#else
            var inst = de.gameObject;
            if (!inst) return;

            if (!Instance._map.TryGetValue(inst, out var prefab) || prefab == null)
            {
                Destroy(inst);
                return;
            }

            inst.SetActive(false);
            inst.transform.SetParent(Instance.transform, false);

            if (!Instance._pools.TryGetValue(prefab, out var q))
                q = Instance._pools[prefab] = new();

            q.Enqueue(inst);
            Instance._map.Remove(inst);
#endif
        }
    }
}
