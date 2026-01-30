using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MaskCollect.Data
{
    /// <summary>
    /// Central database holding references to all masks in the game.
    /// Used for UI rendering and collection tracking.
    /// </summary>
    [CreateAssetMenu(fileName = "MaskDatabase", menuName = "MaskCollect/Mask Database")]
    public class MaskDatabase : ScriptableObject
    {
        [SerializeField] private List<MaskData> _allMasks = new List<MaskData>();

        public IReadOnlyList<MaskData> AllMasks => _allMasks;
        public int TotalMaskCount => _allMasks.Count;

        /// <summary>
        /// Retrieves a mask by its unique ID.
        /// </summary>
        public MaskData GetMaskByID(string id)
        {
            return _allMasks.FirstOrDefault(m => m.MaskId == id);
        }

        /// <summary>
        /// Get all masks for a specific biome.
        /// </summary>
        public List<MaskData> GetMasksByBiome(BiomeType biome)
        {
            return _allMasks.Where(m => m.HomeBiome == biome).ToList();
        }

        /// <summary>
        /// Get a random mask from unowned masks.
        /// </summary>
        public MaskData GetRandomMask(HashSet<string> ownedMaskIds)
        {
            var unowned = _allMasks.Where(m => !ownedMaskIds.Contains(m.MaskId)).ToList();
            if (unowned.Count == 0) return null;
            return unowned[Random.Range(0, unowned.Count)];
        }

        /// <summary>
        /// Get a random mask weighted by rarity.
        /// </summary>
        public MaskData GetRandomWeightedMask(HashSet<string> ownedMaskIds)
        {
            var unowned = _allMasks.Where(m => !ownedMaskIds.Contains(m.MaskId)).ToList();
            if (unowned.Count == 0) return null;

            float totalWeight = unowned.Sum(m => m.GetDropWeight());
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var mask in unowned)
            {
                currentWeight += mask.GetDropWeight();
                if (randomValue <= currentWeight)
                    return mask;
            }

            return unowned[unowned.Count - 1];
        }

#if UNITY_EDITOR
        /// <summary>
        /// Helper for the Editor Tool to populate the database.
        /// </summary>
        public void SetMasks(List<MaskData> masks)
        {
            _allMasks = masks;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
