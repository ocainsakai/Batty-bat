using System;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>Data stored when a character is purchased.</summary>
    [Serializable] public class PurchasedCharacter { public string characterId; }

    /// <summary>Data stored when an icon is purchased.</summary>
    [Serializable] public class PurchasedIcon { public string iconId; }

    /// <summary>Data stored when a frame is purchased.</summary>
    [Serializable] public class PurchasedFrame { public string frameId; }

    /// <summary>Data stored when a shop item is purchased.</summary>
    [Serializable] public class PurchasedShopItem { public string itemId; }

    /// <summary>Data stored when an inventory item is purchased.</summary>
    [Serializable]
    public class PurchasedInventoryItem
    {
        public string uniqueItemGuid;
        public string itemId;
        public int itemLevel;
        public Dictionary<int, int> upgrades;
    }

    /* ---------- Lightweight wrappers for JSON serialization ---------- */
    [Serializable] public class PurchasedCharacterList { public List<PurchasedCharacter> purchasedCharacters = new(); public PurchasedCharacterList(List<PurchasedCharacter> l) => purchasedCharacters = l; }
    [Serializable] public class PurchasedIconList { public List<PurchasedIcon> purchasedIcons = new(); public PurchasedIconList(List<PurchasedIcon> l) => purchasedIcons = l; }
    [Serializable] public class PurchasedFrameList { public List<PurchasedFrame> purchasedFrames = new(); public PurchasedFrameList(List<PurchasedFrame> l) => purchasedFrames = l; }
    [Serializable] public class PurchasedShopItemList { public List<PurchasedShopItem> purchasedShopItems = new(); public PurchasedShopItemList(List<PurchasedShopItem> l) => purchasedShopItems = l; }
    [Serializable] public class PurchasedInventoryItemList { public List<PurchasedInventoryItem> purchasedInventoryItems = new(); public PurchasedInventoryItemList(List<PurchasedInventoryItem> l) => purchasedInventoryItems = l; }
}
