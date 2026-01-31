using BulletHellTemplate.PVP;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages game data and settings.
    /// </summary>
    public class GameInstance : MonoBehaviour
    {
        [Tooltip("Array of currency GameData.")]
        public List<Currency> currencyData = new List<Currency>();

        [Tooltip("Array of character data scriptable objects.")]
        public CharacterData[] characterData;

        [Tooltip("Array of icon items.")]
        public IconItem[] iconItems;

        [Tooltip("Array of frame items.")]
        public FrameItem[] frameItems;

        [Tooltip("Array of inventory items.")]
        public InventoryItem[] inventoryItems;

        [Tooltip("List of all character types.")]
        public CharacterTypeData[] characterTypes;

        [Tooltip("Array of all map info data in the game.")]
        public MapInfoData[] mapInfoData;

        [Tooltip("Array of quest items.")]
        public QuestItem[] questData;

        [Tooltip("Array of coupon items.")]
        public CouponItem[] couponData;

        [Tooltip("Array of shop items.")]
        public ShopItem[] shopData;

        [Tooltip("Array of battle pass items.")]
        public BattlePassItem[] battlePassData;

        [Tooltip("Rewards for new players, typically one per day.")]
        public RewardItem[] newPlayerRewardItems;

        [Tooltip("All daily reward items for each day.")]
        public RewardItem[] dailyRewardItems;

        [Header("PVP")]
        public PvpModeData[] pvpModes;

        [Header("Password Length Rules")]
        [Tooltip("Minimum number of characters required in the password.")]
        public int minPasswordLength = 8;

        [Tooltip("Maximum number of characters allowed in the password.")]
        public int maxPasswordLength = 20;

        [Header("Password Complexity Requirements")]
        [Tooltip("Whether at least one uppercase letter is required.")]
        public bool requireUppercase = false;

        [Tooltip("Whether at least one lowercase letter is required.")]
        public bool requireLowercase = false;

        [Tooltip("Whether at least one numeric digit is required.")]
        public bool requireNumbers = false;

        [Tooltip("Whether at least one special character (!@#...) is required.")]
        public bool requireSpecial = false;

        [Header("Controller Reference")]
        [Tooltip("Reference to the main character entity.")]
        public CharacterEntity characterEntity;

        [Header("Account Levels")]
        [Tooltip("Information about account levels.")]
        public AccountLevel accountLevels;

        [Header("Character Mastery")]
        [Tooltip("Settings for character mastery.")]
        public CharacterMastery characterMastery;

        [Header("Mastery Levels")]
        [Tooltip("Array of mastery level details.")]
        public CharacterMasteryLevel[] masteryLevels;

        [Header("Elements Settings")]
        [Tooltip("Percentage increase in damage when advantage element.")]
        public float advantageDamageIncrease = 0.2f; 

        [Tooltip("Percentage reduction in damage when weakness element.")]
        public float weaknessDamageReduction = 0.2f;

        [Header("BattlePass earn EXP for winning?")]
        [Tooltip("Battle Pass EXP awarded for winning a match")]
        public int BattlePassEXP = 210;

        public int battlePassDurationDays = 60;

        [Header("Map Name MainMenu")]
        [Tooltip("Name of the main menu scene")]
        public string mainMenuScene = "Home";

        [Header("Currency earned in matches")]
        [Tooltip("Currency used in the game")]
        public string goldCurrency = "GO";

        [Header("Battle Pass Settings")]
        public int maxLevelPass = 100; // Maximum level for the Battle Pass
        public float baseExpPass = 1000;
        public int SeasonLengthInDays = 30;
        [Header("0.1 = 10% more at each level // 1 = 100% more")]
        public float incPerLevelPass = 0.1f;
        public string battlePassCurrencyID = "DM"; // Currency ID for purchasing the Battle Pass
        public int battlePassPrice = 1000; // Price of the Battle Pass

        [Tooltip("Currency ID used for name change.")]
        public string changeNameTick = "TKN";

        [Tooltip("Minimum length for the player's name.")]
        public int minNameLength = 3;

        [Tooltip("Maximum length for the player's name.")]
        public int maxNameLength = 20;

        [Tooltip("Number of tickets required to change the player's name.")]
        public int ticketsToChange = 1;

        [Tooltip("Indicates if a ticket is required for name change.")]
        public bool needTicket;

        [Header("Platform Selection")]
        [Tooltip("Specifies the platform type to determine the correct UIGameplay.")]
        public PlatformType platformType;

        public WorldType worldType = WorldType.World3D;

        [Header("Prefab References")]
        [Tooltip("Reference to the UIGameplay for mobile platform.")]
        public UIGameplay mobileGameplay;

        [Tooltip("Reference to the UIGameplay for desktop platform.")]
        public UIGameplay desktopGameplay;

        public static GameInstance Singleton;
        private bool isInitialized;

        /// <summary>
        /// Initializes the GameInstance as a singleton.
        /// </summary>
        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
                isInitialized = true;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Checks if the GameInstance has been initialized.
        /// </summary>
        /// <returns>True if initialized; otherwise, false.</returns>
        public bool IsInitialized()
        {
            return isInitialized;
        }

        /// <summary>
        /// Gets the CharacterData for the currently selected character.
        /// </summary>
        /// <returns>The matching CharacterData, or null if not found.</returns>
        public CharacterData GetCharacterData()
        {
            int selectedId = PlayerSave.GetSelectedCharacter();
            var character = characterData.FirstOrDefault(c => c.characterId == selectedId);
            if (character == null)
                Debug.LogWarning($"CharacterData with ID '{selectedId}' not found.");
            return character;
        }

        /// <summary>
        /// Returns the character data that matches the given ID.
        /// </summary>
        public CharacterData GetCharacterDataById(int charId) =>
            characterData.FirstOrDefault(c => c.characterId == charId);

        /// <summary>
        /// Returns the inventory item that matches the given ID.
        /// </summary>
        public InventoryItem GetInventoryItemById(string id) =>
            inventoryItems.FirstOrDefault(i => i.itemId == id);

        /// <summary>
        /// Returns the quest item that matches the given ID.
        /// </summary>
        public QuestItem GetQuestItemById(int id) =>
            questData.FirstOrDefault(q => q.questId == id);

        /// <summary>
        /// Returns the IconItem that matches the given ID.
        /// </summary>
        public IconItem GetIconItemById(string id) =>
            iconItems.FirstOrDefault(i => i.iconId == id);

        /// <summary>
        /// Returns the FrameItem that matches the given ID.
        /// </summary>
        public FrameItem GetFrameItemById(string id) =>
            frameItems.FirstOrDefault(f => f.frameId == id);

        /// <summary>
        /// Returns the CouponItem that matches the given ID.
        /// </summary>
        public CouponItem GetCouponItemById(string id) =>
            couponData.FirstOrDefault(c => c.idCoupon == id);

        /// <summary>
        /// Returns the MapInfoData that matches the given ID.
        /// </summary>
        public MapInfoData GetMapInfoDataById(int mapId) =>
            mapInfoData.FirstOrDefault(m => m.mapId == mapId);

        public PvpModeData GetPvpModeByKey(string key)
        {
            foreach (var m in pvpModes)
                if (m && m.GetModeKey() == key) return m;
            return null;
        }

        /// <summary>
        /// Gets the icon for a given character type.
        /// </summary>
        /// <param name="characterType">The character type data.</param>
        /// <returns>The corresponding icon sprite.</returns>
        public Sprite GetIconForCharacterType(CharacterTypeData characterType) => 
            characterType.icon;
       
        /// <summary>
        /// Gets the required account experience for a specific level.
        /// </summary>
        /// <param name="level">The account level (1-based index).</param>
        /// <returns>The required experience, or -1 if level is invalid.</returns>
        public int GetAccountExpForLevel(int level)
        {
            if (accountLevels.accountExpPerLevel != null && level >= 1 && level <= accountLevels.accountExpPerLevel.Length)
                return accountLevels.accountExpPerLevel[level - 1];

            Debug.LogWarning("Invalid account level requested.");
            return -1;
        }

        /// <summary>
        /// Gets the required mastery experience for a specific mastery level.
        /// </summary>
        /// <param name="level">The mastery level (0-based index).</param>
        /// <returns>The required experience, or -1 if level is invalid.</returns>
        public int GetMasteryExpForLevel(int level)
        {
            if (characterMastery.masteryExpPerLevel != null && level >= 0 && level < characterMastery.masteryExpPerLevel.Length)
                return characterMastery.masteryExpPerLevel[level];

            Debug.LogWarning("Invalid mastery level requested.");
            return 0;
        }

        /// <summary>
        /// Gets the mastery level details for a specific level.
        /// </summary>
        /// <param name="level">The mastery level (1-based index).</param>
        /// <returns>The mastery level details, or a default value if level is invalid.</returns>
        public CharacterMasteryLevel GetMasteryLevel(int level)
        {
            if (masteryLevels != null && level >= 0 && level < masteryLevels.Length)
            {
                return masteryLevels[level];
            }
            Debug.LogWarning("Invalid mastery level requested.");
            return masteryLevels[masteryLevels.Length - 1];
        }

        /// <summary>
        /// Gets the UIGameplay instance based on the current platform.
        /// </summary>
        /// <returns>The corresponding UIGameplay instance for PC or Mobile.</returns>
        public UIGameplay GetUIGameplayForPlatform()
        {
            switch (platformType)
            {
                case PlatformType.PC:
                    return desktopGameplay;
                case PlatformType.Mobile:
                    return mobileGameplay;
                default:
                    Debug.LogWarning("Unsupported platform type specified.");
                    return null;
            }
        }      

        /// <summary>
        /// Gets the current platform type.
        /// </summary>
        /// <returns>The platform type.</returns>
        public PlatformType GetCurrentPlatform() => platformType;

        public WorldType GetCurrentWorldType() => worldType;
       
        /// <summary>
        /// Automatically fills the mastery experience array based on the calculation method set in 'characterMastery.characterExpCalculationMethod'.
        /// </summary>
        public void AutoFillMasteryExp()
        {
            if (characterMastery.Equals(null))
            {
                Debug.LogWarning("CharacterMastery is not assigned.");
                return;
            }

            ExpCalculationMethod method = characterMastery.characterExpCalculationMethod;
            int maxLevel = characterMastery.maxMasteryLevel;
            characterMastery.masteryExpPerLevel = new int[maxLevel];

            for (int i = 0; i < maxLevel; i++)
            {
                int value = 0;
                float t = (maxLevel > 1) ? (float)i / (maxLevel - 1) : 0f;

                switch (method)
                {
                    case ExpCalculationMethod.Linear:
                        value = Mathf.RoundToInt(Mathf.Lerp(characterMastery.initialExp, characterMastery.finalExp, t));
                        break;
                    case ExpCalculationMethod.Exponential:
                        if (characterMastery.initialExp <= 0) characterMastery.initialExp = 1;
                        value = Mathf.RoundToInt(characterMastery.initialExp * Mathf.Pow((float)characterMastery.finalExp / characterMastery.initialExp, t));
                        break;
                    case ExpCalculationMethod.Custom:
                    default:
                        value = Mathf.RoundToInt(Mathf.Lerp(characterMastery.initialExp, characterMastery.finalExp, t));
                        break;
                }

                characterMastery.masteryExpPerLevel[i] = value;
            }
        }


        /// <summary>
        /// Calculates the total damage with elemental advantages and disadvantages.
        /// </summary>
        /// <param name="attackerElement">The element of the attacker.</param>
        /// <param name="targetElement">The element of the target.</param>
        /// <param name="totalDamageWithoutElements">The base damage before elemental adjustments.</param>
        /// <returns>The final damage after applying elemental adjustments.</returns>
        public float TotalDamageWithElements(CharacterTypeData attackerElement, CharacterTypeData targetElement, float baseDamage)
        {
            if (attackerElement == null || targetElement == null)
                return baseDamage;

            bool advantage =
                Array.Exists(attackerElement.strengths, e => e == targetElement) ||
                Array.Exists(targetElement.weaknesses, e => e == attackerElement);

            bool disadvantage =
                Array.Exists(attackerElement.weaknesses, e => e == targetElement) ||
                Array.Exists(targetElement.strengths, e => e == attackerElement);

            if (advantage && !disadvantage)
                return baseDamage * (1f + advantageDamageIncrease);

            if (disadvantage && !advantage)
                return baseDamage * (1f - weaknessDamageReduction);

            return baseDamage;       
        }

        /// <summary>
        /// Automatically fills the account experience array based on the calculation method set in 'accountLevels.accountExpCalculationMethod'.
        /// </summary>
        public void AutoFillAccountExp()
        {
            if (accountLevels.Equals(null))
            {
                Debug.LogWarning("AccountLevel is not assigned.");
                return;
            }

            ExpCalculationMethod method = accountLevels.accountExpCalculationMethod;
            int maxLevel = accountLevels.accountMaxLevel;
            accountLevels.accountExpPerLevel = new int[maxLevel];

            for (int i = 0; i < maxLevel; i++)
            {
                int value = 0;
                float t = (maxLevel > 1) ? (float)i / (maxLevel - 1) : 0f;

                switch (method)
                {
                    case ExpCalculationMethod.Linear:
                        value = Mathf.RoundToInt(Mathf.Lerp(accountLevels.initialExp, accountLevels.finalExp, t));
                        break;
                    case ExpCalculationMethod.Exponential:
                        if (accountLevels.initialExp <= 0) accountLevels.initialExp = 1;
                        value = Mathf.RoundToInt(accountLevels.initialExp * Mathf.Pow((float)accountLevels.finalExp / accountLevels.initialExp, t));
                        break;
                    case ExpCalculationMethod.Custom:
                    default:
                        value = Mathf.RoundToInt(Mathf.Lerp(accountLevels.initialExp, accountLevels.finalExp, t));
                        break;
                }

                accountLevels.accountExpPerLevel[i] = value;
            }
        }
    }

    /// <summary>
    /// Represents the available platform types.
    /// </summary>
    public enum PlatformType
    {
        Mobile,
        PC
    }

    [System.Serializable]
    public struct AccountLevel
    {
        [Tooltip("Maximum account level.")]
        public int accountMaxLevel;

        [Tooltip("Initial EXP for account leveling.")]
        public int initialExp;

        [Tooltip("Final EXP for the highest account level.")]
        public int finalExp;

        [Tooltip("Selected calculation method for account EXP progression.")]
        public ExpCalculationMethod accountExpCalculationMethod;

        [Tooltip("EXP required per account level.")]
        public int[] accountExpPerLevel;
    }

    /// <summary>
    /// Represents the character mastery settings.
    /// </summary>
    [System.Serializable]
    public struct CharacterMastery
    {
        [Tooltip("Maximum mastery level.")]
        public int maxMasteryLevel;

        [Tooltip("Initial EXP for mastery leveling.")]
        public int initialExp;

        [Tooltip("Final EXP for the highest mastery level.")]
        public int finalExp;

        [Tooltip("Selected calculation method for mastery EXP progression.")]
        public ExpCalculationMethod characterExpCalculationMethod;

        [Tooltip("Experience points required per mastery level.")]
        public int[] masteryExpPerLevel;
    }

    /// <summary>
    /// Represents the details for a specific mastery level.
    /// </summary>
    [System.Serializable]
    public struct CharacterMasteryLevel
    {
        [Tooltip("Name of the mastery level.")]
        public string masteryName;

        [Tooltip("Translated name of the mastery level.")]
        public NameTranslatedByLanguage[] masteryNameTranslated;

        [Tooltip("Icon representing the mastery level.")]
        public Sprite masteryIcon;
    }

    /// <summary>
    /// Contains translation data for a name by language.
    /// </summary>
    [System.Serializable]
    public struct NameTranslatedByLanguage
    {
        public string LanguageId;
        public string Translate;
    }

    /// <summary>
    /// Contains translation data for a description by language.
    /// </summary>
    [System.Serializable]
    public struct DescriptionTranslatedByLanguage
    {
        public string LanguageId;

        [TextArea]
        public string Translate;
    }
    /// <summary>
    /// Methods for calculating experience progression.
    /// </summary>
    [System.Serializable]
    public enum ExpCalculationMethod
    {
        Linear,
        Exponential,
        Custom
    }
    [System.Serializable]
    public enum WorldType
    {
        World3D,
        World2D
    }
}
