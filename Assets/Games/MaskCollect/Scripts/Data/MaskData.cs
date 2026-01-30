using UnityEngine;

namespace MaskCollect.Data
{
    public enum MaskRarity
    {
        Common,      // 50% drop rate
        Uncommon,    // 30% drop rate
        Rare,        // 15% drop rate
        Legendary    // 5% drop rate
    }

    [CreateAssetMenu(fileName = "New Mask", menuName = "MaskCollect/Mask Data")]
    public class MaskData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string maskId;
        [SerializeField] private string maskName;
        [SerializeField, TextArea] private string description;

        [Header("Visuals")]
        [SerializeField] private Sprite maskSprite;
        [SerializeField] private Sprite silhouetteSprite; // Optional: for locked display

        [Header("Properties")]
        [SerializeField] private MaskRarity rarity = MaskRarity.Common;
        [SerializeField] private string associatedAnimal;
        [SerializeField] private BiomeType homeBiome = BiomeType.PeacefulMeadow;

        // Public accessors
        public string MaskId => string.IsNullOrEmpty(maskId) ? name : maskId;
        public string MaskName => maskName;
        public string Description => description;
        public Sprite MaskSprite => maskSprite;
        public Sprite SilhouetteSprite => silhouetteSprite;
        public MaskRarity Rarity => rarity;
        public string AssociatedAnimal => associatedAnimal;
        public BiomeType HomeBiome => homeBiome;

        /// <summary>
        /// Get drop weight based on rarity
        /// </summary>
        public float GetDropWeight()
        {
            return rarity switch
            {
                MaskRarity.Common => 50f,
                MaskRarity.Uncommon => 30f,
                MaskRarity.Rare => 15f,
                MaskRarity.Legendary => 5f,
                _ => 50f
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(maskId))
            {
                maskId = name.ToLower().Replace(" ", "_");
            }
        }
#endif
    }
}
