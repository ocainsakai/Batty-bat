using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Manages spawning of animals in the game world.
    /// </summary>
    public class AnimalSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<GameObject> animalPrefabs = new();
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Transform spawnParent;

        [Header("Timing")]
        [SerializeField] private float initialDelay = 1f;
        [SerializeField] private float minSpawnInterval = 3f;
        [SerializeField] private float maxSpawnInterval = 8f;

        [Header("Limits")]
        [SerializeField] private int maxAnimalsOnScreen = 3;
        [SerializeField] private bool autoSpawn = true;

        private List<AnimalEntity> _activeAnimals = new();
        private bool _isSpawning = false;

        public int ActiveAnimalCount => _activeAnimals.Count;
        public bool CanSpawn => _activeAnimals.Count < maxAnimalsOnScreen;

        private void Start()
        {
            if (autoSpawn)
            {
                StartSpawning();
            }
        }

        public void StartSpawning()
        {
            if (_isSpawning) return;
            _isSpawning = true;
            SpawnLoop().Forget();
        }

        public void StopSpawning()
        {
            _isSpawning = false;
        }

        private async UniTaskVoid SpawnLoop()
        {
            await UniTask.WaitForSeconds(initialDelay);

            while (_isSpawning)
            {
                if (CanSpawn)
                {
                    SpawnRandomAnimal();
                }

                float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
                await UniTask.WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Spawn a random animal at a random spawn point
        /// </summary>
        public AnimalEntity SpawnRandomAnimal()
        {
            if (animalPrefabs.Count == 0)
            {
                Debug.LogWarning("[AnimalSpawner] No animal prefabs assigned!");
                return null;
            }

            if (!CanSpawn)
            {
                Debug.Log("[AnimalSpawner] Max animals on screen reached");
                return null;
            }

            // Pick random prefab
            int prefabIndex = Random.Range(0, animalPrefabs.Count);
            var prefab = animalPrefabs[prefabIndex];

            // Pick random spawn point
            Vector3 spawnPosition = GetRandomSpawnPosition();

            return SpawnAnimal(prefab, spawnPosition);
        }

        /// <summary>
        /// Spawn a specific animal prefab
        /// </summary>
        public AnimalEntity SpawnAnimal(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return null;

            var parent = spawnParent != null ? spawnParent : transform;
            var instance = Instantiate(prefab, position, Quaternion.identity, parent);
            
            var entity = instance.GetComponent<AnimalEntity>();
            if (entity != null)
            {
                RegisterAnimal(entity);
            }

            Debug.Log($"[AnimalSpawner] Spawned animal at {position}");
            return entity;
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int index = Random.Range(0, spawnPoints.Length);
                return spawnPoints[index].position;
            }

            // Fallback: Random position in view
            float x = Random.Range(-5f, 5f);
            float y = Random.Range(-3f, 3f);
            return new Vector3(x, y, 0);
        }

        private void RegisterAnimal(AnimalEntity animal)
        {
            _activeAnimals.Add(animal);
            animal.OnAnimalLeft += HandleAnimalLeft;
        }

        private void HandleAnimalLeft(AnimalEntity animal)
        {
            animal.OnAnimalLeft -= HandleAnimalLeft;
            _activeAnimals.Remove(animal);
        }

        /// <summary>
        /// Clear all active animals (for scene cleanup)
        /// </summary>
        public void ClearAllAnimals()
        {
            foreach (var animal in _activeAnimals)
            {
                if (animal != null)
                {
                    Destroy(animal.gameObject);
                }
            }
            _activeAnimals.Clear();
        }

        private void OnDestroy()
        {
            StopSpawning();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null) return;

            Gizmos.color = Color.green;
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
#endif
    }
}
