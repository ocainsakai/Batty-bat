using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;          // async/await for Unity

#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate.VFX
{
    [DisallowMultipleComponent]
    public sealed class MonsterPool : MonoBehaviour
    {
        /*─────────────────────────  Singleton  ─────────────────────────*/
        public static MonsterPool Instance { get; private set; }

        /*─────────────────────────  Local pool  ────────────────────────*/
#if !FUSION2
        //  Off‑line build: simple queue pool per‑prefab
        private readonly Dictionary<GameObject, Queue<GameObject>> _pools            = new();
        private readonly Dictionary<GameObject, GameObject>        _instanceToPrefab = new();
#else
        //  Online build: the local pool is only a fallback
        private readonly Dictionary<GameObject, Queue<GameObject>> _fallbackPools = new();
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

        private NetworkRunner _runner;          // cached runner reference
#endif

        /*─────────────────────────  Initialization  ────────────────────*/
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

#if FUSION2
        /// <summary>Lazy‑find (and cache) the NetworkRunner in the scene.</summary>
        private NetworkRunner EnsureRunner()
        {
            if (_runner != null) return _runner;

            if (FusionLobbyManager.Instance != null)
                _runner = FusionLobbyManager.Instance.Runner;

#if UNITY_6000_0_OR_NEWER
            if (_runner == null)
                _runner = FindFirstObjectByType<NetworkRunner>(FindObjectsInactive.Include);
#else
            if (_runner == null)
                _runner = FindObjectOfType<NetworkRunner>(true);
#endif
            return _runner;
        }
#endif

        /*─────────────────────────  Pre‑loading  ───────────────────────*/
        /// <summary>
        /// Optionally pre‑instantiates monsters for all configured waves
        /// (useful only for off‑line mobile builds; Fusion handles pooling).
        /// </summary>
        public async UniTask PreloadAsync(IEnumerable<Wave> waves)
        {
#if FUSION2
            await UniTask.CompletedTask;  
#else
            foreach (Wave wave in waves)
            {
                foreach (var cfg in wave.monsters)
                {
                    int qty = Mathf.CeilToInt(wave.waveDuration / cfg.spawnInterval);
                    for (int i = 0; i < qty; ++i)
                    {
                        var go = await Spawn(cfg.monsterPrefab.gameObject,
                                             Vector3.zero,
                                             Quaternion.identity);
                        Release(go);
                    }
                    await UniTask.Yield();
                }
            }
#endif
        }

        /*────────────────────────────  Spawn  ──────────────────────────*/
        public async UniTask<GameObject> Spawn(
            GameObject prefab,
            Vector3 pos,
            Quaternion rot

        #if FUSION2
            ,bool useFusionIfPossible = true   
        #endif
)
        {
#if FUSION2
            NetworkRunner runner = useFusionIfPossible ? EnsureRunner() : null;
            bool useFusion = runner && runner.IsRunning && runner.IsSceneAuthority &&
                             runner.State == NetworkRunner.States.Running;

            if (useFusion)
            {
                NetworkObject netObj = await runner.SpawnAsync(
                    prefab, pos, rot, runner.LocalPlayer);
                return netObj.gameObject;
            }
#endif
            await Task.Yield();
            /* Offline build or fallback */
            return SpawnLocal(prefab, pos, rot);
        }

        /*───────────────────────────  Release  ─────────────────────────*/
        /// <summary>
        /// Despawns / returns an instance to the appropriate pool.
        /// </summary>
        public void Release(GameObject instance)
        {
            if (instance == null) return;

#if FUSION2
            NetworkRunner runner = EnsureRunner();
            var netObj = instance.GetComponent<NetworkObject>();

            if (runner != null && runner.IsRunning && netObj != null)
            {
                runner.Despawn(netObj);
                return;
            }
            /* else fall through to local queue‑pool release */
            ReleaseLocal(instance, _fallbackPools);
#else
            ReleaseLocal(instance, _pools);
#endif
        }

        /*──────────────────────  LOCAL POOL HELPERS  ───────────────────*/
        private GameObject SpawnLocal(GameObject prefab, Vector3 pos, Quaternion rot)
        {
#if FUSION2
            var pools = _fallbackPools;
#else
            var pools = _pools;
#endif
            if (!pools.TryGetValue(prefab, out var q))
                q = pools[prefab] = new Queue<GameObject>();

            GameObject go = q.Count > 0 ? q.Dequeue()
                                        : Instantiate(prefab);

            _instanceToPrefab[go] = prefab;
            go.transform.SetPositionAndRotation(pos, rot);
            go.SetActive(true);
            return go;
        }

        private void ReleaseLocal(GameObject instance, Dictionary<GameObject, Queue<GameObject>> pools)
        {
            if (!_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                Debug.LogWarning($"[MonsterPool] Instance “{instance.name}” was not created by this pool — destroying.");
                Destroy(instance);
                return;
            }

            if (instance.TryGetComponent(out MonsterMovementComponent move))
                move.Shutdown();                           

            instance.SetActive(false);
            instance.transform.SetParent(transform, false);

            pools[prefab].Enqueue(instance);
            _instanceToPrefab.Remove(instance);
        }

        public void DestroyPool()
        {
#if FUSION2
            foreach (var kv in _fallbackPools)           
                while (kv.Value.Count > 0)
                    Destroy(kv.Value.Dequeue());
            _fallbackPools.Clear();
#else
            foreach (var kv in _pools)
                while (kv.Value.Count > 0)
                    Destroy(kv.Value.Dequeue());
            _pools.Clear();
#endif
            _instanceToPrefab.Clear();
        }
    }
}
