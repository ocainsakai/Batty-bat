using System;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// ScriptableObject representing a Battle Pass Item.
    /// Contains item title, description, translations, icon, and reward details.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBattlePassItem", menuName = "BattlePass/Item", order = 51)]
    public class BattlePassItem : ScriptableObject
    {
        /// <summary>
        /// Unique identifier for the Battle Pass item.
        /// </summary>
        public string passId;

        /// <summary>
        /// Fallback item title (in English).
        /// </summary>
        public string itemTitle;

        /// <summary>
        /// Translated item titles.
        /// </summary>
        public NameTranslatedByLanguage[] itemTitleTranslated;

        /// <summary>
        /// Fallback item description (in English).
        /// </summary>
        public string itemDescription;

        /// <summary>
        /// Translated item descriptions.
        /// </summary>
        public DescriptionTranslatedByLanguage[] itemDescriptionTranslated;

        /// <summary>
        /// Icon of the item.
        /// </summary>
        public Sprite itemIcon;

        /// <summary>
        /// Type of reward (Character, Currency, Icon, Frame, InventoryItem).
        /// </summary>
        public RewardType rewardType;

        /// <summary>
        /// Reward tier: Free or Paid.
        /// </summary>
        public RewardTier rewardTier;

        [Header("Character Reward")]
        public CharacterData[] characterData;

        [Header("Currency Reward")]
        public CurrencyReward currencyReward;

        [Header("Icon Reward")]
        public IconItem iconReward;

        [Header("Frame Reward")]
        public FrameItem frameReward;

        [Header("Items Reward")]
        public InventoryItem[] inventoryItems;

        /// <summary>
        /// Reward types enumeration.
        /// </summary>
        public enum RewardType
        {
            CharacterReward,
            CurrencyReward,
            IconReward,
            FrameReward,
            InventoryItemReward
        }

        /// <summary>
        /// Reward tier enumeration.
        /// </summary>
        public enum RewardTier
        {
            Free,
            Paid
        }

        /// <summary>
        /// Retrieves the translated item title for the specified language.
        /// Falls back to the default English title if translation is not available.
        /// </summary>
        /// <param name="currentLang">The current language identifier.</param>
        /// <returns>The translated title if available; otherwise, the fallback title.</returns>
        public string GetTranslatedTitle(string currentLang)
        {
            if (itemTitleTranslated != null)
            {
                foreach (var trans in itemTitleTranslated)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return itemTitle;
        }

        /// <summary>
        /// Retrieves the translated item description for the specified language.
        /// Falls back to the default English description if translation is not available.
        /// </summary>
        /// <param name="currentLang">The current language identifier.</param>
        /// <returns>The translated description if available; otherwise, the fallback description.</returns>
        public string GetTranslatedDescription(string currentLang)
        {
            if (itemDescriptionTranslated != null)
            {
                foreach (var trans in itemDescriptionTranslated)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return itemDescription;
        }
    }
}
