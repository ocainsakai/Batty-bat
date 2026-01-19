using UnityEngine;
using System;

namespace Core.Systems.CollectionFramework
{
    /// <summary>
    /// Base ScriptableObject for all collectible items.
    /// Can be extended for game-specific item types.
    /// </summary>
    [CreateAssetMenu(fileName = "Item", menuName = "Collection Framework/Item Definition", order = 1)]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this item")]
        public string ItemID;
        
        [Tooltip("Display name of the item")]
        public string ItemName;
        
        [Tooltip("Description of the item")]
        [TextArea(3, 5)]
        public string Description;

        [Header("Visuals")]
        [Tooltip("Icon for inventory/UI")]
        public Sprite Icon;
        
        [Tooltip("Sprite for world display")]
        public Sprite DisplaySprite;
        
        [Tooltip("3D prefab (optional)")]
        public GameObject Prefab;
        
        [Tooltip("Color tint")]
        public Color Color = Color.white;

        [Header("Classification")]
        [Tooltip("Category of this item")]
        public ItemCategory Category;
        
        [Tooltip("Rarity tier")]
        public ItemRarity Rarity = ItemRarity.Common;
        
        [Tooltip("Type of item")]
        public ItemType Type = ItemType.Collectible;

        [Header("Inventory Properties")]
        [Tooltip("Maximum stack size (1 = not stackable)")]
        [Range(1, 999)]
        public int MaxStackSize = 99;
        
        [Tooltip("Can this item be consumed/used?")]
        public bool IsConsumable = false;
        
        [Tooltip("Is this item unique (only one can exist)?")]
        public bool IsUnique = false;

        [Header("Value")]
        [Tooltip("Generic value (points, currency, etc.)")]
        public int Value = 1;
        
        [Tooltip("Sell price (if applicable)")]
        public int SellPrice = 0;

        [Header("Gallery")]
        [Tooltip("Show this item in gallery/collection book?")]
        public bool ShowInGallery = true;
        
        [Tooltip("Gallery-specific description")]
        [TextArea(2, 4)]
        public string GalleryDescription;
        
        [Tooltip("How this item is unlocked")]
        public UnlockCondition UnlockCondition;

        [Header("Effects (Optional)")]
        [Tooltip("Particle effect when collected")]
        public GameObject CollectionEffect;
        
        [Tooltip("Sound when collected")]
        public AudioClip CollectionSound;
    }

    [Serializable]
    public class ItemCategory
    {
        public string CategoryID;
        public string CategoryName;
        public Sprite CategoryIcon;
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    public enum ItemType
    {
        Collectible,    // Generic collectible
        Resource,       // Crafting resource
        Consumable,     // Usable item
        Equipment,      // Equippable item
        QuestItem,      // Quest-related
        Currency,       // Money/coins
        PowerUp,        // Temporary boost
        Custom          // Custom type
    }

    public enum UnlockCondition
    {
        None,           // Always available
        FirstCollection,// Unlock on first collect
        MultipleCollect,// Unlock after X collections
        Achievement,    // Unlock via achievement
        Quest,          // Unlock via quest
        Purchase,       // Unlock via purchase
        Custom          // Custom condition
    }
}
