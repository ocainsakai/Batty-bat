using System.Collections.Generic;
using UnityEngine;
#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    public class BoxSpawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        public int maxBoxes = 5;
        public float spawnCooldown = 5f;
        public GameObject boxPrefab;
        public float spawnRadius = 10f;
        public LayerMask blockMask = 0; 

        [Header("Network")]
        public bool useNetworkIfAvailable = true;

        private readonly List<GameObject> currentBoxes = new();
        private float cooldownTimer;

#if FUSION2
        private NetworkRunner Runner =>
            GameplaySync.Instance ? GameplaySync.Instance.Runner : null;

        private bool NetActive =>
            useNetworkIfAvailable &&
            GameplaySync.Instance && GameplaySync.Instance.RunnerActive;

        private bool IAmHost =>
            NetActive && GameplaySync.Instance.IsHost;
#endif

        void OnEnable() => cooldownTimer = spawnCooldown;

        void Update()
        {
            if (GameplayManager.Singleton.IsPaused()) return;

#if FUSION2
            if (NetActive && !IAmHost) return;
#endif

            cooldownTimer -= Time.deltaTime;
            currentBoxes.RemoveAll(go => !go);

            if (cooldownTimer > 0f) return;
            if (currentBoxes.Count >= maxBoxes)
            {
                cooldownTimer = spawnCooldown;
                return;
            }

            if (TryGetSpawnPoint(out var pos))
            {
#if FUSION2
                if (NetActive)
                {
                    var nob = Runner.Spawn(boxPrefab, pos, Quaternion.identity);
                    currentBoxes.Add(nob.gameObject);
                }
                else
#endif
                {
                    var go = Instantiate(boxPrefab, pos, Quaternion.identity);
                    currentBoxes.Add(go);
                }
            }

            cooldownTimer = spawnCooldown;
        }

        private bool TryGetSpawnPoint(out Vector3 spawnPoint)
        {
            for (int attempts = 0; attempts < 10; attempts++)
            {
                var p = transform.position + Random.insideUnitSphere * spawnRadius;
                p.y = transform.position.y;

                if (!Physics.CheckSphere(p, 0.3f, blockMask))
                {
                    spawnPoint = p;
                    return true;
                }
            }
            spawnPoint = Vector3.zero;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
    }
}
