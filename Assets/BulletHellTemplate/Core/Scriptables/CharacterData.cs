using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents character data for use in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterData", menuName = "Character Data", order = 51)]
    public class CharacterData : ScriptableObject
    {
        [Tooltip("The name of the character")]
        public string characterName;

        public NameTranslatedByLanguage[] characterNameTranslated;

        [TextArea]
        public string characterDescription;

        public DescriptionTranslatedByLanguage[] characterDescriptionTranslated;

        [Tooltip("Unique identifier for the character")]
        public int characterId;

        [Tooltip("Reference to the character type data")]
        public CharacterTypeData characterType;

        [Tooltip("Icon sprite for the character")]
        public Sprite icon;

        [Tooltip("Icon sprite representing the character tier")]
        public Sprite tierIcon;

        [Tooltip("The class type of the character")]
        public string characterClassType;

        public NameTranslatedByLanguage[] characterClassTranslated;

        [Tooltip("Icon representing the character class")]
        public Sprite characterClassIcon;

        [Tooltip("Rarity of the character")]
        public CharacterRarity characterRarity;

        [Header("Default Model")]
        [Tooltip("Default model for the character")]
        public CharacterModel characterModel;

        [Header("Skins Models")]
        [Tooltip("Array of skin models for the character")]
        public CharacterSkin[] characterSkins;

        [Header("Basic Attack Skill")]
        [Tooltip("Basic attack skill data")]
        public SkillData autoAttack;

        [Header("Character Skills")]
        [Tooltip("Array of character skill data")]
        public SkillData[] skills;

        [Header("Character Item Slots")]
        [Tooltip("Array of item slot names for the character")]
        public string[] itemSlots;

        public string[] runeSlots;

        [Header("Character already unlocked")]
        [SerializeField]
        private bool isUnlock;

        [Header("Character Base Stats")]
        [Tooltip("Base characterStatsComponent of the character")]
        public CharacterStats baseStats;

        [Header("Experience Settings")]
        [Tooltip("Maximum level of the character")]
        public int maxLevel;

        [Tooltip("Percentage increase of base characterStatsComponent per level")]
        public float statsPercentageIncreaseByLevel;

        [Tooltip("Method used to calculate experience points required per level")]
        public ExpCalculationMethod expCalculationMethod;

        [Tooltip("Initial experience points required for level 1")]
        public int initialExp;

        [Tooltip("Experience points required for the final level")]
        public int finalExp;

        [Tooltip("Experience points required per level")]
        public int[] expPerLevel;

        [Header("Upgrade Cost Settings")]
        public string currencyId = "GO";
        [Tooltip("Upgrade cost per level")]
        public int[] upgradeCostPerLevel;

        [Tooltip("Initial upgrade cost for level 1")]
        public int initialUpgradeCost;

        [Tooltip("Upgrade cost for the final level")]
        public int finalUpgradeCost;

        [Tooltip("Method used to calculate upgrade cost per level")]
        public ExpCalculationMethod upgradeCostCalculationMethod;

        [Header("Character Upgrades")]
        [Tooltip("List of stat upgrades available for the character")]
        public List<StatUpgrade> statUpgrades;

        /// <summary>
        /// Gets a value indicating whether the character is unlocked.
        /// </summary>
        public bool CheckUnlocked
        {
            get { return isUnlock; }
        }

        /// <summary>
        /// Gets the icon associated with the character type.
        /// </summary>
        /// <returns>The sprite representing the character type icon.</returns>
        public Sprite GetCharacterTypeIcon()
        {
            return characterType.icon;
        }
    }

    /// <summary>
    /// Enum representing the different character rarities.
    /// </summary>
    public enum CharacterRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Represents a stat upgrade with its associated data.
    /// </summary>
    [System.Serializable]
    public class StatUpgrade
    {
        [Tooltip("The name of the upgrade")]
        public string upgradeName;

        public NameTranslatedByLanguage[] upgradeNameTranslated;

        [Tooltip("The stat type that will be upgraded")]
        public StatType statType;

        [Tooltip("Maximum level for the upgrade")]
        public int upgradeMaxLevel = 5;

        [Tooltip("Array holding the upgrade amounts for each level")]
        public float[] upgradeAmounts = new float[5];

        [Tooltip("Array holding the upgrade costs for each level")]
        public int[] upgradeCosts = new int[5];

        [Tooltip("Icon representing the upgrade")]
        public Sprite upgradeIcon;

        [Tooltip("Currency tag associated with the upgrade cost")]
        public string currencyTag = "GO";

        /// <summary>
        /// Applies the upgrade to the specified character characterStatsComponent for the given level.
        /// </summary>
        /// <param name="stats">The runtime character characterStatsComponent to apply the upgrade to.</param>
        /// <param name="level">The level of the upgrade.</param>
        public void ApplyUpgrade(CharacterStatsRuntime stats, int level)
        {
            if (level < 1 || level > upgradeMaxLevel)
            {
                Debug.LogError("Invalid upgrade level.");
                return;
            }

            float upgradeAmount = upgradeAmounts[level - 1];

            switch (statType)
            {
                case StatType.HP:
                    stats.baseHP += upgradeAmount;
                    break;
                case StatType.HPRegen:
                    stats.baseHPRegen += upgradeAmount;
                    break;
                case StatType.HPLeech:
                    stats.baseHPLeech += upgradeAmount;
                    break;
                case StatType.MP:
                    stats.baseMP += upgradeAmount;
                    break;
                case StatType.MPRegen:
                    stats.baseMPRegen += upgradeAmount;
                    break;
                case StatType.Damage:
                    stats.baseDamage += upgradeAmount;
                    break;
                case StatType.AttackSpeed:
                    stats.baseAttackSpeed += upgradeAmount;
                    break;
                case StatType.CooldownReduction:
                    stats.baseCooldownReduction += upgradeAmount;
                    break;
                case StatType.CriticalRate:
                    stats.baseCriticalRate += upgradeAmount;
                    break;
                case StatType.CriticalDamageMultiplier:
                    stats.baseCriticalDamageMultiplier += upgradeAmount;
                    break;
                case StatType.Defense:
                    stats.baseDefense += upgradeAmount;
                    break;
                case StatType.Shield:
                    stats.baseShield += upgradeAmount;
                    break;
                case StatType.MoveSpeed:
                    stats.baseMoveSpeed += upgradeAmount;
                    break;
                case StatType.CollectRange:
                    stats.baseCollectRange += upgradeAmount;
                    break;
                case StatType.MaxStats:
                    stats.baseMaxStats += upgradeAmount;
                    break;
                case StatType.MaxSkills:
                    stats.baseMaxSkills += upgradeAmount;
                    break;
            }
        }

        /// <summary>
        /// Gets the cost of the upgrade for the specified level.
        /// </summary>
        /// <param name="level">The level of the upgrade.</param>
        /// <returns>The cost of the upgrade at the given level.</returns>
        public int GetUpgradeCost(int level)
        {
            if (level < 1 || level > upgradeMaxLevel)
            {
                Debug.LogError("Invalid upgrade level.");
                return 0;
            }

            return upgradeCosts[level - 1];
        }
    }
    [System.Serializable]
    public struct CharacterSkin
    {
        public Sprite skinIcon;
        public string skinName;
        public NameTranslatedByLanguage[] skinNameTranslated;
        public CharacterModel skinCharacterModel;
        public bool isUnlocked;
        public string unlockCurrencyId;
        public int unlockPrice;
    }
}
