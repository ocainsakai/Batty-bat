using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaskCollect.Data
{
    [CreateAssetMenu(fileName = "BiomeDatabase", menuName = "MaskCollect/Biome Database")]
    public class BiomeDatabase : ScriptableObject
    {
        [SerializeField] private List<BiomeData> allBiomes = new();

        public IReadOnlyList<BiomeData> AllBiomes => allBiomes;
        public int TotalBiomeCount => allBiomes.Count;

        private Dictionary<BiomeType, BiomeData> _biomeLookup;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _biomeLookup = new Dictionary<BiomeType, BiomeData>();
            foreach (var biome in allBiomes)
            {
                if (biome != null && !_biomeLookup.ContainsKey(biome.Type))
                {
                    _biomeLookup[biome.Type] = biome;
                }
            }
        }

        /// <summary>
        /// Get biome by type
        /// </summary>
        public BiomeData GetBiome(BiomeType type)
        {
            if (_biomeLookup == null) BuildLookup();
            return _biomeLookup.TryGetValue(type, out var biome) ? biome : null;
        }

        /// <summary>
        /// Get biome by ID
        /// </summary>
        public BiomeData GetBiomeById(string biomeId)
        {
            return allBiomes.FirstOrDefault(b => b != null && b.BiomeId == biomeId);
        }

        /// <summary>
        /// Get all biomes of a specific difficulty
        /// </summary>
        public List<BiomeData> GetBiomesByDifficulty(BiomeDifficulty difficulty)
        {
            return allBiomes.Where(b => b != null && b.Difficulty == difficulty).ToList();
        }

        /// <summary>
        /// Get the starting biome (first unlocked by default)
        /// </summary>
        public BiomeData GetStartingBiome()
        {
            return allBiomes.FirstOrDefault(b => b != null && b.IsUnlockedByDefault);
        }

        /// <summary>
        /// Find which biome contains a specific animal type
        /// </summary>
        public BiomeData FindBiomeForAnimal(string animalType)
        {
            return allBiomes.FirstOrDefault(b => b != null && b.HasAnimalType(animalType));
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-populate from folder")]
        private void AutoPopulate()
        {
            allBiomes.Clear();
            var guids = UnityEditor.AssetDatabase.FindAssets("t:BiomeData");
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var biome = UnityEditor.AssetDatabase.LoadAssetAtPath<BiomeData>(path);
                if (biome != null)
                {
                    allBiomes.Add(biome);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[BiomeDatabase] Populated {allBiomes.Count} biomes");
        }

        [ContextMenu("Sort by Difficulty")]
        private void SortByDifficulty()
        {
            allBiomes = allBiomes.OrderBy(b => b.Difficulty).ToList();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
