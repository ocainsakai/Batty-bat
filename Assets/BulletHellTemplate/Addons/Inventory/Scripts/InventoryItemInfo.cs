using BulletHellTemplate;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BulletHellTemplate
{
    public class InventoryItemInfo : MonoBehaviour
    {
        [Header("Item UI Elements")]
        public Image icon;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        [Header("Upgrade UI Elements")]
        public TextMeshProUGUI levelText;
        public Image currencyUpgradeIcon;
        public Image upgradeProgressBar;
        public float progressDuration;
        public TextMeshProUGUI upgradeCostText;
        public TextMeshProUGUI successRateText;
        public TextMeshProUGUI upgradeResult;
        public Button upgradeButton;

        [Header("UI Settings")]
        public Color baseStatsColor;
        public Color bonusStatsColor;
        public Color sucessColor;
        public Color failColor;

        [Header("Translated Messages")]
        public string sucessMessage;
        public NameTranslatedByLanguage[] sucessMessageTranslated;
        public string failMessage;
        public NameTranslatedByLanguage[] failMessageTranslated;
        public string sucessRateString;
        public NameTranslatedByLanguage[] sucessRateStringTranslated;
        public string maxLevelString;
        public NameTranslatedByLanguage[] maxLevelStringTranslated;

        [Header("Rarity Frame")]
        public Image frame;
        public Sprite commonFrame;
        public Sprite uncommonFrame;
        public Sprite rareFrame;
        public Sprite epicFrame;
        public Sprite legendaryFrame;

        [Header("Item Stats UI Elements")]
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI hpRegenText;
        public TextMeshProUGUI hpLeechText;
        public TextMeshProUGUI mpText;
        public TextMeshProUGUI mpRegenText;
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI attackSpeedText;
        public TextMeshProUGUI cooldownReductionText;
        public TextMeshProUGUI criticalRateText;
        public TextMeshProUGUI criticalDamageMultiplierText;
        public TextMeshProUGUI defenseText;
        public TextMeshProUGUI shieldText;
        public TextMeshProUGUI moveSpeedText;
        public TextMeshProUGUI collectRangeText;

        private InventoryItem currentItem;
        private string currentUniqueGuid; // The unique ID for this purchased item instance

        /// <summary>
        /// Opens the item information UI, displaying item details such as characterStatsComponent, upgrade levels, and costs.
        /// Receives both the inventory ScriptableObject and the unique GUID for the purchased instance.
        /// </summary>
        /// <param name="item">The inventory item (scriptable) to display.</param>
        /// <param name="uniqueGuid">The purchased instance unique GUID.</param>
        public void OpenItemInfo(InventoryItem item, string uniqueGuid)
        {
            currentItem = item;
            currentUniqueGuid = uniqueGuid;

            if (icon != null)
                icon.sprite = item.itemIcon;

            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            title.text = GetTranslatedString(item.titleTranslatedByLanguage, item.title, currentLang);
            description.text = GetTranslatedDescriptionString(item.descriptionTranslatedByLanguage, item.description, currentLang);

            // Get the level of this specific item instance
            int currentLevel = InventorySave.GetItemUpgradeLevel(uniqueGuid);

            if (currentLevel >= item.itemUpgrades.Count)
            {
                levelText.text = GetTranslatedString(maxLevelStringTranslated, maxLevelString, currentLang);
                upgradeCostText.text = GetTranslatedString(maxLevelStringTranslated, maxLevelString, currentLang);
                successRateText.text = "";
                upgradeButton.interactable = false;
                currencyUpgradeIcon.gameObject.SetActive(false);
            }
            else
            {
                levelText.text = $"Lv: {currentLevel}";
                upgradeButton.interactable = true;
                currencyUpgradeIcon.gameObject.SetActive(true);

                var nextUpgrade = item.itemUpgrades[currentLevel];
                upgradeCostText.text = $"{nextUpgrade.upgradeCosts}";
                currencyUpgradeIcon.sprite = MonetizationManager.Singleton.GetCurrencyIcon(nextUpgrade.currencyTag);

                SetSuccessRateText(nextUpgrade.successRate);
            }

            SetRarityFrame(item);
            DisplayImportantStats(item, currentLevel);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Updates the item info UI after an upgrade with the new level data.
        /// </summary>
        /// <param name="item">The inventory item ScriptableObject being updated.</param>
        /// <param name="newLevel">The new upgrade level of the item.</param>
        public void UpdateItemInfo(InventoryItem item, int newLevel)
        {
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();

            if (newLevel >= item.itemUpgrades.Count)
            {
                levelText.text = newLevel + " " + GetTranslatedString(maxLevelStringTranslated, maxLevelString, currentLang);
                upgradeButton.interactable = false;
                upgradeCostText.text = GetTranslatedString(maxLevelStringTranslated, maxLevelString, currentLang);
                successRateText.text = "";
                currencyUpgradeIcon.gameObject.SetActive(false);
            }
            else
            {
                levelText.text = $"Lv: {newLevel}";
                var nextUpgrade = item.itemUpgrades[newLevel];
                upgradeCostText.text = $"{nextUpgrade.upgradeCosts}";
                currencyUpgradeIcon.sprite = MonetizationManager.Singleton.GetCurrencyIcon(nextUpgrade.currencyTag);
                SetSuccessRateText(nextUpgrade.successRate);
                upgradeButton.interactable = true;
                currencyUpgradeIcon.gameObject.SetActive(true);
            }

            DisplayImportantStats(item, newLevel);
        }

        /// <summary>
        /// Called by the upgrade button. Triggers the upgrade process in the UIInventoryMenu,
        /// using the current item's unique GUID.
        /// </summary>
        public void OnClickUpgrade()
        {
            UIInventoryMenu.Singleton.UpgradeItem(currentUniqueGuid, currentItem, this);
        }

        /// <summary>
        /// Sets the success rate text with color feedback based on the value.
        /// </summary>
        private void SetSuccessRateText(float successRate)
        {
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            string successRateLabel = GetTranslatedString(sucessRateStringTranslated, sucessRateString, currentLang);

            float percentage = successRate * 100f;
            successRateText.text = $"{successRateLabel}: {percentage:F1}%";

            if (percentage > 70) successRateText.color = Color.green;
            else if (percentage >= 40) successRateText.color = Color.yellow;
            else successRateText.color = Color.red;
        }

        /// <summary>
        /// Displays the item's characterStatsComponent: base value plus a bonus increment colored in 'bonusStatsColor'.
        /// The bonus is based on summing the statIncreasePercentagePerLevel for all levels up to currentLevel.
        /// </summary>
        private void DisplayImportantStats(InventoryItem item, int currentLevel)
        {
            if (item == null) return;
            CharacterStats stats = item.itemStats;

            float totalUpgradePercentage = 0f;
            for (int i = 0; i < currentLevel; i++)
            {
                totalUpgradePercentage += item.itemUpgrades[i].statIncreasePercentagePerLevel;
            }

            // Helper function to build a "base + bonus" text with colors
            string BuildStatText(float baseValue)
            {
                float bonusValue = baseValue * totalUpgradePercentage;
                return $"<color=#{ColorUtility.ToHtmlStringRGBA(baseStatsColor)}>{baseValue}</color> " +
                       $"+ <color=#{ColorUtility.ToHtmlStringRGBA(bonusStatsColor)}>{bonusValue:F1}</color>";
            }

            if (hpText != null) hpText.text = BuildStatText(stats.baseHP);
            if (hpRegenText != null) hpRegenText.text = BuildStatText(stats.baseHPRegen);
            if (hpLeechText != null) hpLeechText.text = BuildStatText(stats.baseHPLeech);
            if (mpText != null) mpText.text = BuildStatText(stats.baseMP);
            if (mpRegenText != null) mpRegenText.text = BuildStatText(stats.baseMPRegen);
            if (damageText != null) damageText.text = BuildStatText(stats.baseDamage);
            if (attackSpeedText != null) attackSpeedText.text = BuildStatText(stats.baseAttackSpeed);
            if (cooldownReductionText != null) cooldownReductionText.text = BuildStatText(stats.baseCooldownReduction);
            if (criticalRateText != null) criticalRateText.text = BuildStatText(stats.baseCriticalRate);
            if (criticalDamageMultiplierText != null) criticalDamageMultiplierText.text = BuildStatText(stats.baseCriticalDamageMultiplier);
            if (defenseText != null) defenseText.text = BuildStatText(stats.baseDefense);
            if (shieldText != null) shieldText.text = BuildStatText(stats.baseShield);
            if (moveSpeedText != null) moveSpeedText.text = BuildStatText(stats.baseMoveSpeed);
            if (collectRangeText != null) collectRangeText.text = BuildStatText(stats.baseCollectRange);
        }

        /// <summary>
        /// Sets the frame sprite based on the item's rarity.
        /// </summary>
        private void SetRarityFrame(InventoryItem item)
        {
            if (frame == null) return;

            switch (item.rarity)
            {
                case Rarity.Common:
                    frame.sprite = commonFrame; break;
                case Rarity.Uncommon:
                    frame.sprite = uncommonFrame; break;
                case Rarity.Rare:
                    frame.sprite = rareFrame; break;
                case Rarity.Epic:
                    frame.sprite = epicFrame; break;
                case Rarity.Legendary:
                    frame.sprite = legendaryFrame; break;
            }
        }

        /// <summary>
        /// Coroutine to animate an upgrade progress bar and display a success/fail message.
        /// </summary>
        public IEnumerator UpgradeProgressRoutine(bool isSuccess, float duration)
        {
            upgradeProgressBar.fillAmount = 0f;
            float timer = 0f;
            while (timer < progressDuration)
            {
                timer += Time.deltaTime;
                upgradeProgressBar.fillAmount = Mathf.Clamp01(timer / progressDuration);
                yield return null;
            }
            upgradeProgressBar.fillAmount = 0f;

            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            if (isSuccess)
            {
                string successMsg = GetTranslatedString(sucessMessageTranslated, sucessMessage, currentLang);
                StartCoroutine(showUpgradeResult(successMsg, sucessColor, duration));
            }
            else
            {
                string failMsg = GetTranslatedString(failMessageTranslated, failMessage, currentLang);
                StartCoroutine(showUpgradeResult(failMsg, failColor, duration));
            }
        }

        /// <summary>
        /// Displays a temporary upgrade result message.
        /// </summary>
        public IEnumerator showUpgradeResult(string message, Color _color, float duration)
        {
            upgradeResult.color = _color;
            upgradeResult.text = message;
            yield return new WaitForSeconds(duration);
            upgradeResult.text = "";
        }

        /// <summary>
        /// Gets a translated string if available in translations; otherwise returns the fallback.
        /// </summary>
        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        /// <summary>
        /// Gets a translated description if available; otherwise returns fallback.
        /// </summary>
        private string GetTranslatedDescriptionString(DescriptionTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }
    }
}