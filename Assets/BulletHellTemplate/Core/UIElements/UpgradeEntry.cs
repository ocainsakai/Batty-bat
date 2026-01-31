using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BulletHellTemplate.Core.Events;

namespace BulletHellTemplate
{
    public class UpgradeEntry : MonoBehaviour
    {
        public TextMeshProUGUI upgradeNameText;
        public TextMeshProUGUI upgradeCostText;
        public TextMeshProUGUI upgradeLevelText;
        public Image upgradeIconImage;              
        public Button upgradeButton;

        private StatUpgrade statUpgrade;
        private CharacterStatsRuntime characterStats; 
        private int currentLevel;
        private int characterId;

        public string Level = "Level:";
        public NameTranslatedByLanguage[] LevelTranslated;
        public string MaxLevel = "Max Level";
        public NameTranslatedByLanguage[] MaxLevelTranslated;
        private string currentLang;
        /// <summary>
        /// Initializes the upgrade entry with the given stat upgrade and character characterStatsComponent.
        /// </summary>
        /// <param name="upgrade">The stat upgrade data.</param>
        /// <param name="stats">The character characterStatsComponent to apply the upgrade to.</param>
        /// <param name="level">The current level of the upgrade.</param>
        /// <param name="characterId">The ID of the character.</param>
        public void Initialize(StatUpgrade upgrade, Sprite upgradeIcon, CharacterStatsRuntime stats, int level, int characterId)
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            statUpgrade = upgrade;
            characterStats = stats;
            currentLevel = level;
            this.characterId = characterId;

            string _upgradeName = UICharacterMenu.Singleton.GetTranslatedString(upgrade.upgradeNameTranslated, upgrade.upgradeName, currentLang);
            string _level = UICharacterMenu.Singleton.GetTranslatedString(LevelTranslated, Level, currentLang);
            string _maxLevel = UICharacterMenu.Singleton.GetTranslatedString(MaxLevelTranslated, MaxLevel, currentLang);
            upgradeNameText.text = _upgradeName;
            upgradeLevelText.text = $"{_level} {currentLevel}/{statUpgrade.upgradeMaxLevel}";
            upgradeIconImage.sprite = upgradeIcon;  // Set the upgrade icon
            UpdateCostText(currentLevel + 1);
            upgradeButton.onClick.AddListener(OnUpgradeButtonClick);

            if (currentLevel >= statUpgrade.upgradeMaxLevel)
            {
                upgradeButton.interactable = false;
                upgradeCostText.text = _maxLevel;
            }
        }

        /// <summary>
        /// Updates the cost text based on the current level.
        /// </summary>
        /// <param name="nextLevel">The next level to which the upgrade will be applied.</param>
        private void UpdateCostText(int nextLevel)
        {
            string _maxLevel = UICharacterMenu.Singleton.GetTranslatedString(MaxLevelTranslated, MaxLevel, currentLang);
            if (nextLevel > statUpgrade.upgradeMaxLevel)
            {
                upgradeCostText.text = _maxLevel;
            }
            else
            {
                int cost = statUpgrade.GetUpgradeCost(nextLevel);
                upgradeCostText.text = $"{cost}";
            }
        }

        /// <summary>
        /// Handles the upgrade button click event.
        /// </summary>
        private async void OnUpgradeButtonClick()
        {
            string _level = UICharacterMenu.Singleton.GetTranslatedString(LevelTranslated, Level, currentLang);
            string _maxLevel = UICharacterMenu.Singleton.GetTranslatedString(MaxLevelTranslated, MaxLevel, currentLang);

            RequestResult result = await BackendManager.Service.UpdateCharacterStatUpgradeAsync(
                statUpgrade,
                characterStats,
                characterId
            );

            if (result.Success)
            {
                currentLevel++;
                upgradeLevelText.text = $"{_level} {currentLevel}/{statUpgrade.upgradeMaxLevel}";
                UpdateCostText(currentLevel + 1);

                if (currentLevel >= statUpgrade.upgradeMaxLevel)
                {
                    upgradeButton.interactable = false;
                    upgradeCostText.text = _maxLevel;
                }

                UICharacterMenu.Singleton.OnCharacterUpgrade.Invoke();
                EventBus.Publish(new MenuCharacterUpgradedEvent(UICharacterMenu.Singleton.tempCharacterModel));
                UICharacterMenu.Singleton.UpdateDetailedStats();
                UICharacterMenu.Singleton.DisplayUpgradeSucess();
            }
            else
            {
                if (result.Reason == "0")
                    UICharacterMenu.Singleton.DisplayAlreadyMaxLevel();
                else
                    UICharacterMenu.Singleton.DisplayNotEnoughCurrency();
            }

            UICharacterMenu.Singleton.LoadCharacterSkills(characterId);
        }
    }
}
