using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    [CreateAssetMenu(fileName = "NewMapInfoData", menuName = "BulletHellTemplate/MapData/Map Info Data", order = 52)]
    public class MapInfoData : ScriptableObject
    {
        [Tooltip("The scene associated with the map.")]
        public string scene;

        [Tooltip("The unique identifier for the map.")]
        public int mapId;

        [Tooltip("Indicates whether the map is unlocked.")]
        public bool isUnlocked;

        [Tooltip("The name of the map.")]
        public string mapName;
        public NameTranslatedByLanguage[] mapNameTranslated;

        [Tooltip("A brief description of the map.")]
        public string mapDescription;
        public DescriptionTranslatedByLanguage[] mapDescriptionTranslated;

        [Tooltip("The preview image of the map.")]
        public Sprite mapPreviewImage;

        [Tooltip("The minimap display image.")]
        public Sprite mapMinimapImage;

        [Tooltip("The difficulty rating of the map."), Range(1, 5)]
        public int difficultyRating;

        [Space, Header("Reward Settings")]
        [Tooltip("Enable to provide rewards when completing this map for the first time.")]
        public bool isRewardOnCompleteFirstTime;

        [Tooltip("List of currency rewards for completing the map.")]
        public List<MapRewards> WinMapRewards;

        [Tooltip("Type of special reward for completing the map.")]
        public MapRewardType rewardType;

        [Tooltip("Icon reward item.")]
        public IconItem iconItem;

        [Tooltip("Frame reward item.")]
        public FrameItem frameItem;

        [Tooltip("Character data reward.")]
        public CharacterData characterData;

        public InventoryItem inventoryItem;

        [Space, Header("Map Access Settings")]
        [Tooltip("Requires specific currency to access this map.")]
        public bool isNeedCurrency;

        [Tooltip("Currency type needed to enter the map.")]
        public Currency currency;

        [Tooltip("Amount of currency required.")]
        public int amount;

        [Tooltip("Allows skipping this map requirement when completing previous maps.")]
        public bool canIgnoreMap;

        [Space, Header("Event Map Settings")]
        [Tooltip("Enable if this is a time-limited event map.")]
        public bool isEventMap;

        [Tooltip("Unique identifier for the event in Firebase.")]
        public string eventIdName;
    }

    [System.Serializable]
    public class MapRewards
    {
        [Tooltip("Type of currency to reward.")]
        public Currency currency;

        [Tooltip("Amount of currency to award.")]
        public int amount;

        public int accountExp;

        public int characterExp;

        public int characterMasteryAmount;

        
    }

    public enum MapRewardType
    {
        None,
        Icon,
        Frame,
        Character,
        InventoryItem
    }
}
