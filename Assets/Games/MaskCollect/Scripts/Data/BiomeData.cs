using UnityEngine;

namespace MaskCollect.Data
{
    public enum BiomeType
    {
        PeacefulMeadow,    // Peaceful Meadow
        MysticForest,      // Mystic Forest
        ZenValley,         // Zen Valley
        WhimsicalWetlands  // Whimsical Wetlands
    }

    public enum BiomeDifficulty
    {
        Easy,      // Common masks, starter area
        Medium,    // Uncommon masks
        Hard,      // Rare masks
        Secret     // Legendary masks
    }

    [CreateAssetMenu(fileName = "New Biome", menuName = "MaskCollect/Biome Data")]
    public class BiomeData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string biomeId;
        [SerializeField] private string biomeName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private BiomeType biomeType;
        [SerializeField] private BiomeDifficulty difficulty = BiomeDifficulty.Easy;

        [Header("Visuals")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite minimapIcon;
        [SerializeField] private Color primaryColor = Color.green;
        [SerializeField] private Color secondaryColor = Color.yellow;
        [SerializeField] private Color accentColor = Color.white;

        [Header("Environment")]
        [SerializeField] private GameObject biomePrefab; // The entire biome scene/area
        [SerializeField] private GameObject[] decorationPrefabs;
        [SerializeField] private GameObject[] obstaclePrefabs;

        [Header("Audio")]
        [SerializeField] private AudioClip ambientMusic;
        [SerializeField] private AudioClip[] ambientSounds;

        [Header("Animals in this Biome")]
        [SerializeField] private MaskData[] availableMasks;
        [SerializeField] private string[] animalTypes; // e.g., "dog", "cat", "rabbit", "bird"

        [Header("Unlock Conditions")]
        [SerializeField] private bool isUnlockedByDefault = false;
        [SerializeField] private int masksRequiredToUnlock = 0;
        [SerializeField] private BiomeData prerequisiteBiome;

        // Public accessors
        public string BiomeId => string.IsNullOrEmpty(biomeId) ? name : biomeId;
        public string BiomeName => biomeName;
        public string Description => description;
        public BiomeType Type => biomeType;
        public BiomeDifficulty Difficulty => difficulty;
        
        public Sprite BackgroundSprite => backgroundSprite;
        public Sprite MinimapIcon => minimapIcon;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public Color AccentColor => accentColor;
        
        public GameObject BiomePrefab => biomePrefab;
        public GameObject[] DecorationPrefabs => decorationPrefabs;
        public GameObject[] ObstaclePrefabs => obstaclePrefabs;
        
        public AudioClip AmbientMusic => ambientMusic;
        public AudioClip[] AmbientSounds => ambientSounds;
        
        public MaskData[] AvailableMasks => availableMasks;
        public string[] AnimalTypes => animalTypes;
        
        public bool IsUnlockedByDefault => isUnlockedByDefault;
        public int MasksRequiredToUnlock => masksRequiredToUnlock;
        public BiomeData PrerequisiteBiome => prerequisiteBiome;

        /// <summary>
        /// Check if a specific animal type exists in this biome
        /// </summary>
        public bool HasAnimalType(string animalType)
        {
            if (animalTypes == null) return false;
            
            foreach (var type in animalTypes)
            {
                if (type.Equals(animalType, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a random decoration prefab
        /// </summary>
        public GameObject GetRandomDecoration()
        {
            if (decorationPrefabs == null || decorationPrefabs.Length == 0) return null;
            return decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
        }

        /// <summary>
        /// Get a random ambient sound
        /// </summary>
        public AudioClip GetRandomAmbientSound()
        {
            if (ambientSounds == null || ambientSounds.Length == 0) return null;
            return ambientSounds[Random.Range(0, ambientSounds.Length)];
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(biomeId))
            {
                biomeId = biomeType.ToString().ToLower();
            }
            
            if (string.IsNullOrEmpty(biomeName))
            {
                biomeName = biomeType switch
                {
                    BiomeType.PeacefulMeadow => "Peaceful Meadow",
                    BiomeType.MysticForest => "Mystic Forest",
                    BiomeType.ZenValley => "Zen Valley",
                    BiomeType.WhimsicalWetlands => "Whimsical Wetlands",
                    _ => name
                };
            }
        }
#endif
    }
}
