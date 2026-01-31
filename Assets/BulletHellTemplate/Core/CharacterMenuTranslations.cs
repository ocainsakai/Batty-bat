using System;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Holds translations for all stat names and common messages.
    /// </summary>
    [Serializable]
    public class CharacterMenuTranslations
    {
        [Header("Stat Translations")]
        [Tooltip("Default HP text if no translation is available.")]
        public string defaultHp = "HP";
        [Tooltip("Array of translations for HP stat name.")]
        public NameTranslatedByLanguage[] hpTranslation;

        [Tooltip("Default HP Regen text if no translation is available.")]
        public string defaultHpRegen = "HP Regen";
        [Tooltip("Array of translations for HP Regen stat name.")]
        public NameTranslatedByLanguage[] hpRegenTranslation;

        [Tooltip("Default HP Leech text if no translation is available.")]
        public string defaultHpLeech = "HP Leech";
        [Tooltip("Array of translations for HP Leech stat name.")]
        public NameTranslatedByLanguage[] hpLeechTranslation;

        [Tooltip("Default MP text if no translation is available.")]
        public string defaultMp = "MP";
        [Tooltip("Array of translations for MP stat name.")]
        public NameTranslatedByLanguage[] mpTranslation;

        [Tooltip("Default MP Regen text if no translation is available.")]
        public string defaultMpRegen = "MP Regen";
        [Tooltip("Array of translations for MP Regen stat name.")]
        public NameTranslatedByLanguage[] mpRegenTranslation;

        [Tooltip("Default Damage text if no translation is available.")]
        public string defaultDamage = "Damage";
        [Tooltip("Array of translations for Damage stat name.")]
        public NameTranslatedByLanguage[] damageTranslation;

        [Tooltip("Default Attack Speed text if no translation is available.")]
        public string defaultAttackSpeed = "Attack Speed";
        [Tooltip("Array of translations for Attack Speed stat name.")]
        public NameTranslatedByLanguage[] attackSpeedTranslation;

        [Tooltip("Default Cooldown Reduction text if no translation is available.")]
        public string defaultCooldownReduction = "Cooldown Reduction";
        [Tooltip("Array of translations for Cooldown Reduction stat name.")]
        public NameTranslatedByLanguage[] cooldownReductionTranslation;

        [Tooltip("Default Critical Rate text if no translation is available.")]
        public string defaultCriticalRate = "Critical Rate";
        [Tooltip("Array of translations for Critical Rate stat name.")]
        public NameTranslatedByLanguage[] criticalRateTranslation;

        [Tooltip("Default Critical Damage Multiplier text if no translation is available.")]
        public string defaultCriticalDamageMultiplier = "Crit Damage Multiplier";
        [Tooltip("Array of translations for Critical Damage Multiplier stat name.")]
        public NameTranslatedByLanguage[] criticalDamageMultiplierTranslation;

        [Tooltip("Default Defense text if no translation is available.")]
        public string defaultDefense = "Defense";
        [Tooltip("Array of translations for Defense stat name.")]
        public NameTranslatedByLanguage[] defenseTranslation;

        [Tooltip("Default Shield text if no translation is available.")]
        public string defaultShield = "Shield";
        [Tooltip("Array of translations for Shield stat name.")]
        public NameTranslatedByLanguage[] shieldTranslation;

        [Tooltip("Default Move Speed text if no translation is available.")]
        public string defaultMoveSpeed = "Move Speed";
        [Tooltip("Array of translations for Move Speed stat name.")]
        public NameTranslatedByLanguage[] moveSpeedTranslation;

        [Tooltip("Default Collect Range text if no translation is available.")]
        public string defaultCollectRange = "Collect Range";
        [Tooltip("Array of translations for Collect Range stat name.")]
        public NameTranslatedByLanguage[] collectRangeTranslation;

        [Header("Common Messages")]
        [Tooltip("Error message for not enough EXP.")]
        public string errorNotEnoughExp = "Not enough EXP.";
        [Tooltip("Array of translations for error message when not enough EXP.")]
        public NameTranslatedByLanguage[] errorNotEnoughExpTranslated;

        [Tooltip("Error message for not enough currency.")]
        public string errorNotEnoughCurrency = "Not enough currency.";
        [Tooltip("Array of translations for error message when not enough currency.")]
        public NameTranslatedByLanguage[] errorNotEnoughCurrencyTranslated;

        [Tooltip("Error message for already at max level.")]
        public string errorAlreadyMaxLevel = "Already at max level.";
        [Tooltip("Array of translations for error message when already at max level.")]
        public NameTranslatedByLanguage[] errorAlreadyMaxLevelTranslated;

        [Tooltip("Success message for level up.")]
        public string successLevelUp = "Level Up successful!";
        [Tooltip("Array of translations for success message when level up is achieved.")]
        public NameTranslatedByLanguage[] successLevelUpTranslated;

        [Header("Rarity")]
        [Tooltip("Default rarity for Common items.")]
        public string CommonRarity = "Common";
        [Tooltip("Array of translations for Common rarity.")]
        public NameTranslatedByLanguage[] CommonRarityTranslated;

        [Tooltip("Default rarity for Uncommon items.")]
        public string UncommonRarity = "Uncommon";
        [Tooltip("Array of translations for Uncommon rarity.")]
        public NameTranslatedByLanguage[] UncommonRarityTranslated;

        [Tooltip("Default rarity for Rare items.")]
        public string RareRarity = "Rare";
        [Tooltip("Array of translations for Rare rarity.")]
        public NameTranslatedByLanguage[] RareRarityTranslated;

        [Tooltip("Default rarity for Epic items.")]
        public string EpicRarity = "Epic";
        [Tooltip("Array of translations for Epic rarity.")]
        public NameTranslatedByLanguage[] EpicRarityTranslated;

        [Tooltip("Default rarity for Legendary items.")]
        public string LegendaryRarity = "Legendary";
        [Tooltip("Array of translations for Legendary rarity.")]
        public NameTranslatedByLanguage[] LegendaryRarityTranslated;
    }
}
