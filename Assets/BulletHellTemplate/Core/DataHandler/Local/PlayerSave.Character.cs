using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BulletHellTemplate
{
    public static partial class PlayerSave
    {
        // ─────────────────────────────────────────────────── Keys
        private const string KEY_SELECTED_CHAR = "PLAYERSELECTEDCHARACTER_";
        private const string KEY_FAV_CHAR = "PLAYERFAVOURITECHARACTER_";
        private const string KEY_MASTERY_LVL = "CHARACTERMASTERYLEVEL_";
        private const string KEY_MASTERY_EXP = "CHARACTERMASTERYCURRENTEXP_";
        private const string KEY_LEVEL = "CHARACTERLEVEL_";
        private const string KEY_CUR_EXP = "CHARACTERCURRENTEXP_";
        private const string KEY_SLOT = "ITEMSLOT_";            // +id+slot
        private const string KEY_ITEM_LVL = "ITEMLEVEL_";           // +guid
        private const string KEY_UPGRADE = "CHARACTERUPGRADELEVEL_";// +id+stat
        private const string KEY_SKIN = "CHARACTERSKIN_";
        private const string KEY_UNLOCKED_SKINS = "CHARACTERUNLOCKEDSKINS_";

        #region Selection & Favourite
        public static void SetSelectedCharacter(int id) =>
            SecurePrefs.SetEncryptedInt(KEY_SELECTED_CHAR, id);

        public static int GetSelectedCharacter()
        {
            if (SecurePrefs.HasKey(KEY_SELECTED_CHAR))
                return SecurePrefs.GetDecryptedInt(KEY_SELECTED_CHAR, 0);

            // fallback: first unlocked in the DB
            foreach (var c in GameInstance.Singleton.characterData)
                if (c.CheckUnlocked) return c.characterId;

            Debug.LogError("No unlocked characters available to select.");
            return 0;
        }

        public static void SetFavouriteCharacter(int id) =>
            SecurePrefs.SetEncryptedInt(KEY_FAV_CHAR, id);

        public static int GetFavouriteCharacter() =>
            SecurePrefs.GetDecryptedInt(KEY_FAV_CHAR, 0);
        #endregion

        #region Level / Experience / Mastery
        public static void SetCharacterLevel(int charId, int lvl) =>
            SecurePrefs.SetEncryptedInt(KEY_LEVEL + charId, lvl);

        public static int GetCharacterLevel(int charId) =>
            SecurePrefs.GetDecryptedInt(KEY_LEVEL + charId, 1);

        public static void SetCharacterCurrentExp(int charId, int exp) =>
            SecurePrefs.SetEncryptedInt(KEY_CUR_EXP + charId, exp);

        public static int GetCharacterCurrentExp(int charId) =>
            SecurePrefs.GetDecryptedInt(KEY_CUR_EXP + charId, 0);

        public static void SetCharacterMasteryLevel(int charId, int lvl) =>
            SecurePrefs.SetEncryptedInt(KEY_MASTERY_LVL + charId, lvl);

        public static int GetCharacterMasteryLevel(int charId) =>
            SecurePrefs.GetDecryptedInt(KEY_MASTERY_LVL + charId, 0);

        public static void SetCharacterCurrentMasteryExp(int charId, int exp) =>
            SecurePrefs.SetEncryptedInt(KEY_MASTERY_EXP + charId, exp);

        public static int GetCharacterCurrentMasteryExp(int charId) =>
            SecurePrefs.GetDecryptedInt(KEY_MASTERY_EXP + charId, 0);
        #endregion

        #region Equipment & Items
        public static void SetCharacterSlotItem(int charId, string slot, string guid)
        {
            string key = $"{KEY_SLOT}{charId}_{slot}";
            SecurePrefs.SetEncryptedString(key, guid);    
        }

        public static string GetCharacterSlotItem(int charId, string slot)
        {
            string key = $"{KEY_SLOT}{charId}_{slot}";
            return SecurePrefs.GetDecryptedString(key, string.Empty);
        }

        public static void SetItemUpgradeLevel(string guid, int lvl) =>
            SecurePrefs.SetEncryptedInt(KEY_ITEM_LVL + guid, lvl);

        public static int GetItemUpgradeLevel(string guid) =>
            SecurePrefs.GetDecryptedInt(KEY_ITEM_LVL + guid, 0);
        #endregion

        #region Skins
        public static void SetCharacterSkin(int charId, int skinIdx) =>
            SecurePrefs.SetEncryptedInt(KEY_SKIN + charId, skinIdx);

        public static int GetCharacterSkin(int charId) =>
            SecurePrefs.GetDecryptedInt(KEY_SKIN + charId, -1);

        public static void SaveCharacterUnlockedSkins(int charId, List<int> skins)
        {
            string json = JsonUtility.ToJson(new UnlockedSkinsData { unlockedSkins = skins });
            SecurePrefs.SetEncryptedString(KEY_UNLOCKED_SKINS + charId, json);
        }

        public static List<int> LoadCharacterUnlockedSkins(int charId)
        {
            string json = SecurePrefs.GetDecryptedString(KEY_UNLOCKED_SKINS + charId, string.Empty);
            if (string.IsNullOrEmpty(json)) return new();
            var data = JsonUtility.FromJson<UnlockedSkinsData>(json);
            return data?.unlockedSkins ?? new();
        }

        public static void RemoveInventoryItem(string uniqueGuid)
        {
            var item = purchasedInventoryItems.FirstOrDefault(i => i.uniqueItemGuid == uniqueGuid);
            if (item != null)
            {
                purchasedInventoryItems.Remove(item);
                SaveAllPurchased();
            }
        }

        public static void ClearCharacterSlot(int charId, string slot)
        {
            SetCharacterSlotItem(charId, slot, string.Empty);
        }

        public static void DeleteItemFromCharacterAndInventory(int charId, string slot, string uniqueGuid)
        {
            ClearCharacterSlot(charId, slot);
            RemoveInventoryItem(uniqueGuid);
        }

        #endregion

        #region Stat Upgrades
        public static void SaveInitialCharacterUpgradeLevel(int charId, StatType stat) =>
            SecurePrefs.SetEncryptedInt($"{KEY_UPGRADE}{charId}_{stat}", 0);

        public static void SetCharacterUpgradeLevel(int charId, StatType stat, int lvl) =>
            SecurePrefs.SetEncryptedInt($"{KEY_UPGRADE}{charId}_{stat}", lvl);

        public static int GetCharacterUpgradeLevel(int charId, StatType stat) =>
            SecurePrefs.GetDecryptedInt($"{KEY_UPGRADE}{charId}_{stat}");

        public static Dictionary<StatType, int> LoadAllCharacterUpgradeLevels(int charId)
        {
            var dict = new Dictionary<StatType, int>();
            foreach (StatType s in System.Enum.GetValues(typeof(StatType)))
                dict[s] = GetCharacterUpgradeLevel(charId, s);
            return dict;
        }

        public static void ResetCharacterUpgrades(int charId)
        {
            foreach (StatType s in System.Enum.GetValues(typeof(StatType)))
                SecurePrefs.DeleteKey($"{KEY_UPGRADE}{charId}_{s}");
        }
        #endregion

        [System.Serializable]
        private class UnlockedSkinsData { public List<int> unlockedSkins; }
    }
}
