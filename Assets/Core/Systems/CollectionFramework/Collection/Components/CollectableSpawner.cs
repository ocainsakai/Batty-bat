using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Core.Patterns;
using Core.Patterns.ObjectPool;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// Manages spawning and respawning of collectables.
    /// Uses object pooling for performance.
    /// </summary>
    public class CollectableSpawner : MonoSingleton<CollectableSpawner>
    {
        [Header("Collectables")]
        [SerializeField] private List<CollectibleDefinition> collectibleDefinitions = new List<CollectibleDefinition>();
        [SerializeField] private GameObject collectablePrefab;

        [Header("Spawning")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private Transform collectablesParent;

        private Dictionary<string, ComponentPool<Collectable>> _pools = new Dictionary<string, ComponentPool<Collectable>>();
        private Dictionary<string, List<Collectable>> _activeCollectables = new Dictionary<string, List<Collectable>>();
        private Dictionary<string, int> _respawnQueue = new Dictionary<string, int>();

        protected override void Awake()
        {
            base.Awake();

            if (Instance == this)
            {
                InitializePools();
            }
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnInitialCollectables();
            }
        }

        private void InitializePools()
        {
            if (collectablePrefab == null)
            {
                Debug.LogError("[CollectableSpawner] Collectable prefab is not assigned!");
                return;
            }

            // Create parent if not assigned
            if (collectablesParent == null)
            {
                collectablesParent = new GameObject("Collectables").transform;
                collectablesParent.SetParent(transform);
            }

            // Create pools for each collectable type
            foreach (var definition in collectibleDefinitions)
            {
                if (definition == null) continue;

                string poolName = "Collectable_" + definition.CollectibleID;

                var pool = PoolManager.Instance.CreatePool<Collectable>(
                    poolName,
                    collectablePrefab,
                    initialSize: definition.MaxSpawnCount,
                    maxSize: definition.MaxSpawnCount * 2
                );

                _pools[definition.CollectibleID] = pool;
                _activeCollectables[definition.CollectibleID] = new List<Collectable>();
                _respawnQueue[definition.CollectibleID] = 0;

                Debug.Log($"[CollectableSpawner] Created pool for {definition.CollectibleName}");
            }
        }

        private void SpawnInitialCollectables()
        {
            foreach (var definition in collectibleDefinitions)
            {
                if (definition == null) continue;

                for (int i = 0; i < definition.MaxSpawnCount; i++)
                {
                    Spawn(definition);
                }
            }

            Debug.Log($"[CollectableSpawner] Spawned initial collectables for {collectibleDefinitions.Count} types");
        }

        /// <summary>
        /// Spawn a single collectable
        /// </summary>
        public Collectable Spawn(CollectibleDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogWarning("[CollectableSpawner] Trying to spawn null definition");
                return null;
            }

            Vector3 position = GetSpawnPosition(definition);
            string poolName = "Collectable_" + definition.CollectibleID;

            var collectable = PoolManager.Instance.Spawn<Collectable>(
                poolName,
                position,
                Quaternion.identity
            );

            if (collectable != null)
            {
                collectable.Initialize(definition);

                // Track active collectable
                if (_activeCollectables.ContainsKey(definition.CollectibleID))
                {
                    _activeCollectables[definition.CollectibleID].Add(collectable);
                }
            }

            return collectable;
        }

        /// <summary>
        /// Schedule a collectable to respawn after its respawn time
        /// </summary>
        public async void ScheduleRespawn(CollectibleDefinition definition)
        {
            if (definition == null || definition.RespawnTime <= 0) return;

            // Increment respawn queue
            if (_respawnQueue.ContainsKey(definition.CollectibleID))
            {
                _respawnQueue[definition.CollectibleID]++;
            }

            // Wait for respawn time
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(definition.RespawnTime),
                cancellationToken: this.GetCancellationTokenOnDestroy()
            );

            // Decrement queue and spawn
            if (_respawnQueue.ContainsKey(definition.CollectibleID))
            {
                _respawnQueue[definition.CollectibleID]--;
                Spawn(definition);
            }
        }

        /// <summary>
        /// Despawn a collectable (return to pool)
        /// </summary>
        public void Despawn(Collectable collectable)
        {
            if (collectable == null || collectable.Definition == null) return;

            string poolName = "Collectable_" + collectable.Definition.CollectibleID;
            PoolManager.Instance.Despawn(poolName, collectable);

            // Remove from active list
            if (_activeCollectables.ContainsKey(collectable.Definition.CollectibleID))
            {
                _activeCollectables[collectable.Definition.CollectibleID].Remove(collectable);
            }
        }

        private Vector3 GetSpawnPosition(CollectibleDefinition definition)
        {
            switch (definition.Pattern)
            {
                case SpawnPattern.Random:
                    return GetRandomPosition(definition);

                case SpawnPattern.Grid:
                    return GetGridPosition(definition);

                case SpawnPattern.Cluster:
                    return GetClusterPosition(definition);

                case SpawnPattern.Circle:
                    return GetCirclePosition(definition);

                case SpawnPattern.Line:
                    return GetLinePosition(definition);

                default:
                    return GetRandomPosition(definition);
            }
        }

        private Vector3 GetRandomPosition(CollectibleDefinition definition)
        {
            return new Vector3(
                Random.Range(definition.SpawnAreaMin.x, definition.SpawnAreaMax.x),
                Random.Range(definition.SpawnAreaMin.y, definition.SpawnAreaMax.y),
                0f
            );
        }

        private Vector3 GetGridPosition(CollectibleDefinition definition)
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(definition.MaxSpawnCount));
            Vector2 areaSize = definition.SpawnAreaMax - definition.SpawnAreaMin;
            Vector2 cellSize = areaSize / gridSize;

            int activeCount = _activeCollectables.ContainsKey(definition.CollectibleID)
                ? _activeCollectables[definition.CollectibleID].Count
                : 0;

            int x = activeCount % gridSize;
            int y = activeCount / gridSize;

            return new Vector3(
                definition.SpawnAreaMin.x + (x + 0.5f) * cellSize.x,
                definition.SpawnAreaMin.y + (y + 0.5f) * cellSize.y,
                0f
            );
        }

        private Vector3 GetClusterPosition(CollectibleDefinition definition)
        {
            Vector3 clusterCenter = GetRandomPosition(definition);
            float clusterRadius = 2f;

            Vector2 randomOffset = Random.insideUnitCircle * clusterRadius;
            return clusterCenter + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        private Vector3 GetCirclePosition(CollectibleDefinition definition)
        {
            Vector2 center = (definition.SpawnAreaMin + definition.SpawnAreaMax) * 0.5f;
            float radius = Mathf.Min(
                (definition.SpawnAreaMax.x - definition.SpawnAreaMin.x) * 0.4f,
                (definition.SpawnAreaMax.y - definition.SpawnAreaMin.y) * 0.4f
            );

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            return new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius,
                0f
            );
        }

        private Vector3 GetLinePosition(CollectibleDefinition definition)
        {
            float t = Random.Range(0f, 1f);
            return Vector3.Lerp(
                new Vector3(definition.SpawnAreaMin.x, definition.SpawnAreaMin.y, 0f),
                new Vector3(definition.SpawnAreaMax.x, definition.SpawnAreaMax.y, 0f),
                t
            );
        }

        /// <summary>
        /// Clear all collectables
        /// </summary>
        public void ClearAll()
        {
            foreach (var kvp in _activeCollectables)
            {
                kvp.Value.Clear();
            }

            foreach (var kvp in _respawnQueue)
            {
                _respawnQueue[kvp.Key] = 0;
            }
        }
    }
}
