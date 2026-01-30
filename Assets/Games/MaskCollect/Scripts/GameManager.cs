using UnityEngine;
using MaskCollect.Data;
using MaskCollect.Gameplay;
using MaskCollect.UI;

namespace MaskCollect
{
    /// <summary>
    /// Main game manager for Mask Collect.
    /// Coordinates all game systems and handles game flow.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private MaskDatabase maskDatabase;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private RewardSystem rewardSystem;
        [SerializeField] private AnimalSpawner animalSpawner;

        [Header("UI")]
        [SerializeField] private HUD hud;
        [SerializeField] private CollectionView collectionView;
        [SerializeField] private RewardPopup rewardPopup;

        [Header("Game Settings")]
        [SerializeField] private bool startSpawningOnAwake = true;

        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public MaskDatabase MaskDatabase => maskDatabase;
        public InventoryManager Inventory => inventoryManager;
        public RewardSystem Rewards => rewardSystem;
        public AnimalSpawner Spawner => animalSpawner;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            InitializeSystems();

            if (startSpawningOnAwake && animalSpawner != null)
            {
                animalSpawner.StartSpawning();
            }

            Debug.Log("[GameManager] Mask Collect initialized");
        }

        private void InitializeSystems()
        {
            // Ensure inventory is loaded
            if (inventoryManager != null)
            {
                inventoryManager.LoadInventory();
            }

            // Subscribe to collection complete event
            if (rewardSystem != null)
            {
                rewardSystem.OnCollectionCompleted += HandleCollectionComplete;
            }
        }

        private void HandleCollectionComplete()
        {
            Debug.Log("[GameManager] Congratulations! All masks collected!");
            
            // TODO: Show completion celebration
            // Could trigger special animation, unlock bonus content, etc.
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            Time.timeScale = 0f;
            animalSpawner?.StopSpawning();
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            animalSpawner?.StartSpawning();
        }

        /// <summary>
        /// Reset game progress (for testing)
        /// </summary>
        [ContextMenu("Reset Progress")]
        public void ResetProgress()
        {
            inventoryManager?.ClearInventory();
            Debug.Log("[GameManager] Progress reset");
        }

        private void OnDestroy()
        {
            if (rewardSystem != null)
            {
                rewardSystem.OnCollectionCompleted -= HandleCollectionComplete;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Collect Random Mask")]
        private void DebugCollectRandom()
        {
            rewardSystem?.GiveRandomReward();
        }

        [ContextMenu("Debug - Spawn Animal")]
        private void DebugSpawnAnimal()
        {
            animalSpawner?.SpawnRandomAnimal();
        }
#endif
    }
}
