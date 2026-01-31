using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public static class OfflineCharacterHandler
    {
        public static RequestResult TrySelectCharacterAsync(int cid)
        {
            CharacterData characterData = GameInstance.Singleton.GetCharacterDataById(cid);
            if (PlayerSave.IsCharacterPurchased(cid.ToString()) || characterData.CheckUnlocked)
            {
                PlayerSave.SetSelectedCharacter(cid);
                return RequestResult.Ok();
            }
            return RequestResult.Fail("0");
        }

        public static RequestResult UpdatePlayerCharacterFavouriteAsync(int cid)
        {
            CharacterData characterData = GameInstance.Singleton.GetCharacterDataById(cid);
            if (PlayerSave.IsCharacterPurchased(cid.ToString()) || characterData.CheckUnlocked)
            {
                PlayerSave.SetFavouriteCharacter(cid);
                return RequestResult.Ok();
            }
            return RequestResult.Fail("0");
        }

        public static RequestResult TryUnlockCharacterSkin(int cid, int skinId)
        {
            CharacterData characterData = GameInstance.Singleton.GetCharacterDataById(cid);
            string currencyId = characterData.characterSkins[skinId].unlockCurrencyId;
            int currencyAmount = MonetizationManager.GetCurrency(currencyId);
            int requiredAmount = characterData.characterSkins[skinId].unlockPrice;

            if (currencyAmount >= requiredAmount)
            {
                int newAmount = currencyAmount - requiredAmount;
                MonetizationManager.SetCurrency(currencyId, newAmount);

                List<int> unlockedSkins = PlayerSave.LoadCharacterUnlockedSkins(cid);
                if (!unlockedSkins.Contains(skinId))
                {
                    unlockedSkins.Add(skinId);
                }

                PlayerSave.SaveCharacterUnlockedSkins(cid, unlockedSkins);
                PlayerSave.SetCharacterSkin(cid, skinId);

                return RequestResult.Ok();
            }
            return RequestResult.Fail("0");
        }

        public static RequestResult TryUpgradeStat(
           StatUpgrade statUpgrade,
           CharacterStatsRuntime stats,
           int characterId)
        {
            // 1) current level and next level
            int currentLevel = PlayerSave.GetCharacterUpgradeLevel(characterId, statUpgrade.statType);
            int nextLevel = currentLevel + 1;

            // 2) fail if already maxed
            if (currentLevel >= statUpgrade.upgradeMaxLevel)
                return RequestResult.Fail("0");

            // 3) compute cost and check currency
            int cost = statUpgrade.GetUpgradeCost(nextLevel);
            int balance = MonetizationManager.GetCurrency(statUpgrade.currencyTag);
            if (balance < cost)
                return RequestResult.Fail("1");

            // 4) spend, apply and persist
            MonetizationManager.SetCurrency(statUpgrade.currencyTag, balance - cost);
            statUpgrade.ApplyUpgrade(stats, nextLevel);
            PlayerSave.SetCharacterUpgradeLevel(characterId, statUpgrade.statType, nextLevel);

            return RequestResult.Ok();
        }

        public static RequestResult UpdateCharacterSkin(int cid, int skinId)
        {
            CharacterData characterData = GameInstance.Singleton.GetCharacterDataById(cid);
            List<int> unlockedSkins = PlayerSave.LoadCharacterUnlockedSkins(cid);
            if (unlockedSkins.Contains(skinId) || characterData.characterSkins[skinId].isUnlocked)
            {
                PlayerSave.SetCharacterSkin(cid, skinId);
                return RequestResult.Ok();
            }
            return RequestResult.Fail("0");
        }

        public static RequestResult TryCharacterLevelUp(int cid)
        {
            CharacterData cd = GameInstance.Singleton.GetCharacterDataById(cid);
            int currentLevel = PlayerSave.GetCharacterLevel(cid);
            int currentExp = PlayerSave.GetCharacterCurrentExp(cid);
            int expNeeded = (currentLevel < cd.expPerLevel.Length) ? cd.expPerLevel[currentLevel] : 9999999;
            int costNeeded = (currentLevel < cd.upgradeCostPerLevel.Length) ? cd.upgradeCostPerLevel[currentLevel] : 9999999;
            int playerCurrency = MonetizationManager.GetCurrency(cd.currencyId);

            if (currentLevel >= cd.maxLevel) return RequestResult.Fail("0");

            if (currentExp < expNeeded) return RequestResult.Fail("1");

            if (playerCurrency < costNeeded) return RequestResult.Fail("2");

            int newValue = playerCurrency - costNeeded;
            MonetizationManager.SetCurrency(cd.currencyId, newValue);
            PlayerSave.SetCharacterCurrentExp(cd.characterId, currentExp - expNeeded);
            int newLevel = currentLevel + 1;
            PlayerSave.SetCharacterLevel(cd.characterId, newLevel);

            return RequestResult.Ok();
        }

        public static RequestResult SetCharacterSlotItem(
            int characterId,
            string slotName,
            string uniqueItemGuid)
        {
            // UNEQUIP
            if (string.IsNullOrEmpty(uniqueItemGuid))
            {
                PlayerSave.SetCharacterSlotItem(characterId, slotName, "");
                return RequestResult.Ok();
            }

            // Look-up purchased record
            var purchased = PlayerSave.GetInventoryItems()
                                      .Find(i => i.uniqueItemGuid == uniqueItemGuid);
            if (purchased == null) return RequestResult.Fail("0");

            // Optional: block duplicate equip in another character
            RemoveItemFromAllCharacters(uniqueItemGuid);

            PlayerSave.SetCharacterSlotItem(characterId, slotName, uniqueItemGuid);
            return RequestResult.Ok();
        }

        public static RequestResult DeletePurchasedItem(string uniqueItemGuid)
        {
            var purchased = PlayerSave.GetInventoryItems()
                                      .Find(i => i.uniqueItemGuid == uniqueItemGuid);
            if (purchased == null) return RequestResult.Fail("0");

            // Unequip from all characters
            RemoveItemFromAllCharacters(uniqueItemGuid);

            // Remove local upgrade key
            PlayerPrefs.DeleteKey($"{uniqueItemGuid}_level");

            PlayerSave.RemoveInventoryItem(uniqueItemGuid);

            PlayerPrefs.Save();
            return RequestResult.Ok();
        }
        private static void RemoveItemFromAllCharacters(string uniqueGuid)
        {
            var chars = GameInstance.Singleton.characterData;
            if (chars == null) return;

            foreach (var cd in chars)
            {
                var slots = cd.itemSlots;
                if (slots == null) continue;

                foreach (string slot in slots)
                {
                    string eq = PlayerSave.GetCharacterSlotItem(cd.characterId, slot);
                    if (eq == uniqueGuid)
                        PlayerSave.SetCharacterSlotItem(cd.characterId, slot, "");
                }

                if (cd.runeSlots != null)
                {
                    foreach (string slot in cd.runeSlots)
                    {
                        string eq = PlayerSave.GetCharacterSlotItem(cd.characterId, slot);
                        if (eq == uniqueGuid)
                            PlayerSave.SetCharacterSlotItem(cd.characterId, slot, "");
                    }
                }
            }
        }
    }
}
