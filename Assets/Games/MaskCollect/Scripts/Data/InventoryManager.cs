using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaskCollect.Data
{
    /// <summary>
    /// Manages the player's mask collection.
    /// Uses PlayerPrefs for simple persistence.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        private const string SAVE_KEY = "MaskCollect_Inventory";
        private const string SEPARATOR = "|";

        [SerializeField] private MaskDatabase maskDatabase;

        private HashSet<string> _collectedMaskIds = new();

        public event Action<MaskData> OnMaskCollected;
        public event Action OnInventoryLoaded;
        public event Action OnInventoryCleared;

        public int CollectedCount => _collectedMaskIds.Count;
        public int TotalMaskCount => maskDatabase != null ? maskDatabase.AllMasks.Count : 0;
        public bool IsCollectionComplete => CollectedCount >= TotalMaskCount;
        public HashSet<string> CollectedMaskIds => _collectedMaskIds;

        private static InventoryManager _instance;
        public static InventoryManager Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadInventory();
        }

        /// <summary>
        /// Check if a mask is already collected
        /// </summary>
        public bool HasMask(string maskId)
        {
            return _collectedMaskIds.Contains(maskId);
        }

        /// <summary>
        /// Check if a mask is already collected
        /// </summary>
        public bool HasMask(MaskData mask)
        {
            return mask != null && HasMask(mask.MaskId);
        }

        /// <summary>
        /// Add a mask to the collection
        /// </summary>
        /// <returns>True if the mask was newly added, false if already owned</returns>
        public bool CollectMask(MaskData mask)
        {
            if (mask == null) return false;

            if (_collectedMaskIds.Add(mask.MaskId))
            {
                SaveInventory();
                OnMaskCollected?.Invoke(mask);
                Debug.Log($"[InventoryManager] Collected new mask: {mask.MaskName}");
                return true;
            }

            Debug.Log($"[InventoryManager] Already have mask: {mask.MaskName}");
            return false;
        }

        /// <summary>
        /// Get all collected masks as MaskData objects
        /// </summary>
        public List<MaskData> GetCollectedMasks()
        {
            var result = new List<MaskData>();
            foreach (var maskId in _collectedMaskIds)
            {
                var mask = maskDatabase.GetMaskByID(maskId);
                if (mask != null)
                {
                    result.Add(mask);
                }
            }
            return result;
        }

        /// <summary>
        /// Get collection progress as percentage (0-1)
        /// </summary>
        public float GetCollectionProgress()
        {
            if (TotalMaskCount == 0) return 0f;
            return (float)CollectedCount / TotalMaskCount;
        }

        /// <summary>
        /// Save inventory to PlayerPrefs
        /// </summary>
        public void SaveInventory()
        {
            string saveData = string.Join(SEPARATOR, _collectedMaskIds);
            PlayerPrefs.SetString(SAVE_KEY, saveData);
            PlayerPrefs.Save();
            Debug.Log($"[InventoryManager] Saved {_collectedMaskIds.Count} masks");
        }

        /// <summary>
        /// Load inventory from PlayerPrefs
        /// </summary>
        public void LoadInventory()
        {
            _collectedMaskIds.Clear();

            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string saveData = PlayerPrefs.GetString(SAVE_KEY);
                if (!string.IsNullOrEmpty(saveData))
                {
                    var ids = saveData.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in ids)
                    {
                        _collectedMaskIds.Add(id);
                    }
                }
            }

            OnInventoryLoaded?.Invoke();
            Debug.Log($"[InventoryManager] Loaded {_collectedMaskIds.Count} masks");
        }

        /// <summary>
        /// Clear all collected masks (for testing/reset)
        /// </summary>
        [ContextMenu("Clear Inventory")]
        public void ClearInventory()
        {
            _collectedMaskIds.Clear();
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
            OnInventoryCleared?.Invoke();
            Debug.Log("[InventoryManager] Inventory cleared");
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Collect All Masks")]
        private void DebugCollectAll()
        {
            if (maskDatabase == null) return;
            foreach (var mask in maskDatabase.AllMasks)
            {
                CollectMask(mask);
            }
        }

        [ContextMenu("Debug - Collect Random Mask")]
        private void DebugCollectRandom()
        {
            if (maskDatabase == null) return;
            var unowned = maskDatabase.AllMasks.FirstOrDefault(m => !_collectedMaskIds.Contains(m.MaskId));
            if (unowned != null)
            {
                CollectMask(unowned);
            }
            else
            {
                Debug.Log("[InventoryManager] All masks already collected!");
            }
        }
#endif
    }
}
