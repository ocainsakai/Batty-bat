using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Core.Patterns;
using Core.Patterns.ObjectPool;
using Games.Worm.Data;

namespace Games.Worm.Resources
{
    /// <summary>
    /// Manages spawning and respawning of all resources in the game.
    /// Uses object pooling for performance.
    /// </summary>
    public class ResourceSpawner : MonoSingleton<ResourceSpawner>
    {
        [Header("Resources")]
        [SerializeField] private List<ResourceDefinition> resourceDefinitions = new List<ResourceDefinition>();
        [SerializeField] private GameObject resourcePrefab;

        [Header("Spawning")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private Transform resourcesParent;

        private Dictionary<string, ComponentPool<Resource>> _resourcePools = new Dictionary<string, ComponentPool<Resource>>();
        private Dictionary<string, List<Resource>> _activeResources = new Dictionary<string, List<Resource>>();
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
                SpawnInitialResources();
            }
        }

        private void InitializePools()
        {
            if (resourcePrefab == null)
            {
                Debug.LogError("[ResourceSpawner] Resource prefab is not assigned!");
                return;
            }

            // Create parent for resources if not assigned
            if (resourcesParent == null)
            {
                resourcesParent = new GameObject("Resources").transform;
                resourcesParent.SetParent(transform);
            }

            // Create pools for each resource type
            foreach (var definition in resourceDefinitions)
            {
                if (definition == null) continue;

                string poolName = "Resource_" + definition.ResourceID;
                
                // Create pool using PoolManager
                var pool = PoolManager.Instance.CreatePool<Resource>(
                    poolName,
                    resourcePrefab,
                    initialSize: definition.MaxSpawnCount,
                    maxSize: definition.MaxSpawnCount * 2
                );

                _resourcePools[definition.ResourceID] = pool;
                _activeResources[definition.ResourceID] = new List<Resource>();
                _respawnQueue[definition.ResourceID] = 0;

                Debug.Log($"[ResourceSpawner] Created pool for {definition.ResourceName} ({definition.ResourceID})");
            }
        }

        private void SpawnInitialResources()
        {
            foreach (var definition in resourceDefinitions)
            {
                if (definition == null) continue;

                for (int i = 0; i < definition.MaxSpawnCount; i++)
                {
                    SpawnResource(definition);
                }
            }

            Debug.Log($"[ResourceSpawner] Spawned initial resources for {resourceDefinitions.Count} types");
        }

        /// <summary>
        /// Spawn a single resource
        /// </summary>
        public Resource SpawnResource(ResourceDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogWarning("[ResourceSpawner] Trying to spawn null resource definition");
                return null;
            }

            Vector3 position = GetSpawnPosition(definition);
            string poolName = "Resource_" + definition.ResourceID;

            var resource = PoolManager.Instance.Spawn<Resource>(
                poolName,
                position,
                Quaternion.identity
            );

            if (resource != null)
            {
                resource.Initialize(definition);
                
                // Track active resource
                if (_activeResources.ContainsKey(definition.ResourceID))
                {
                    _activeResources[definition.ResourceID].Add(resource);
                }
            }

            return resource;
        }

        /// <summary>
        /// Schedule a resource to respawn after its respawn time
        /// </summary>
        public async void ScheduleRespawn(ResourceDefinition definition)
        {
            if (definition == null) return;

            // Increment respawn queue
            if (_respawnQueue.ContainsKey(definition.ResourceID))
            {
                _respawnQueue[definition.ResourceID]++;
            }

            // Wait for respawn time
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(definition.RespawnTime),
                cancellationToken: this.GetCancellationTokenOnDestroy()
            );

            // Decrement queue and spawn
            if (_respawnQueue.ContainsKey(definition.ResourceID))
            {
                _respawnQueue[definition.ResourceID]--;
                SpawnResource(definition);
            }
        }

        private Vector3 GetSpawnPosition(ResourceDefinition definition)
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

                default:
                    return GetRandomPosition(definition);
            }
        }

        private Vector3 GetRandomPosition(ResourceDefinition definition)
        {
            return new Vector3(
                Random.Range(definition.SpawnAreaMin.x, definition.SpawnAreaMax.x),
                Random.Range(definition.SpawnAreaMin.y, definition.SpawnAreaMax.y),
                0f
            );
        }

        private Vector3 GetGridPosition(ResourceDefinition definition)
        {
            // Simple grid pattern
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(definition.MaxSpawnCount));
            Vector2 areaSize = definition.SpawnAreaMax - definition.SpawnAreaMin;
            Vector2 cellSize = areaSize / gridSize;

            int activeCount = _activeResources.ContainsKey(definition.ResourceID) 
                ? _activeResources[definition.ResourceID].Count 
                : 0;

            int x = activeCount % gridSize;
            int y = activeCount / gridSize;

            return new Vector3(
                definition.SpawnAreaMin.x + (x + 0.5f) * cellSize.x,
                definition.SpawnAreaMin.y + (y + 0.5f) * cellSize.y,
                0f
            );
        }

        private Vector3 GetClusterPosition(ResourceDefinition definition)
        {
            // Random cluster center, then spawn around it
            Vector3 clusterCenter = GetRandomPosition(definition);
            float clusterRadius = 2f;

            Vector2 randomOffset = Random.insideUnitCircle * clusterRadius;
            return clusterCenter + new Vector3(randomOffset.x, randomOffset.y, 0f);
        }

        private Vector3 GetCirclePosition(ResourceDefinition definition)
        {
            // Spawn in circle pattern
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

        /// <summary>
        /// Get statistics for a resource type
        /// </summary>
        public ResourceStats GetResourceStats(string resourceID)
        {
            return new ResourceStats
            {
                ActiveCount = _activeResources.ContainsKey(resourceID) ? _activeResources[resourceID].Count : 0,
                RespawnQueue = _respawnQueue.ContainsKey(resourceID) ? _respawnQueue[resourceID] : 0
            };
        }

        /// <summary>
        /// Clear all resources (for reset/new game)
        /// </summary>
        public void ClearAllResources()
        {
            foreach (var kvp in _activeResources)
            {
                kvp.Value.Clear();
            }

            foreach (var kvp in _respawnQueue)
            {
                _respawnQueue[kvp.Key] = 0;
            }
        }
    }

    public struct ResourceStats
    {
        public int ActiveCount;
        public int RespawnQueue;
    }
}
