using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles saving and loading of purchased items data using PlayerPrefs.
    /// This class does not apply any logic or validation. Only persists data.
    /// </summary>
    public static partial class PlayerSave
    {
        private static List<PurchasedCharacter> purchasedCharacters = new();
        private static List<PurchasedIcon> purchasedIcons = new();
        private static List<PurchasedFrame> purchasedFrames = new();
        private static List<PurchasedShopItem> purchasedShopItems = new();
        private static List<PurchasedInventoryItem> purchasedInventoryItems = new();

        private const string KeyCharacters = "PurchasedCharacters";
        private const string KeyIcons = "PurchasedIcons";
        private const string KeyFrames = "PurchasedFrames";
        private const string KeyShopItems = "PurchasedShopItems";
        private const string KeyInventoryItems = "PurchasedInventoryItems";

        /// <summary>
        /// Loads all purchased items from PlayerPrefs into memory.
        /// </summary>
        public static void LoadAllPurchased()
        {
            purchasedCharacters = LoadList<PurchasedCharacterList>(KeyCharacters)?.purchasedCharacters ?? new();
            purchasedIcons = LoadList<PurchasedIconList>(KeyIcons)?.purchasedIcons ?? new();
            purchasedFrames = LoadList<PurchasedFrameList>(KeyFrames)?.purchasedFrames ?? new();
            purchasedShopItems = LoadList<PurchasedShopItemList>(KeyShopItems)?.purchasedShopItems ?? new();
            purchasedInventoryItems = LoadList<PurchasedInventoryItemList>(KeyInventoryItems)?.purchasedInventoryItems ?? new();
        }

        /// <summary>
        /// Saves all purchased items to PlayerPrefs.
        /// </summary>
        public static void SaveAllPurchased()
        {
            SaveList(KeyCharacters, new PurchasedCharacterList(purchasedCharacters));
            SaveList(KeyIcons, new PurchasedIconList(purchasedIcons));
            SaveList(KeyFrames, new PurchasedFrameList(purchasedFrames));
            SaveList(KeyShopItems, new PurchasedShopItemList(purchasedShopItems));
            SaveList(KeyInventoryItems, new PurchasedInventoryItemList(purchasedInventoryItems));
            PlayerPrefs.Save();
        }

        public static void ClearAllPurchased()
        {
            purchasedCharacters.Clear();
            purchasedIcons.Clear();
            purchasedFrames.Clear();
            purchasedShopItems.Clear();
            purchasedInventoryItems.Clear();
        }

        // Setters – used only by logic handlers like OfflinePurchasesHandler
        public static void SetCharacters(List<PurchasedCharacter> list) => purchasedCharacters = list;
        public static void SetIcons(List<PurchasedIcon> list) => purchasedIcons = list;
        public static void SetFrames(List<PurchasedFrame> list) => purchasedFrames = list;
        public static void SetShopItems(List<PurchasedShopItem> list) => purchasedShopItems = list;
        public static void SetInventoryItems(List<PurchasedInventoryItem> list) => purchasedInventoryItems = list;

        // Adders – for appending from external logic (not validated here)
        public static void AddCharacter(PurchasedCharacter c) { purchasedCharacters.Add(c); SaveAllPurchased(); }
        public static void AddIcon(PurchasedIcon i) { purchasedIcons.Add(i); SaveAllPurchased(); }
        public static void AddFrame(PurchasedFrame f) { purchasedFrames.Add(f); SaveAllPurchased(); }
        public static void AddShopItem(PurchasedShopItem s) { purchasedShopItems.Add(s); SaveAllPurchased(); }
        public static void AddInventoryItem(PurchasedInventoryItem i) { purchasedInventoryItems.Add(i); SaveAllPurchased(); }

        // Getters
        public static List<PurchasedCharacter> GetCharacters() => new(purchasedCharacters);
        public static List<PurchasedIcon> GetIcons() => new(purchasedIcons);
        public static List<PurchasedFrame> GetFrames() => new(purchasedFrames);
        public static List<PurchasedShopItem> GetShopItems() => new(purchasedShopItems);
        public static List<PurchasedInventoryItem> GetInventoryItems() => new(purchasedInventoryItems);

        // Checkers
        public static bool IsCharacterPurchased(string id) => purchasedCharacters.Any(c => c.characterId == id);
        public static bool IsIconPurchased(string id) => purchasedIcons.Any(i => i.iconId == id);
        public static bool IsFramePurchased(string id) => purchasedFrames.Any(f => f.frameId == id);
        public static bool IsShopItemPurchased(string id) => purchasedShopItems.Any(s => s.itemId == id);
        public static bool IsInventoryItemPurchased(string id) => purchasedInventoryItems.Any(i => i.itemId == id);

        // Internal JSON helpers
        private static T LoadList<T>(string key) where T : class
        {
            var json = PlayerPrefs.GetString(key, null);
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<T>(json);
        }

        private static void SaveList<T>(string key, T list)
        {
            string json = JsonUtility.ToJson(list);
            PlayerPrefs.SetString(key, json);
        }

        public static void WipePurchasedCache()
        {
            PlayerPrefs.DeleteKey("PurchasedCharacters");
            PlayerPrefs.DeleteKey("PurchasedIcons");
            PlayerPrefs.DeleteKey("PurchasedFrames");
            PlayerPrefs.DeleteKey("PurchasedShopItems");
            PlayerPrefs.DeleteKey("PurchasedInventoryItems");
            ClearAllPurchased();
            PlayerPrefs.Save();
        }
    }
}
