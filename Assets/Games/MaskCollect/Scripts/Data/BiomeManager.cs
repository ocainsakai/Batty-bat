using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MaskCollect.Data
{
    /// <summary>
    /// Manages biome states, transitions, and unlock progress.
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BiomeDatabase biomeDatabase;
        [SerializeField] private InventoryManager inventory;

        [Header("Current State")]
        [SerializeField] private BiomeData currentBiome;

        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 1f;

        private HashSet<BiomeType> _unlockedBiomes = new();

        // Events
        public event Action<BiomeData> OnBiomeEntered;
        public event Action<BiomeData> OnBiomeExited;
        public event Action<BiomeData> OnBiomeUnlocked;

        public BiomeData CurrentBiome => currentBiome;
        public IReadOnlyCollection<BiomeType> UnlockedBiomes => _unlockedBiomes;

        private static BiomeManager _instance;
        public static BiomeManager Instance => _instance;

        private const string SAVE_KEY = "MaskCollect_UnlockedBiomes";

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
            if (inventory == null)
            {
                inventory = InventoryManager.Instance;
            }

            LoadUnlockedBiomes();
            CheckForNewUnlocks();

            // Subscribe to mask collection to check for unlocks
            if (inventory != null)
            {
                inventory.OnMaskCollected += HandleMaskCollected;
            }

            // Enter starting biome if none set
            if (currentBiome == null && biomeDatabase != null)
            {
                var startingBiome = biomeDatabase.GetStartingBiome();
                if (startingBiome != null)
                {
                    _ = EnterBiome(startingBiome);
                }
            }
        }

        private void OnDestroy()
        {
            if (inventory != null)
            {
                inventory.OnMaskCollected -= HandleMaskCollected;
            }
        }

        #region Biome Navigation

        /// <summary>
        /// Enter a specific biome
        /// </summary>
        public async UniTask EnterBiome(BiomeData biome)
        {
            if (biome == null) return;

            if (!IsBiomeUnlocked(biome.Type))
            {
                Debug.LogWarning($"[BiomeManager] Biome {biome.BiomeName} is locked!");
                return;
            }

            // Exit current biome
            if (currentBiome != null)
            {
                OnBiomeExited?.Invoke(currentBiome);
            }

            // Transition effect could go here
            await UniTask.WaitForSeconds(transitionDuration);

            // Enter new biome
            currentBiome = biome;
            OnBiomeEntered?.Invoke(biome);

            Debug.Log($"[BiomeManager] Entered biome: {biome.BiomeName}");
        }

        /// <summary>
        /// Enter biome by type
        /// </summary>
        public async UniTask EnterBiome(BiomeType type)
        {
            var biome = biomeDatabase?.GetBiome(type);
            if (biome != null)
            {
                await EnterBiome(biome);
            }
        }

        #endregion

        #region Unlock System

        /// <summary>
        /// Check if a biome is unlocked
        /// </summary>
        public bool IsBiomeUnlocked(BiomeType type)
        {
            return _unlockedBiomes.Contains(type);
        }

        /// <summary>
        /// Check if a biome can be unlocked based on current progress
        /// </summary>
        public bool CanUnlockBiome(BiomeData biome)
        {
            if (biome == null) return false;
            if (biome.IsUnlockedByDefault) return true;
            if (IsBiomeUnlocked(biome.Type)) return true;

            // Check prerequisite biome
            if (biome.PrerequisiteBiome != null && !IsBiomeUnlocked(biome.PrerequisiteBiome.Type))
            {
                return false;
            }

            // Check mask count requirement
            if (inventory != null && inventory.CollectedCount >= biome.MasksRequiredToUnlock)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to unlock a biome
        /// </summary>
        public bool TryUnlockBiome(BiomeData biome)
        {
            if (biome == null) return false;
            if (IsBiomeUnlocked(biome.Type)) return true;

            if (CanUnlockBiome(biome))
            {
                _unlockedBiomes.Add(biome.Type);
                SaveUnlockedBiomes();
                OnBiomeUnlocked?.Invoke(biome);
                Debug.Log($"[BiomeManager] Unlocked biome: {biome.BiomeName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get unlock progress for a biome (0-1)
        /// </summary>
        public float GetUnlockProgress(BiomeData biome)
        {
            if (biome == null || biome.MasksRequiredToUnlock <= 0) return 1f;
            if (inventory == null) return 0f;
            return Mathf.Clamp01((float)inventory.CollectedCount / biome.MasksRequiredToUnlock);
        }

        private void HandleMaskCollected(MaskData mask)
        {
            CheckForNewUnlocks();
        }

        private void CheckForNewUnlocks()
        {
            if (biomeDatabase == null) return;

            foreach (var biome in biomeDatabase.AllBiomes)
            {
                if (biome != null && !IsBiomeUnlocked(biome.Type))
                {
                    TryUnlockBiome(biome);
                }
            }
        }

        #endregion

        #region Persistence

        private void SaveUnlockedBiomes()
        {
            var biomeList = new List<int>();
            foreach (var biome in _unlockedBiomes)
            {
                biomeList.Add((int)biome);
            }
            
            string saveData = string.Join(",", biomeList);
            PlayerPrefs.SetString(SAVE_KEY, saveData);
            PlayerPrefs.Save();
        }

        private void LoadUnlockedBiomes()
        {
            _unlockedBiomes.Clear();

            // Always unlock default biomes
            if (biomeDatabase != null)
            {
                foreach (var biome in biomeDatabase.AllBiomes)
                {
                    if (biome != null && biome.IsUnlockedByDefault)
                    {
                        _unlockedBiomes.Add(biome.Type);
                    }
                }
            }

            // Load saved unlocks
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string saveData = PlayerPrefs.GetString(SAVE_KEY);
                if (!string.IsNullOrEmpty(saveData))
                {
                    var parts = saveData.Split(',');
                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int biomeInt))
                        {
                            _unlockedBiomes.Add((BiomeType)biomeInt);
                        }
                    }
                }
            }

            Debug.Log($"[BiomeManager] Loaded {_unlockedBiomes.Count} unlocked biomes");
        }

        [ContextMenu("Clear Unlock Progress")]
        public void ClearUnlockProgress()
        {
            _unlockedBiomes.Clear();
            PlayerPrefs.DeleteKey(SAVE_KEY);
            LoadUnlockedBiomes(); // Reload defaults
        }

        #endregion

        #region Utility

        /// <summary>
        /// Get list of all biomes with their lock status
        /// </summary>
        public List<(BiomeData biome, bool isUnlocked, float progress)> GetAllBiomesWithStatus()
        {
            var result = new List<(BiomeData, bool, float)>();
            
            if (biomeDatabase == null) return result;

            foreach (var biome in biomeDatabase.AllBiomes)
            {
                if (biome != null)
                {
                    bool unlocked = IsBiomeUnlocked(biome.Type);
                    float progress = unlocked ? 1f : GetUnlockProgress(biome);
                    result.Add((biome, unlocked, progress));
                }
            }

            return result;
        }

        /// <summary>
        /// Get animals available in current biome
        /// </summary>
        public string[] GetCurrentBiomeAnimals()
        {
            return currentBiome?.AnimalTypes ?? System.Array.Empty<string>();
        }

        /// <summary>
        /// Get masks available in current biome
        /// </summary>
        public MaskData[] GetCurrentBiomeMasks()
        {
            return currentBiome?.AvailableMasks ?? System.Array.Empty<MaskData>();
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Debug - Unlock All Biomes")]
        private void DebugUnlockAll()
        {
            if (biomeDatabase == null) return;
            foreach (var biome in biomeDatabase.AllBiomes)
            {
                if (biome != null)
                {
                    _unlockedBiomes.Add(biome.Type);
                }
            }
            SaveUnlockedBiomes();
            Debug.Log("[BiomeManager] All biomes unlocked");
        }
#endif
    }
}
