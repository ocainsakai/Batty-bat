using System;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MaskCollect.Data;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Handles reward distribution when animals are helped.
    /// Can give specific masks or random weighted masks.
    /// </summary>
    public class RewardSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskDatabase maskDatabase;
        [SerializeField] private InventoryManager inventory;

        [Header("Settings")]
        [SerializeField] private bool prioritizeUnownedMasks = true;
        [SerializeField] private float duplicateMaskChance = 0.1f; // 10% chance to get duplicate

        // Events
        public event Action<MaskData, bool> OnRewardGiven; // mask, isNew
        public event Action OnCollectionCompleted;

        private static RewardSystem _instance;
        public static RewardSystem Instance => _instance;

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
        }

        /// <summary>
        /// Give a specific mask as reward
        /// </summary>
        public void GiveReward(MaskData mask)
        {
            if (mask == null)
            {
                GiveRandomReward();
                return;
            }

            bool isNew = inventory.CollectMask(mask);
            OnRewardGiven?.Invoke(mask, isNew);

            if (inventory.IsCollectionComplete)
            {
                OnCollectionCompleted?.Invoke();
            }

            Debug.Log($"[RewardSystem] Gave mask: {mask.MaskName} (New: {isNew})");
        }

        /// <summary>
        /// Give a random mask based on weighted rarity
        /// </summary>
        public void GiveRandomReward()
        {
            if (maskDatabase == null)
            {
                Debug.LogError("[RewardSystem] MaskDatabase not assigned!");
                return;
            }

            MaskData mask = null;

            if (prioritizeUnownedMasks && !inventory.IsCollectionComplete)
            {
                // Try to give unowned mask first
                bool giveDuplicate = UnityEngine.Random.value < duplicateMaskChance;
                
                if (!giveDuplicate)
                {
                    // Get random unowned mask using database method
                    mask = maskDatabase.GetRandomMask(inventory.CollectedMaskIds);
                }
            }

            // Fallback to any random mask
            if (mask == null && maskDatabase.AllMasks.Count > 0)
            {
                mask = maskDatabase.AllMasks[UnityEngine.Random.Range(0, maskDatabase.AllMasks.Count)];
            }

            if (mask != null)
            {
                GiveReward(mask);
            }
        }

        /// <summary>
        /// Give reward with delay and optional animation
        /// </summary>
        public async UniTask GiveRewardDelayed(MaskData mask, float delay)
        {
            await UniTask.WaitForSeconds(delay);
            GiveReward(mask);
        }

        /// <summary>
        /// Give multiple rewards (for special events)
        /// </summary>
        public void GiveMultipleRewards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GiveRandomReward();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Give Random Reward")]
        private void DebugGiveRandom()
        {
            GiveRandomReward();
        }
#endif
    }
}
