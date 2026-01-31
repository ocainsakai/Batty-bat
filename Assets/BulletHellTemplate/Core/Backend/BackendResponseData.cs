using System.Collections.Generic;
using System;
using UnityEngine;

namespace BulletHellTemplate
{
    public class PurchaseResponse
    {
        public bool success;
        public string itemId;
        public int newCurrencyAmount;
        public Dictionary<string, int> packageRewards;      
        public List<ServerInventoryItem> inventoryItems;   
    }
    [Serializable]
    public class ServerInventoryItem
    {
        public string uniqueItemGuid { get; set; }
        public string itemId { get; set; }
        public int itemLevel { get; set; }
        public Dictionary<int, int> upgrades { get; set; }
    }
    [Serializable]
    public class InventoryItemResponse
    {
        public string uniqueItemGuid;
        public string itemId;
        public int itemLevel;
        public Dictionary<int, int> upgrades;
    }
    [Serializable]
    /// <summary>
    /// Response DTO from server containing full Battle Pass progress.
    /// </summary>
    public class BattlePassProgressResponse
    {
        public int currentXP;
        public int currentLevel;
        public bool isUnlocked;
        public Dictionary<string, bool> claimedRewards;
    }
}