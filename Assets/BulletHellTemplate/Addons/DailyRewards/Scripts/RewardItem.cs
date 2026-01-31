using BulletHellTemplate;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{

    [CreateAssetMenu(fileName = "New Reward Item", menuName = "Rewards/Reward Item")]
    public class RewardItem : ScriptableObject
    {
        public string rewardId; // Unique ID of the reward
        public Sprite icon; // Icon to display for the reward
        public string title; // Title of the reward
        public NameTranslatedByLanguage[] titleTranslated;
        public string description; // Description of the reward
        public DescriptionTranslatedByLanguage[] descriptionTranslated;
        public RewardType rewardType; // Type of reward (currency, item, etc.)
        public int amount; // Amount for currency rewards (if applicable)

        [Header("Specific Rewards")]
        public List<Currency> currencyRewards; // List of currency rewards   
        public List<IconItem> iconRewards; // List of icon rewards
        public List<FrameItem> frameRewards; // List of frame rewards
        public List<CharacterData> characterRewards; // List of character rewards
        public List<InventoryItem> inventoryItems;
    }

    public enum RewardType
    {
        Currency,
        InventoryItem,
        Icon,
        Frame,
        Character
    }
}