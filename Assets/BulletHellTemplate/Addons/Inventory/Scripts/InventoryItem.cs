using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents an inventory item that can be equipped by a character.
    /// </summary>
    [CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/InventoryItem", order = 52)]
    public class InventoryItem : ScriptableObject
    {

        /// <summary>
        /// The title of the inventory item.
        /// </summary>
        public string title;

        /// <summary>
        /// Array of translated titles by language.
        /// </summary>
        public NameTranslatedByLanguage[] titleTranslatedByLanguage;

        /// <summary>
        /// The description of the inventory item.
        /// </summary>
        [TextArea]
        public string description;

        /// <summary>
        /// Array of translated descriptions by language.
        /// </summary>
        public DescriptionTranslatedByLanguage[] descriptionTranslatedByLanguage;

        /// <summary>
        /// Unique identifier for the inventory item.
        /// </summary>
        public string itemId;

        /// <summary>
        /// Category of the inventory item.
        /// </summary>
        public string category;

        /// <summary>
        /// Icon representing the inventory item.
        /// </summary>
        public Sprite itemIcon;

        /// <summary>
        /// The character characterStatsComponent associated with this inventory item.
        /// </summary>
        public CharacterStats itemStats;

        /// <summary>
        /// List of upgrades available for this inventory item.
        /// </summary>
        public List<ItemUpgrade> itemUpgrades;

        /// <summary>
        /// Indicates whether the inventory item is unlocked.
        /// </summary>
        public bool isUnlocked;

        /// <summary>
        /// The rarity of the inventory item.
        /// </summary>
        public Rarity rarity;

        /// <summary>
        /// The slot type for this item (e.g., Armor, Weapon, etc.).
        /// </summary>
        [Tooltip("The slot type for this item (e.g., Armor, Weapon, etc.).")]
        public string slot;
    }

    /// <summary>
    /// Represents an upgrade for an inventory item.
    /// </summary>
    [System.Serializable]
    public class ItemUpgrade
    {
        /// <summary>
        /// The percentage increase in characterStatsComponent per level.
        /// </summary>
        public float statIncreasePercentagePerLevel;

        /// <summary>
        /// The currency tag used for the upgrade cost.
        /// </summary>
        public string currencyTag = "GO";

        /// <summary>
        /// The cost required for the upgrade.
        /// </summary>
        public int upgradeCosts;

        /// <summary>
        /// The success rate for the upgrade (between 0.1 and 1.0).
        /// </summary>
        [Range(0.1f, 1f)]
        public float successRate = 1.0f;

        /// <summary>
        /// If true, the level will decrease if the upgrade fails.
        /// </summary>
        public bool decreaseLevelIfFail = false;
    }
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
}
