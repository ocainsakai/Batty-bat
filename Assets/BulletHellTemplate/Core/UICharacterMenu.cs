using BulletHellTemplate.Core.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the UI menu for character selection, filtering, sorting, and detailed character view.
    /// Applies the characterStatsComponent from character base, level upgrades, and equipped items (including upgrade bonuses).
    /// </summary>
    public class UICharacterMenu : MonoBehaviour
    {
        [Header("Menu Sections")]
        [Tooltip("Panel that displays the list of characters.")]
        public GameObject characterSelection;

        [Tooltip("Panel that displays the details of a specific character.")]
        public GameObject characterDetails;

        [Header("Prefabs and Container")]
        [Tooltip("Prefab for each character entry in the UI.")]
        public CharacterEntry characterEntryPrefab;

        [Tooltip("Container to hold the temporarily spawned character model.")]
        public Transform tempCharacterContainer;

        [Tooltip("Container to hold the character entries.")]
        public Transform container;

        [Tooltip("UI element for displaying skill information.")]
        public UISkillInfo uiSkillInfo;

        [Tooltip("Prefab for each skill info entry in the UI.")]
        public SkillInfoEntry skillInfoPrefab;

        [Tooltip("Button component for marking a character as favourite.")]
        public FavouriteCharacter favouriteCharacterButton;

        [Tooltip("Image component to indicate which character is marked as favourite.")]
        public Image favouriteSelected;

        [Tooltip("Container to hold the displayed character skills.")]
        public Transform containerSkills;

        [Tooltip("Prefab for skin entry UI.")]
        public SkinEntry skinEntry;

        [Tooltip("Container to hold the skin entries.")]
        public Transform skinsContainer;

        [Tooltip("UI GameObject for skin selection.")]
        public GameObject UISkins;

        [Tooltip("Button to open the skin selection UI.")]
        public Button openSkinSelection;

        public Button unlockSkinButton;

        public TextMeshProUGUI unlockSkinPrice;

        public Image unlockedCurrencyIcon;

        public RawImage tempCharacterRendererImage;

        [Header("UIItemsApply")]
        [Tooltip("Reference to the UIItemsApply system that manages equipping items.")]
        public UIItemsApply uiItemsApply;

        [Header("Upgrade Prefabs and Container")]
        [Tooltip("Prefab for each upgrade entry in the UI.")]
        public UpgradeEntry upgradeEntryPrefab;

        [Tooltip("Container to hold the upgrade entries.")]
        public Transform containerUpgrades;

        [Header("Filters")]
        [Tooltip("Shows only characters that have an active quest requirement.")]
        public Toggle filterCharacterWithQuest;

        [Tooltip("Shows only characters that can level up (EXP + cost requirements).")]
        public Toggle filterCharacterCanUpgrade;

        [Tooltip("Filters characters by a substring in their names.")]
        public TMP_InputField searchByWord;

        [Header("Orders options")]
        [Tooltip("Sort characters by rarity (Legendary > Epic > Rare > Uncommon > Common). Only one order can be active at a time.")]
        public Toggle orderByRarity;

        [Tooltip("Sort characters by their current level (higher first). Only one order can be active at a time.")]
        public Toggle orderByLevel;

        [Tooltip("Sort characters by their current mastery level (higher first). Only one order can be active at a time.")]
        public Toggle orderByMastery;

        [Header("Order Direction")]
        [Tooltip("If active, inverts the sorted list to ascending order.")]
        public Toggle orderByDescending;

        [Header("UI")]
        [Tooltip("Button to go back to the character selection list.")]
        public Button backToCharacterSelection;

        [Tooltip("Button to confirm the selection of the currently viewed character.")]
        public Button selectCharacterButton;

        [Tooltip("Button to level up the currently viewed character.")]
        public Button levelUpCharacter;

        [Tooltip("Text that displays the price required to level up.")]
        public TextMeshProUGUI levelUpPrice;

        [Tooltip("Icon for the currency used in leveling up.")]
        public Image currencyLevelUpIcon;

        [Tooltip("Text that displays the character's name in the details panel.")]
        public TextMeshProUGUI characterName;

        [Tooltip("Text that displays the character's description in the details panel.")]
        public TextMeshProUGUI characterDescription;

        [Tooltip("Text that displays the character's rarity in the details panel.")]
        public TextMeshProUGUI characterRarity;

        [Tooltip("Text that displays the character's class type.")]
        public TextMeshProUGUI characterClass;

        [Tooltip("Icon that displays the character's class.")]
        public Image characterClassIcon;

        [Tooltip("Icon that displays the character's elemental type.")]
        public Image characterElementalIcon;

        [Tooltip("Text that displays the character's current level.")]
        public TextMeshProUGUI characterLevel;

        [Tooltip("Text that displays the character's current EXP in the format current/required.")]
        public TextMeshProUGUI characterCurrentExp;

        [Tooltip("Image progress bar for character level EXP.")]
        public Image curentProgressLevelExpBar;

        [Tooltip("Text that displays the current mastery name.")]
        public TextMeshProUGUI masteryName;

        [Tooltip("Icon that displays the current mastery icon.")]
        public Image masteryIcon;

        [Tooltip("Text that displays the character's current mastery EXP in the format current/required.")]
        public TextMeshProUGUI characterCurrentMasteryExp;

        [Tooltip("Image progress bar for mastery EXP.")]
        public Image curentProgressMasteryExpBar;

        [Header("Character Stats (Basic)")]
        [Tooltip("Displays the base HP of the character.")]
        public TextMeshProUGUI characterHp;

        [Tooltip("Displays additional HP bonus (only from characterBuffsComponent/items/runes).")]
        public TextMeshProUGUI characterHpBonus;

        [Tooltip("Displays the base damage of the character.")]
        public TextMeshProUGUI characterDamage;

        [Tooltip("Displays additional damage bonus (only from characterBuffsComponent/items/runes).")]
        public TextMeshProUGUI characterDamageBonus;

        [Tooltip("Displays the base energy/MP of the character.")]
        public TextMeshProUGUI characterEnergy;

        [Tooltip("Displays additional energy bonus (only from characterBuffsComponent/items/runes).")]
        public TextMeshProUGUI characterEnergyBonus;

        [Header("Rarity Settings")]
        [Tooltip("The Image that will display the rarity banner sprite.")]
        public Image rarityImage;

        [Tooltip("Banner for 'Common' rarity.")]
        public Sprite commonBanner;

        [Tooltip("Banner for 'Uncommon' rarity.")]
        public Sprite uncommonBanner;

        [Tooltip("Banner for 'Rare' rarity.")]
        public Sprite rareBanner;

        [Tooltip("Banner for 'Epic' rarity.")]
        public Sprite epicBanner;

        [Tooltip("Banner for 'Legendary' rarity.")]
        public Sprite legendaryBanner;

        [Header("Rarity Text Gradient")]
        [Tooltip("Gradient colors used for 'Common' rarity.")]
        public TextRarityGradient commonTextColor;

        [Tooltip("Gradient colors used for 'Uncommon' rarity.")]
        public TextRarityGradient uncommonTextColor;

        [Tooltip("Gradient colors used for 'Rare' rarity.")]
        public TextRarityGradient rareTextColor;

        [Tooltip("Gradient colors used for 'Epic' rarity.")]
        public TextRarityGradient epicTextColor;

        [Tooltip("Gradient colors used for 'Legendary' rarity.")]
        public TextRarityGradient legendaryTextColor;

        [Header("Character Detailed Stats UI")]
        [Tooltip("Displays the character's HP.")]
        public TextMeshProUGUI hpText;

        [Tooltip("Displays the character's HP regeneration.")]
        public TextMeshProUGUI hpRegenText;

        [Tooltip("Displays the character's HP leech percentage.")]
        public TextMeshProUGUI hpLeechText;

        [Tooltip("Displays the character's MP.")]
        public TextMeshProUGUI mpText;

        [Tooltip("Displays the character's MP regeneration.")]
        public TextMeshProUGUI mpRegenText;

        [Tooltip("Displays the character's base damage.")]
        public TextMeshProUGUI damageText;

        [Tooltip("Displays the character's attack speed.")]
        public TextMeshProUGUI attackSpeedText;

        [Tooltip("Displays the character's cooldown reduction percentage.")]
        public TextMeshProUGUI cooldownReductionText;

        [Tooltip("Displays the character's critical rate percentage.")]
        public TextMeshProUGUI criticalRateText;

        [Tooltip("Displays the character's critical damage multiplier.")]
        public TextMeshProUGUI criticalDamageMultiplierText;

        [Tooltip("Displays the character's defense value.")]
        public TextMeshProUGUI defenseText;

        [Tooltip("Displays the character's shield value.")]
        public TextMeshProUGUI shieldText;

        [Tooltip("Displays the character's move speed.")]
        public TextMeshProUGUI moveSpeedText;

        [Tooltip("Displays the character's collect range.")]
        public TextMeshProUGUI collectRangeText;

        [Header("Stat Colors")]
        [Tooltip("Color for stat name text.")]
        public Color statNameColor;

        [Tooltip("Color for stat value text.")]
        public Color statValueColor;

        [Tooltip("Color for stat bonus text.")]
        public Color statBonusColor;

        [Header("Error Message Text")]
        [Tooltip("Displays any relevant error messages during character upgrades.")]
        public TextMeshProUGUI errorMessageUpgrade;

        [Header("UI Translations")]
        [Tooltip("Translations for stat names and common messages.")]
        public CharacterMenuTranslations uiTranslations;

        [Header("Events")]
        [Tooltip("Event invoked when the menu is opened.")]
        public UnityEvent OnOpenMenu;

        [Tooltip("Event invoked when the menu is closed.")]
        public UnityEvent OnCloseMenu;

        [Tooltip("Event invoked when character details panel opens.")]
        public UnityEvent OnOpenCharacterDetails;

        [Tooltip("Event invoked when a character upgrade happens.")]
        public UnityEvent OnCharacterUpgrade;

        [Tooltip("Event invoked when an item is added to a character.")]
        public UnityEvent OnCharacterAddItem;

        [Tooltip("Event invoked when a rune is added to a character.")]
        public UnityEvent OnCharacterAddRune;

        public static UICharacterMenu Singleton { get; private set; }

        // Holds the currently viewed character ID in the details panel
        private int currentDetailCharacterId;
        private int currentDetailCharacterSkin;
        [HideInInspector]public CharacterModel tempCharacterModel;
        private string currentLang;
        private List<CharacterData> unlockedCharacters = new List<CharacterData>();
        private List<CharacterEntry> characterEntries = new List<CharacterEntry>();

        private void Start()
        {
            Singleton = this;
            if (backToCharacterSelection != null)
                backToCharacterSelection.onClick.AddListener(OnClickBackToCharacterSelection);
            if (selectCharacterButton != null)
                selectCharacterButton.onClick.AddListener(OnClickSelectCharacter);
            if (levelUpCharacter != null)
                levelUpCharacter.onClick.AddListener(OnClickLevelUp);
            if (filterCharacterWithQuest != null)
                filterCharacterWithQuest.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (filterCharacterCanUpgrade != null)
                filterCharacterCanUpgrade.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (orderByRarity != null)
                orderByRarity.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (orderByLevel != null)
                orderByLevel.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (orderByMastery != null)
                orderByMastery.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (orderByDescending != null)
                orderByDescending.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (searchByWord != null)
                searchByWord.onValueChanged.AddListener((val) => ApplyFiltersAndSorting());
            if (unlockSkinButton != null)
            {
                unlockSkinButton.onClick.AddListener(UnlockSkin);
                unlockSkinButton.gameObject.SetActive(false);
            }
            if (openSkinSelection != null)
                openSkinSelection.onClick.AddListener(LoadCharacterSkins);

        }

        private void OnEnable()
        {
            OnOpenMenu.Invoke();
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            ResetFilters();
            if (characterSelection != null) characterSelection.SetActive(true);
            if (characterDetails != null) characterDetails.SetActive(false);
            DestroyTemporaryModel();
            StoreUnlockedCharacters();
            ApplyFiltersAndSorting();         
        }

        private void OnDisable()
        {
            OnCloseMenu.Invoke();
            DestroyTemporaryModel();
        }

        private void ResetFilters()
        {
            if (filterCharacterWithQuest != null)
                filterCharacterWithQuest.isOn = false;
            if (filterCharacterCanUpgrade != null)
                filterCharacterCanUpgrade.isOn = false;
            if (searchByWord != null)
                searchByWord.text = string.Empty;
            if (orderByRarity != null)
                orderByRarity.isOn = false;
            if (orderByLevel != null)
                orderByLevel.isOn = false;
            if (orderByMastery != null)
                orderByMastery.isOn = false;
            if (orderByDescending != null)
                orderByDescending.isOn = false;
        }

        private void StoreUnlockedCharacters()
        {
            unlockedCharacters.Clear();
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null)
            {
                Debug.LogError("GameInstance or characterData is null.");
                return;
            }
            foreach (CharacterData item in GameInstance.Singleton.characterData)
            {
                bool isUnlocked = item.CheckUnlocked || PlayerSave.IsCharacterPurchased(item.characterId.ToString());
                if (isUnlocked)
                    unlockedCharacters.Add(item);
            }
        }

        public void ApplyFiltersAndSorting()
        {
            List<CharacterData> filteredList = new List<CharacterData>(unlockedCharacters);
            if (!string.IsNullOrEmpty(searchByWord.text))
            {
                string lowercaseSearch = searchByWord.text.ToLower();
                filteredList = filteredList.FindAll(cd => cd.characterName.ToLower().Contains(lowercaseSearch));
            }
            if (filterCharacterCanUpgrade != null && filterCharacterCanUpgrade.isOn)
                filteredList = filteredList.FindAll(cd => CharacterCanUpgrade(cd));
            if (filterCharacterWithQuest != null && filterCharacterWithQuest.isOn)
                filteredList = filteredList.FindAll(cd => CharacterHasQuest(cd));
            if (orderByRarity != null && orderByRarity.isOn)
                filteredList.Sort((a, b) => CompareByRarityDescending(a, b));
            else if (orderByLevel != null && orderByLevel.isOn)
                filteredList.Sort((a, b) => PlayerSave.GetCharacterLevel(b.characterId).CompareTo(PlayerSave.GetCharacterLevel(a.characterId)));
            else if (orderByMastery != null && orderByMastery.isOn)
                filteredList.Sort((a, b) => PlayerSave.GetCharacterMasteryLevel(b.characterId).CompareTo(PlayerSave.GetCharacterMasteryLevel(a.characterId)));

            if (orderByDescending != null && orderByDescending.isOn)
                filteredList.Reverse();

            DisplayCharacters(filteredList);
        }

        private bool CharacterCanUpgrade(CharacterData cd)
        {
            int currentLevel = PlayerSave.GetCharacterLevel(cd.characterId);
            if (currentLevel >= cd.maxLevel)
                return false;
            int nextLevelIndex = currentLevel;
            if (nextLevelIndex < 0 || nextLevelIndex >= cd.expPerLevel.Length)
                return false;
            int currentExp = PlayerSave.GetCharacterCurrentExp(cd.characterId);
            int expNeeded = cd.expPerLevel[nextLevelIndex];
            if (currentExp < expNeeded)
                return false;
            if (nextLevelIndex < 0 || nextLevelIndex >= cd.upgradeCostPerLevel.Length)
                return false;
            int costNeeded = cd.upgradeCostPerLevel[nextLevelIndex];
            int playerCurrency = MonetizationManager.GetCurrency(cd.currencyId);
            return (playerCurrency >= costNeeded);
        }

        private bool CharacterHasQuest(CharacterData cd)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.questData == null)
                return false;
            foreach (QuestItem quest in GameInstance.Singleton.questData)
            {
                if (PlayerSave.IsQuestCompleted(quest.questId))
                    continue;
                if (quest.requirement == null)
                    continue;
                if (quest.requirement.targetCharacter != null &&
                    quest.requirement.targetCharacter.characterId == cd.characterId)
                {
                    return true;
                }
            }
            return false;
        }

        private int CompareByRarityDescending(CharacterData a, CharacterData b)
        {
            Dictionary<CharacterRarity, int> rarityOrder = new Dictionary<CharacterRarity, int>()
            {
                { CharacterRarity.Legendary, 4 },
                { CharacterRarity.Epic, 3 },
                { CharacterRarity.Rare, 2 },
                { CharacterRarity.Uncommon, 1 },
                { CharacterRarity.Common, 0 }
            };
            return rarityOrder[b.characterRarity].CompareTo(rarityOrder[a.characterRarity]);
        }

        private void DisplayCharacters(List<CharacterData> listToDisplay)
        {
            ClearCharacterEntries();
            foreach (Transform child in container)
                Destroy(child.gameObject);
            characterEntries.Clear();

            foreach (CharacterData item in listToDisplay)
            {
                try
                {
                    CharacterEntry entry = Instantiate(characterEntryPrefab, container);
                    string characterNameTranslated = GetTranslatedString(item.characterNameTranslated, item.characterName, currentLang);
                    entry.Setup(item, characterNameTranslated);

                    if (entry.selected != null)
                        entry.selected.gameObject.SetActive(item.characterId == PlayerSave.GetSelectedCharacter());

                    characterEntries.Add(entry);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error instantiating character entry for {item.characterName}: {ex.Message}");
                }
            }
            UpdateFavouriteSelected(PlayerSave.GetSelectedCharacter());
        }

        public void ReloadCharacters()
        {
            ApplyFiltersAndSorting();
        }

        public void ShowCharacterDetails(int characterId)
        {
            OnOpenCharacterDetails.Invoke();
            currentDetailCharacterId = characterId;
            favouriteCharacterButton.SetCharacterId(currentDetailCharacterId);
            if (characterSelection != null) characterSelection.SetActive(false);
            if (characterDetails != null) characterDetails.SetActive(true);
            UISkins.SetActive(false);
            unlockSkinButton.gameObject.SetActive(false);
            if (tempCharacterRendererImage != null) tempCharacterRendererImage.color = Color.white;
            DestroyTemporaryModel();
            CharacterData cd = GetCharacterDataById(characterId);
            if (cd != null && tempCharacterContainer != null)
            {
                int skinIndex = PlayerSave.GetCharacterSkin(cd.characterId);
                if (cd.characterSkins != null && cd.characterSkins.Length > 0 && skinIndex >= 0 && skinIndex < cd.characterSkins.Length)
                {
                    CharacterSkin skin = cd.characterSkins[skinIndex];
                    if (skin.skinCharacterModel != null)
                        tempCharacterModel = Instantiate(skin.skinCharacterModel, tempCharacterContainer);
                }
                else if (cd.characterModel != null)
                {
                    tempCharacterModel = Instantiate(cd.characterModel, tempCharacterContainer);
                }
            }
            UpdateDetailPanel(cd);
            LoadCharacterSkills(characterId);
            LoadCharacterUpgrades(characterId);
            UpdateFavouriteSelected(characterId);
            uiItemsApply.SetupSlotsForCurrentCharacter(characterId);
        }

        public void ShowPreviewSkin(int skinIndex)
        {
            if (tempCharacterRendererImage != null)
                tempCharacterRendererImage.color = Color.grey;

            DestroyTemporaryModel();

            CharacterData cd = GetCharacterDataById(currentDetailCharacterId);
            if (cd != null && tempCharacterContainer != null)
            {
                if (cd.characterSkins != null && cd.characterSkins.Length > 0 && skinIndex >= 0 && skinIndex < cd.characterSkins.Length)
                {
                    CharacterSkin skin = cd.characterSkins[skinIndex];
                    if (skin.skinCharacterModel != null)
                    {
                        Instantiate(skin.skinCharacterModel, tempCharacterContainer);
                    }
                    else if (cd.characterModel != null)
                    {
                        Instantiate(cd.characterModel, tempCharacterContainer);
                    }
                }
                else if (cd.characterModel != null)
                {
                    Instantiate(cd.characterModel, tempCharacterContainer);
                }
            }
        }

        /// <summary>
        /// Updates the UI details panel with character info, including level, EXP and mastery progress.
        /// </summary>
        /// <param name="cd">Character data to display.</param>
        public void UpdateDetailPanel(CharacterData cd)
        {
            if (cd == null) return;

            string translatedName = GetTranslatedString(cd.characterNameTranslated, cd.characterName, currentLang);
            string translatedDesc = GetTranslatedString(cd.characterDescriptionTranslated, cd.characterDescription, currentLang);
            if (characterName != null) characterName.text = translatedName;
            if (characterDescription != null) characterDescription.text = translatedDesc;

            // Rarity
            if (characterRarity != null)
            {
                characterRarity.text = GetRarityTranslated(cd.characterRarity.ToString());
                ApplyRarityGradient(characterRarity, cd.characterRarity);
            }
            if (rarityImage != null)
                rarityImage.sprite = GetRarityBanner(cd.characterRarity);

            // Class & Icon
            string translatedClass = GetTranslatedString(cd.characterClassTranslated, cd.characterClassType, currentLang);
            if (characterClass != null) characterClass.text = translatedClass;
            if (characterClassIcon != null) characterClassIcon.sprite = cd.characterClassIcon;
            if (characterElementalIcon != null) characterElementalIcon.sprite = cd.GetCharacterTypeIcon();

            // Level & EXP
            int lvl = PlayerSave.GetCharacterLevel(cd.characterId);
            int currExp = PlayerSave.GetCharacterCurrentExp(cd.characterId);
            if (characterLevel != null) characterLevel.text = $"{lvl}";
            if (lvl < cd.maxLevel && lvl < cd.expPerLevel.Length)
            {
                int requiredExp = cd.expPerLevel[lvl];
                if (characterCurrentExp != null) characterCurrentExp.text = $"{currExp}/{requiredExp}";
                if (curentProgressLevelExpBar != null) curentProgressLevelExpBar.fillAmount = (float)currExp / requiredExp;
            }
            else
            {
                if (characterCurrentExp != null) characterCurrentExp.text = "MAX";
                if (curentProgressLevelExpBar != null) curentProgressLevelExpBar.fillAmount = 1f;
            }

            // Mastery
            int currentMasteryLevel = PlayerSave.GetCharacterMasteryLevel(cd.characterId);
            CharacterMasteryLevel masteryInfo = GameInstance.Singleton.GetMasteryLevel(currentMasteryLevel);
            string masteryTranslatedName = GetTranslatedString(masteryInfo.masteryNameTranslated, masteryInfo.masteryName, currentLang);
            if (masteryName != null) masteryName.text = masteryTranslatedName;
            if (masteryIcon != null) masteryIcon.sprite = masteryInfo.masteryIcon;
            int currMasteryExp = PlayerSave.GetCharacterCurrentMasteryExp(cd.characterId);

            // When the character is not at max mastery level (max index is maxMasteryLevel - 1)
            if (currentMasteryLevel < GameInstance.Singleton.characterMastery.maxMasteryLevel - 1)
            {
                int requiredMasteryExp = GameInstance.Singleton.GetMasteryExpForLevel(currentMasteryLevel);
                if (characterCurrentMasteryExp != null) characterCurrentMasteryExp.text = $"{currMasteryExp}/{requiredMasteryExp}";
                if (curentProgressMasteryExpBar != null) curentProgressMasteryExpBar.fillAmount = (float)currMasteryExp / requiredMasteryExp;
            }
            else
            {
                // When at max mastery level, display 0/MAX (since EXP is reset to 0 on level up)
                if (characterCurrentMasteryExp != null) characterCurrentMasteryExp.text = $"MAX";
                if (curentProgressMasteryExpBar != null) curentProgressMasteryExpBar.fillAmount = 1f;
            }

            // Upgrade Price & Currency
            if (lvl < cd.maxLevel && lvl >= 1 && lvl < cd.expPerLevel.Length)
            {
                int costNeeded = cd.upgradeCostPerLevel[lvl];
                if (levelUpPrice != null) levelUpPrice.text = costNeeded.ToString();
                int playerCurrency = MonetizationManager.GetCurrency(cd.currencyId);
                if (currencyLevelUpIcon != null)
                    currencyLevelUpIcon.sprite = MonetizationManager.Singleton.GetCurrencyIcon(cd.currencyId);
                if (levelUpCharacter != null)
                    levelUpCharacter.interactable = (playerCurrency >= costNeeded);
            }
            else
            {
                if (levelUpPrice != null) levelUpPrice.text = "----";
                if (levelUpCharacter != null) levelUpCharacter.interactable = false;
            }

            // Now update both Detailed and Basic characterStatsComponent
            UpdateDetailedStats();
            UpdateBasicStats(cd);
        }


        /// <summary>
        /// Calculates final characterStatsComponent for display, including base characterStatsComponent (with level-based scaling),
        /// character upgrades, and equipped item characterStatsComponent. Updates the detailed stat UI.
        /// All displayed stat values are rounded to an integer.
        /// </summary>
        /// <param name="cd">The character data.</param>
        /// <param name="currentLevel">The current level of the character.</param>
        public void UpdateDetailedStats()
        {
            CharacterData cd = GetCharacterDataById(currentDetailCharacterId);
            int currentLevel = PlayerSave.GetCharacterLevel(currentDetailCharacterId);
            if (cd == null) return;

            // Create a runtime characterStatsComponent object from base characterStatsComponent
            CharacterStatsRuntime finalStats = new CharacterStatsRuntime(cd.baseStats);

            // Apply level-based multiplier (e.g. +10% per level above 1)
            float levelMultiplier = 1f + (cd.statsPercentageIncreaseByLevel * (currentLevel - 1));
            if (currentLevel > 1 && levelMultiplier > 1f)
            {
                finalStats.baseHP *= levelMultiplier;
                finalStats.baseHPRegen *= levelMultiplier;
                finalStats.baseHPLeech *= levelMultiplier;
                finalStats.baseMP *= levelMultiplier;
                finalStats.baseMPRegen *= levelMultiplier;
                finalStats.baseDamage *= levelMultiplier;
                finalStats.baseAttackSpeed *= levelMultiplier;
                finalStats.baseCooldownReduction *= levelMultiplier;
                finalStats.baseCriticalRate *= levelMultiplier;
                finalStats.baseCriticalDamageMultiplier *= levelMultiplier;
                finalStats.baseDefense *= levelMultiplier;
                finalStats.baseShield *= levelMultiplier;
                finalStats.baseMoveSpeed *= levelMultiplier;
                finalStats.baseCollectRange *= levelMultiplier;
            }

            // Apply character's own stat upgrades (e.g., from "statUpgrades" array)
            Dictionary<StatType, int> upgradeLevels = PlayerSave.LoadAllCharacterUpgradeLevels(cd.characterId);
            foreach (StatUpgrade upgrade in cd.statUpgrades)
            {
                if (upgradeLevels.ContainsKey(upgrade.statType) && upgradeLevels[upgrade.statType] > 0)
                {
                    finalStats.ApplyStatUpgrade(upgrade.statType, upgradeLevels[upgrade.statType], upgrade);
                }
            }

            // Apply item characterStatsComponent (base + upgrade bonuses)
            ApplyEquippedItemsStats(cd, finalStats);

            // Retrieve translated strings for each stat using the current language
            string _hpTranslated = GetTranslatedString(uiTranslations.hpTranslation, uiTranslations.defaultHp, currentLang);
            string _hpRegenTranslated = GetTranslatedString(uiTranslations.hpRegenTranslation, uiTranslations.defaultHpRegen, currentLang);
            string _hpLeechTranslated = GetTranslatedString(uiTranslations.hpLeechTranslation, uiTranslations.defaultHpLeech, currentLang);
            string _mpTranslated = GetTranslatedString(uiTranslations.mpTranslation, uiTranslations.defaultMp, currentLang);
            string _mpRegenTranslated = GetTranslatedString(uiTranslations.mpRegenTranslation, uiTranslations.defaultMpRegen, currentLang);
            string _damageTranslated = GetTranslatedString(uiTranslations.damageTranslation, uiTranslations.defaultDamage, currentLang);
            string _attackSpeedTranslated = GetTranslatedString(uiTranslations.attackSpeedTranslation, uiTranslations.defaultAttackSpeed, currentLang);
            string _cooldownReductionTranslated = GetTranslatedString(uiTranslations.cooldownReductionTranslation, uiTranslations.defaultCooldownReduction, currentLang);
            string _criticalRateTranslated = GetTranslatedString(uiTranslations.criticalRateTranslation, uiTranslations.defaultCriticalRate, currentLang);
            string _criticalDamageMultiplierTranslated = GetTranslatedString(uiTranslations.criticalDamageMultiplierTranslation, uiTranslations.defaultCriticalDamageMultiplier, currentLang);
            string _defenseTranslated = GetTranslatedString(uiTranslations.defenseTranslation, uiTranslations.defaultDefense, currentLang);
            string _shieldTranslated = GetTranslatedString(uiTranslations.shieldTranslation, uiTranslations.defaultShield, currentLang);
            string _moveSpeedTranslated = GetTranslatedString(uiTranslations.moveSpeedTranslation, uiTranslations.defaultMoveSpeed, currentLang);
            string _collectRangeTranslated = GetTranslatedString(uiTranslations.collectRangeTranslation, uiTranslations.defaultCollectRange, currentLang);

            // Display final characterStatsComponent in the detailed UI (rounded to int)
            if (hpText != null)
                hpText.text = $"{_hpTranslated} {Mathf.RoundToInt(finalStats.baseHP)}";
            if (hpRegenText != null)
                hpRegenText.text = $"{_hpRegenTranslated}: {Mathf.RoundToInt(finalStats.baseHPRegen)}/s";
            if (hpLeechText != null)
                hpLeechText.text = $"{_hpLeechTranslated}: {Mathf.RoundToInt(finalStats.baseHPLeech)}%";
            if (mpText != null)
                mpText.text = $"{_mpTranslated}: {Mathf.RoundToInt(finalStats.baseMP)}";
            if (mpRegenText != null)
                mpRegenText.text = $"{_mpRegenTranslated}: {Mathf.RoundToInt(finalStats.baseMPRegen)}/s";
            if (damageText != null)
                damageText.text = $"{_damageTranslated}: {Mathf.RoundToInt(finalStats.baseDamage)}";
            if (attackSpeedText != null)
                attackSpeedText.text = $"{_attackSpeedTranslated}: {Mathf.RoundToInt(finalStats.baseAttackSpeed)}";
            if (cooldownReductionText != null)
                cooldownReductionText.text = $"{_cooldownReductionTranslated}: {Mathf.RoundToInt(finalStats.baseCooldownReduction)}%";
            if (criticalRateText != null)
                criticalRateText.text = $"{_criticalRateTranslated}: {Mathf.RoundToInt(finalStats.baseCriticalRate)}%";
            if (criticalDamageMultiplierText != null)
                criticalDamageMultiplierText.text = $"{_criticalDamageMultiplierTranslated}: {Mathf.RoundToInt(finalStats.baseCriticalDamageMultiplier)}x";
            if (defenseText != null)
                defenseText.text = $"{_defenseTranslated}: {Mathf.RoundToInt(finalStats.baseDefense)}";
            if (shieldText != null)
                shieldText.text = $"{_shieldTranslated}: {Mathf.RoundToInt(finalStats.baseShield)}";
            if (moveSpeedText != null)
                moveSpeedText.text = $"{_moveSpeedTranslated}: {Mathf.RoundToInt(finalStats.baseMoveSpeed)}";
            if (collectRangeText != null)
                collectRangeText.text = $"{_collectRangeTranslated}: {Mathf.RoundToInt(finalStats.baseCollectRange)}";
        }

        /// <summary>
        /// Updates the "basic" characterStatsComponent UI (base + bonus) for HP, Damage, MP, etc.,
        /// including the level-based multiplier in the base portion.
        /// All displayed stat values are rounded to an integer.
        /// </summary>
        /// <param name="cd">The character data.</param>
        private void UpdateBasicStats(CharacterData cd)
        {
            if (cd == null) return;

            int currentLevel = PlayerSave.GetCharacterLevel(cd.characterId);

            // Start with raw base characterStatsComponent
            float baseHP = cd.baseStats.baseHP;
            float baseDamage = cd.baseStats.baseDamage;
            float baseMP = cd.baseStats.baseMP;

            // Apply level-based multiplier to the "base" portion
            float levelMultiplier = 1f + (cd.statsPercentageIncreaseByLevel * (currentLevel - 1));
            if (currentLevel > 1 && levelMultiplier > 1f)
            {
                baseHP *= levelMultiplier;
                baseDamage *= levelMultiplier;
                baseMP *= levelMultiplier;
            }

            // Build final characterStatsComponent for calculating the total (with items + upgrades)
            CharacterStatsRuntime finalStats = new CharacterStatsRuntime(cd.baseStats);

            // Also apply the same level-based multiplier to finalStats
            if (currentLevel > 1 && levelMultiplier > 1f)
            {
                finalStats.baseHP *= levelMultiplier;
                finalStats.baseHPRegen *= levelMultiplier;
                finalStats.baseHPLeech *= levelMultiplier;
                finalStats.baseMP *= levelMultiplier;
                finalStats.baseMPRegen *= levelMultiplier;
                finalStats.baseDamage *= levelMultiplier;
                finalStats.baseAttackSpeed *= levelMultiplier;
                finalStats.baseCooldownReduction *= levelMultiplier;
                finalStats.baseCriticalRate *= levelMultiplier;
                finalStats.baseCriticalDamageMultiplier *= levelMultiplier;
                finalStats.baseDefense *= levelMultiplier;
                finalStats.baseShield *= levelMultiplier;
                finalStats.baseMoveSpeed *= levelMultiplier;
                finalStats.baseCollectRange *= levelMultiplier;
            }

            // Apply character upgrades
            Dictionary<StatType, int> upgradeLevels = PlayerSave.LoadAllCharacterUpgradeLevels(cd.characterId);
            foreach (StatUpgrade upgrade in cd.statUpgrades)
            {
                if (upgradeLevels.ContainsKey(upgrade.statType) && upgradeLevels[upgrade.statType] > 0)
                {
                    finalStats.ApplyStatUpgrade(upgrade.statType, upgradeLevels[upgrade.statType], upgrade);
                }
            }

            // Apply equipped items
            ApplyEquippedItemsStats(cd, finalStats);

            // Calculate bonus by comparing finalStats with the new "base" (already scaled by level)
            float bonusHP = finalStats.baseHP - baseHP;
            float bonusDamage = finalStats.baseDamage - baseDamage;
            float bonusMP = finalStats.baseMP - baseMP;

            // Display base characterStatsComponent (scaled by level, then rounded) and separate bonus (rounded)
            if (characterHp != null)
                characterHp.text = Mathf.RoundToInt(finalStats.baseHP).ToString();
            if (characterHpBonus != null)
                characterHpBonus.text = bonusHP > 0 ? $"+{Mathf.RoundToInt(bonusHP)}" : "0";

            if (characterDamage != null)
                characterDamage.text = Mathf.RoundToInt(finalStats.baseDamage).ToString();
            if (characterDamageBonus != null)
                characterDamageBonus.text = bonusDamage > 0 ? $"+{Mathf.RoundToInt(bonusDamage)}" : "0";

            if (characterEnergy != null)
                characterEnergy.text = Mathf.RoundToInt(finalStats.baseMP).ToString();
            if (characterEnergyBonus != null)
                characterEnergyBonus.text = bonusMP > 0 ? $"+{Mathf.RoundToInt(bonusMP)}" : "0";
        }


        /// <summary>
        /// Applies characterStatsComponent from all equipped items (base itemStats + upgrade bonuses) to the provided CharacterStatsRuntime.
        /// This uses the new uniqueItemGuid approach (or itemId if needed).
        /// </summary>
        private void ApplyEquippedItemsStats(CharacterData cd, CharacterStatsRuntime finalStats)
        {
            // For each slot in this character, find the unique item GUID
            foreach (string slotName in cd.itemSlots)
            {
                // You can store the equipped GUID in PlayerPrefs or a dictionary
                // Example:
                string uniqueItemGuid = InventorySave.GetEquippedItemForSlot(cd.characterId, slotName);
                if (string.IsNullOrEmpty(uniqueItemGuid)) continue;

                // Find the purchased item data
                var purchasedItem = PlayerSave.GetInventoryItems()
                    .Find(pi => pi.uniqueItemGuid == uniqueItemGuid);

                if (purchasedItem == null) continue; // not found or error
                // Map to scriptable
                var soItem = FindItemById(purchasedItem.itemId);
                if (soItem == null) continue;

                // Add base item characterStatsComponent
                if (soItem.itemStats != null)
                {
                    finalStats.baseHP += soItem.itemStats.baseHP;
                    finalStats.baseHPRegen += soItem.itemStats.baseHPRegen;
                    finalStats.baseHPLeech += soItem.itemStats.baseHPLeech;
                    finalStats.baseMP += soItem.itemStats.baseMP;
                    finalStats.baseMPRegen += soItem.itemStats.baseMPRegen;
                    finalStats.baseDamage += soItem.itemStats.baseDamage;
                    finalStats.baseAttackSpeed += soItem.itemStats.baseAttackSpeed;
                    finalStats.baseCooldownReduction += soItem.itemStats.baseCooldownReduction;
                    finalStats.baseCriticalRate += soItem.itemStats.baseCriticalRate;
                    finalStats.baseCriticalDamageMultiplier += soItem.itemStats.baseCriticalDamageMultiplier;
                    finalStats.baseDefense += soItem.itemStats.baseDefense;
                    finalStats.baseShield += soItem.itemStats.baseShield;
                    finalStats.baseMoveSpeed += soItem.itemStats.baseMoveSpeed;
                    finalStats.baseCollectRange += soItem.itemStats.baseCollectRange;
                }

                // Add item upgrade bonuses
                int itemLevel = InventorySave.GetItemUpgradeLevel(uniqueItemGuid);
                float totalUpgrade = 0f;
                for (int i = 0; i < itemLevel && i < soItem.itemUpgrades.Count; i++)
                {
                    totalUpgrade += soItem.itemUpgrades[i].statIncreasePercentagePerLevel;
                }
                if (totalUpgrade > 0f)
                {
                    float factor = totalUpgrade; // e.g. 0.2 = +20%
                    // Multiply characterStatsComponent from itemStats
                    finalStats.baseHP += soItem.itemStats.baseHP * factor;
                    finalStats.baseHPRegen += soItem.itemStats.baseHPRegen * factor;
                    finalStats.baseHPLeech += soItem.itemStats.baseHPLeech * factor;
                    finalStats.baseMP += soItem.itemStats.baseMP * factor;
                    finalStats.baseMPRegen += soItem.itemStats.baseMPRegen * factor;
                    finalStats.baseDamage += soItem.itemStats.baseDamage * factor;
                    finalStats.baseAttackSpeed += soItem.itemStats.baseAttackSpeed * factor;
                    finalStats.baseCooldownReduction += soItem.itemStats.baseCooldownReduction * factor;
                    finalStats.baseCriticalRate += soItem.itemStats.baseCriticalRate * factor;
                    finalStats.baseCriticalDamageMultiplier += soItem.itemStats.baseCriticalDamageMultiplier * factor;
                    finalStats.baseDefense += soItem.itemStats.baseDefense * factor;
                    finalStats.baseShield += soItem.itemStats.baseShield * factor;
                    finalStats.baseMoveSpeed += soItem.itemStats.baseMoveSpeed * factor;
                    finalStats.baseCollectRange += soItem.itemStats.baseCollectRange * factor;
                }
            }
        }

        private InventoryItem FindItemById(string itemId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.inventoryItems == null)
                return null;
            foreach (InventoryItem item in GameInstance.Singleton.inventoryItems)
            {
                if (item.itemId == itemId)
                    return item;
            }
            return null;
        }

        public void LoadCharacterSkills(int characterId)
        {
            foreach (Transform child in containerSkills)
                Destroy(child.gameObject);
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameInstance Singleton is null.");
                return;
            }
            CharacterData cd = GetCharacterDataById(characterId);
            if (cd == null)
            {
                Debug.LogError("Character not found.");
                return;
            }
            if (cd.skills == null || cd.skills.Length == 0)
                Debug.LogWarning("No skills available for this character.");
            foreach (SkillData skill in cd.skills)
            {
                try
                {
                    SkillInfoEntry entry = Instantiate(skillInfoPrefab, containerSkills);
                    string _skillName = GetTranslatedString(skill.skillNameTranslated, skill.skillName, currentLang);
                    string _skillDescription = GetTranslatedString(skill.skillDescriptionTranslated, skill.skillDescription, currentLang);
                    entry.SetSkillInfo(skill.icon, _skillName, _skillDescription);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error instantiating skill entry for {skill.skillName}: {ex.Message}");
                }
            }
            ApplyCharacterUpgrades(cd);
        }

        /// <summary>
        /// Applies the character upgrades and updates the detailed characterStatsComponent UI.
        /// </summary>
        private void ApplyCharacterUpgrades(CharacterData cd)
        {
            if (cd == null) return;
            Dictionary<StatType, int> upgradeLevels = PlayerSave.LoadAllCharacterUpgradeLevels(cd.characterId);
            // (The method UpdateDetailedStats calls ApplyEquippedItemsStats internally.)
            UpdateDetailedStats();
        }

        public void LoadCharacterUpgrades(int characterId)
        {
            foreach (Transform child in containerUpgrades)
                Destroy(child.gameObject);
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameInstance Singleton is null.");
                return;
            }
            CharacterData cd = GetCharacterDataById(characterId);
            if (cd == null)
            {
                Debug.LogError("Character not found.");
                return;
            }
            Dictionary<StatType, int> upgradeLevels = PlayerSave.LoadAllCharacterUpgradeLevels(characterId);
            foreach (StatUpgrade upgrade in cd.statUpgrades)
            {
                int currentLevel = upgradeLevels.ContainsKey(upgrade.statType) ? upgradeLevels[upgrade.statType] : 0;
                UpgradeEntry entry = Instantiate(upgradeEntryPrefab, containerUpgrades);
                entry.Initialize(upgrade, upgrade.upgradeIcon, new CharacterStatsRuntime(cd.baseStats), currentLevel, characterId);
            }
        }

        public void ClearCharacterEntries()
        {
            foreach (Transform child in container)
                Destroy(child.gameObject);
            characterEntries.Clear();
        }

        public async void ChangeSelectedCharacter(int characterId)
        {
            RequestResult ok = await BackendManager.Service.UpdateSelectedCharacterAsync(characterId);
            if (!ok.Success)
            {
                DisplayStatusMessage("Character is not unlocked!", Color.red);
                Debug.LogError($"Failed to update selected character in Backend");
                return;
            }
            if (uiSkillInfo != null) uiSkillInfo.gameObject.SetActive(false);
            foreach (CharacterEntry entry in characterEntries)
            {
                bool isSelected = (entry.GetCharacterId() == characterId);
                if (entry.selected != null) entry.selected.gameObject.SetActive(isSelected);
            }
            LoadCharacterSkills(characterId);
            LoadCharacterUpgrades(characterId);
            UpdateFavouriteSelected(characterId);
            if (UIMainMenu.Singleton != null) UIMainMenu.Singleton.ChangeSelectedCharacter(characterId);
            gameObject.SetActive(false);           
        }
        public void LoadCharacterSkins()
        {
            CharacterData characterData = GetCharacterDataById(currentDetailCharacterId);
            UISkins.SetActive(true);
            if (characterData.characterSkins == null) return;

            foreach (Transform child in skinsContainer)
            {
                Destroy(child.gameObject);
            }

            List<int> unlockedSkins = PlayerSave.LoadCharacterUnlockedSkins(currentDetailCharacterId);
            int index = 0;
            foreach (CharacterSkin skin in characterData.characterSkins)
            {
                bool isUnlocked = skin.isUnlocked || unlockedSkins.Contains(index);
                SkinEntry skinEntryInstance = Instantiate(skinEntry, skinsContainer);
                skinEntryInstance.SetupSkinEntry(skin, index, isUnlocked);
                index++;
            }
        }

        /// <summary>
        /// Changes the character skin if it is already unlocked; otherwise, shows the unlock prompt.
        /// </summary>
        /// <param name="characterSkinIndex">The index of the skin to change to.</param>
        public async void ChangeCharacterSkin(int characterSkinIndex)
        {
            UISkins.SetActive(false);
            currentDetailCharacterSkin = characterSkinIndex;
            CharacterData characterData = GetCharacterDataById(currentDetailCharacterId);
            RequestResult ok = await BackendManager.Service.UpdateCharacterSkin(currentDetailCharacterId, characterSkinIndex);

            if (ok.Success)
            {
                ShowCharacterDetails(currentDetailCharacterId);
                UIMainMenu.Singleton.UpdateCharacter();
                return;
            }

            unlockSkinButton.gameObject.SetActive(true);
            unlockSkinPrice.text = characterData.characterSkins[currentDetailCharacterSkin].unlockPrice.ToString();
            string currencyId = characterData.characterSkins[currentDetailCharacterSkin].unlockCurrencyId;
            unlockedCurrencyIcon.sprite = MonetizationManager.Singleton.GetCurrencyIcon(currencyId);
            ShowPreviewSkin(characterSkinIndex);           
        }

        /// <summary>
        /// Unlocks the selected skin if the player has sufficient currency.
        /// </summary>
        public async void UnlockSkin()
        {
            CharacterData characterData = GetCharacterDataById(currentDetailCharacterId);
            if (characterData.characterSkins == null)
                return;

            int skinId = currentDetailCharacterSkin;
            RequestResult ok = await BackendManager.Service.UnlockCharacterSkin(currentDetailCharacterId, currentDetailCharacterSkin);

            if (ok.Success)
            {
                ShowCharacterDetails(currentDetailCharacterId);
                return;
            }
            DisplayNotEnoughCurrency();           
        }


        /// <summary>
        /// Forces a manual update of the characterStatsComponent for the currently displayed character, e.g. after equipping an item.
        /// </summary>
        public void ForceUpdateCharacterStats()
        {
            if (currentDetailCharacterId <= 0) return;
            CharacterData cd = GetCharacterDataById(currentDetailCharacterId);
            if (cd == null) return;

            // Re-run the logic
            UpdateDetailPanel(cd);
            LoadCharacterSkills(cd.characterId);
            LoadCharacterUpgrades(cd.characterId);
        }

        private CharacterData GetCharacterDataById(int characterId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null)
                return null;
            foreach (CharacterData item in GameInstance.Singleton.characterData)
                if (item.characterId == characterId)
                    return item;
            return null;
        }

        public string GetRarityTranslated(string rarity)
        {
            switch (rarity)
            {
                case "Common":
                    return GetTranslatedString(uiTranslations.CommonRarityTranslated,uiTranslations.CommonRarity,currentLang);
                case "Uncommon":
                    return GetTranslatedString(uiTranslations.UncommonRarityTranslated, uiTranslations.UncommonRarity, currentLang);
                case "Rare":
                    return GetTranslatedString(uiTranslations.RareRarityTranslated, uiTranslations.RareRarity, currentLang);
                case "Epic":
                    return GetTranslatedString(uiTranslations.EpicRarityTranslated, uiTranslations.EpicRarity, currentLang);
                case "Legendary":
                    return GetTranslatedString(uiTranslations.LegendaryRarityTranslated, uiTranslations.LegendaryRarity, currentLang);
                default:
                    return "rarity_error_language";
            }
        }
        public void DisplayUpgradeSucess()
        {
            string translatedMessage = GetTranslatedString(uiTranslations.successLevelUpTranslated, uiTranslations.successLevelUp, currentLang);
            DisplayStatusMessage(translatedMessage, Color.green);
        }
        public void DisplayAlreadyMaxLevel()
        {
            string translatedMessage = GetTranslatedString(uiTranslations.errorAlreadyMaxLevelTranslated, uiTranslations.errorAlreadyMaxLevel, currentLang);
            DisplayStatusMessage(translatedMessage, Color.red);
        }
        public void DisplayNotEnoughCurrency()
        {
            string translatedMessage = GetTranslatedString(uiTranslations.errorNotEnoughCurrencyTranslated, uiTranslations.errorNotEnoughCurrency, currentLang);
            DisplayStatusMessage(translatedMessage, Color.red);
        }
        public void DisplayNotEnoughExp()
        {
            string translatedMessage = GetTranslatedString(uiTranslations.errorNotEnoughExpTranslated, uiTranslations.errorNotEnoughExp, currentLang);
            DisplayStatusMessage(translatedMessage, Color.red);
        }
        public void DisplayStatusMessage(string message, Color color)
        {
            if (errorMessageUpgrade == null) return;
            errorMessageUpgrade.text = message;
            errorMessageUpgrade.color = color;
            StartCoroutine(HideStatusMessageAfterDelay(errorMessageUpgrade, 1f));
        }

        private IEnumerator HideStatusMessageAfterDelay(TextMeshProUGUI _errorMessage, float delay)
        {
            yield return new WaitForSeconds(delay);
            _errorMessage.text = string.Empty;
        }

        private void OnClickSelectCharacter()
        {
            ChangeSelectedCharacter(currentDetailCharacterId);
        }

        private async void OnClickLevelUp()
        {
            CharacterData cd = GetCharacterDataById(currentDetailCharacterId);
            if (cd == null) return;

            RequestResult result = await BackendManager.Service.UpdateCharacterLevelUP(currentDetailCharacterId);

            if (result.Reason == "0")
            {
                DisplayAlreadyMaxLevel();
                return;
            }

            if (result.Reason == "1")
            {
                DisplayNotEnoughExp();
                return;
            }
            if (result.Reason == "2")
            {
                DisplayNotEnoughCurrency();
                return;
            }

            string _levelUp = GetTranslatedString(uiTranslations.successLevelUpTranslated, uiTranslations.successLevelUp, currentLang);
     
            DisplayStatusMessage($"{_levelUp} : {PlayerSave.GetCharacterLevel(currentDetailCharacterId)}", Color.green);
            ShowCharacterDetails(cd.characterId);
            EventBus.Publish(new MenuCharacterUpgradedEvent(tempCharacterModel));

            OnCharacterUpgrade?.Invoke();
            tempCharacterModel.OnUpgrade?.Invoke();
        }

        private void OnClickBackToCharacterSelection()
        {
            DestroyTemporaryModel();
            if (characterSelection != null) characterSelection.SetActive(true);
            if (characterDetails != null) characterDetails.SetActive(false);
        }

        private void DestroyTemporaryModel()
        {
            if (tempCharacterContainer == null) return;
            for (int i = tempCharacterContainer.childCount - 1; i >= 0; i--)
                Destroy(tempCharacterContainer.GetChild(i).gameObject);
        }

        public void UpdateFavouriteSelected(int characterId)
        {
            int favId = PlayerSave.GetFavouriteCharacter();
            if (favouriteSelected != null)
                favouriteSelected.gameObject.SetActive(characterId == favId);
        }

        /// <summary>
        /// Applies a text gradient based on rarity to a TextMeshProUGUI component.
        /// </summary>
        private void ApplyRarityGradient(TextMeshProUGUI tmpText, CharacterRarity rarity)
        {
            string rarityStr = rarity.ToString();
            TextRarityGradient gradient;
            switch (rarityStr)
            {
                case "Common":
                    gradient = commonTextColor; break;
                case "Uncommon":
                    gradient = uncommonTextColor; break;
                case "Rare":
                    gradient = rareTextColor; break;
                case "Epic":
                    gradient = epicTextColor; break;
                case "Legendary":
                    gradient = legendaryTextColor; break;
                default:
                    gradient = commonTextColor; break;
            }
            if (tmpText != null)
            {
                tmpText.enableVertexGradient = true;
                tmpText.colorGradient = new VertexGradient(gradient.topColor, gradient.topColor, gradient.botColor, gradient.botColor);
            }
        }

        private Sprite GetRarityBanner(CharacterRarity rarity)
        {
            switch (rarity)
            {
                case CharacterRarity.Common: return commonBanner;
                case CharacterRarity.Uncommon: return uncommonBanner;
                case CharacterRarity.Rare: return rareBanner;
                case CharacterRarity.Epic: return epicBanner;
                case CharacterRarity.Legendary: return legendaryBanner;
                default: return commonBanner;
            }
        }

        public string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        public string GetTranslatedString(DescriptionTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        public async void TestAddMasteryExp(int exp)
        {
            var result = await BackendManager.Service
                .TestAddCharacterMasteryExpAsync(currentDetailCharacterId, exp);

            if (result.Success)
            {
                UpdateDetailPanel(GetCharacterDataById(currentDetailCharacterId));
            }
            else
            {
                Debug.LogWarning($"Falha ao dar Mastery EXP: {result.Reason}");
            }
        }

        public async void TestAddCharacterExp(int exp)
        {
            var result = await BackendManager.Service
                .TestAddCharacterExpAsync(currentDetailCharacterId, exp);

            if (result.Success)
            {
                UpdateDetailPanel(GetCharacterDataById(currentDetailCharacterId));
            }
            else
            {
                Debug.LogWarning($"Falha ao dar EXP: {result.Reason}");
            }
        }
    }
}
