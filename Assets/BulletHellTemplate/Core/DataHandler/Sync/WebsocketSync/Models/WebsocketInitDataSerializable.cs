using System.Collections.Generic;
using System;

namespace BulletHellTemplate
{


    [Serializable]
    public class InitDto
    {
        public ProfileDto profile;
        public List<CurrencyDto> currencies;
        public OwnedDto owned;
        public List<PlayerCharacterDataDto> charactersData;
        public ProgressDto progress;
        public RewardsProgressInitDto rewards;
        public List<string> usedCoupons;
        public List<int> claimedMapRewards;
    }

    [Serializable]
    public class ProfileDto
    {
        public string displayName;
        public int selectedCharacterId;
        public int favouriteCharacterId;
        public string iconId;
        public string frameId;
        public int accountLevel;
        public int accountCurrentExp;
    }

    [Serializable]
    public class PlayerCharacterDataDto
    {
        public int characterId;
        public int level;
        public int currentExp;
        public int masteryLevel;
        public int masteryCurrentExp;
        public string selectedSkinId;
        public Dictionary<string, string> slots;
        public Dictionary<string, int> upgrades;
        public List<int> unlockedSkins;
    }

    [Serializable]
    public class CurrencyDto
    {
        public string code;
        public int balance;
    }

    [System.Serializable]
    public class InventoryDto
    {
        public string uniqueItemGuid;
        public string templateItemId;
        public int itemLevel;
    }

    [Serializable]
    public class OwnedDto
    {
        public List<string> shopItemIds;
        public List<string> iconIds;
        public List<string> frameIds;
        public List<int> characterIds;
        public List<InventoryDto> inventoryItems;
    }
   
   

    [Serializable]
    class PurchaseResponseDto
    {
        public bool success;
        public string reason;
        public List<InventoryDto> inventory;
    }
    [Serializable]
    public class BattlePassResponseDto
    {
        public bool success;
        public string reason;
        public List<InventoryDto> inventory;
    }
    [Serializable]
    public class QuestProgressDto
    {
        public int questId;
        public int progress;
        public bool completed;
        public string questType;
    }

    [Serializable]
    public class BattlePassProgressDto
    {
        public int level;
        public int currentXp;
        public bool premium;
        public ulong claimedBits;
    }

    [Serializable]
    public class BattlePassMetaDto
    {
        public int season;
        public string seasonStartUtc;
        public int durationDays;
    }

    [Serializable]
    public class ProgressDto
    {
        public List<QuestProgressDto> quests;
        public List<int> unlockedMaps;
        public int score;
        public BattlePassProgressDto battlePass;
        public BattlePassMetaDto battlePassMeta;
    }

    [Serializable]
    public class RewardsProgressInitDto
    {
        public NewPlayerProgress newPlayer;
        public DailyProgress daily;

        [Serializable]
        public class NewPlayerProgress
        {
            public string joinedAt;
            public string lastClaimed;  
            public int claimedBits;   
        }

        [Serializable]
        public class DailyProgress
        {
            public string firstClaimed;
            public string lastClaimed;
            public string claimedBits;
        }
    }

    [Serializable]
    public class MapClaimServerDto
    {
        public bool success;
        public string reason;
        public MapClaimRewardDto reward;
    }

    [Serializable]
    public class MapClaimRewardDto
    {
        public int mapId;
        public bool granted;
        public string reason;

        public CurrencyGrantDto[] currencyGrants;
        public int accountExp;
        public int characterExp;
        public int masteryExp;
        public MapClaimSpecialDto special;
    }

    [Serializable]
    public class CurrencyGrantDto
    {
        public string currencyId;
        public int amount;
    }

    [Serializable]
    public class MapClaimSpecialDto
    {
        public string type;
        public string id;
    }
}