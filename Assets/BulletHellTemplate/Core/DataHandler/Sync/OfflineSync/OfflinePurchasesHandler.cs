using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles offline logic for unlocking and verifying purchased items.
    /// Responsible for applying rules like avoiding duplicates, assigning GUIDs, and updating PlayerSave.
    /// </summary>
    public static class OfflinePurchasesHandler
    {
        public static RequestResult PurchaseShopItem(ShopItem shopItem)
        {
            if (shopItem == null)
                return RequestResult.Fail("0");

            int balance = MonetizationManager.GetCurrency(shopItem.currency);
            if (balance < shopItem.price)
                return RequestResult.Fail("1");

            if (PlayerSave.IsShopItemPurchased(shopItem.itemId))
                return RequestResult.Fail("2");

            MonetizationManager.SetCurrency(shopItem.currency, balance - shopItem.price, pushToBackend: false);

            if (!shopItem.isCurrencyPackage)
            {
                TryUnlockShopItem(shopItem.itemId);
                foreach (var c in shopItem.characterData) TryUnlockCharacter(c.characterId.ToString());
                foreach (var ic in shopItem.icons) TryUnlockIcon(ic.iconId.ToString());
                foreach (var fr in shopItem.frames) TryUnlockFrame(fr.frameId.ToString());
                foreach (var iv in shopItem.inventoryItems) TryUnlockInventoryItem(iv.itemId);
                return RequestResult.Ok();
            }

            foreach (var c in shopItem.currencyRewards) MonetizationManager.SetCurrency(c.currency.coinID, c.amount);
            return RequestResult.Ok();
        }

        public static RequestResult PurchaseInventoryItem(string itemId, int price, string currencyId)
        {
            int balance = MonetizationManager.GetCurrency(currencyId);
            if (balance < price)
                return RequestResult.Fail("1");

            if (PlayerSave.IsInventoryItemPurchased(itemId))
                return RequestResult.Fail("2");

            MonetizationManager.SetCurrency(currencyId, balance - price, pushToBackend: false);
            TryUnlockInventoryItem(itemId);
            return RequestResult.Ok();
        }

        public static RequestResult ClaimBattlePassReward(BattlePassItem reward)
        {
            if (reward == null) return RequestResult.Fail("0");

            bool ok = false;

            switch (reward.rewardType)
            {
                case BattlePassItem.RewardType.CharacterReward:
                    ok = true;
                    foreach (var ch in reward.characterData)
                        ok &= TryUnlockCharacter(ch.characterId.ToString());
                    break;

                case BattlePassItem.RewardType.IconReward:
                    ok = TryUnlockIcon(reward.iconReward.iconId.ToString());
                    break;

                case BattlePassItem.RewardType.FrameReward:
                    ok = TryUnlockFrame(reward.frameReward.frameId.ToString());
                    break;

                case BattlePassItem.RewardType.InventoryItemReward:
                    ok = true;
                    foreach (var inv in reward.inventoryItems)
                        ok &= TryUnlockInventoryItem(inv.itemId);
                    break;

                case BattlePassItem.RewardType.CurrencyReward:
                    string cid = reward.currencyReward.currency.coinID;
                    int amount = reward.currencyReward.amount;
                    MonetizationManager.SetCurrency(
                        cid,
                        MonetizationManager.GetCurrency(cid) + amount,
                        pushToBackend: false);
                    ok = true;
                    break;

                default: ok = false; break;
            }

            if (ok) PlayerSave.MarkBattlePassReward(reward.passId);
            return ok ? RequestResult.Ok() : RequestResult.Fail("Reward fail");
        }
        public static RequestResult TryUnlockBattlePassPremium()
        {
            string currencyId = GameInstance.Singleton.battlePassCurrencyID;
            int price = GameInstance.Singleton.battlePassPrice;
            int balance = MonetizationManager.GetCurrency(currencyId);
            if (balance < price) return RequestResult.Fail("0");
            if (PlayerSave.CheckBattlePassPremiumUnlocked()) return RequestResult.Fail("1");

            MonetizationManager.SetCurrency(currencyId, balance - price, pushToBackend: false);
            PlayerSave.BattlePassPremiumUnlock();
            return RequestResult.Ok();
        }

        public static RequestResult TryUpgradeItem(string uniqueItemGuid, InventoryItem inventorySO)
        {
            // Look-up purchased record
            var purchased = PlayerSave.GetInventoryItems()
                                    .Find(i => i.uniqueItemGuid == uniqueItemGuid);
            if (purchased == null) return RequestResult.Fail("1");     // Treat as insufficient – record missing

            // ScriptableObject data
            var so = GameInstance.Singleton.GetInventoryItemById(inventorySO.itemId);
            if (so == null) return RequestResult.Fail("1");

            int currentLevel = PlayerSave.GetItemUpgradeLevel(uniqueItemGuid);
            int maxLevel = so.itemUpgrades.Count;

            if (currentLevel >= maxLevel)
                return RequestResult.Fail("0");                         // Max level reached

            var upgradeData = so.itemUpgrades[currentLevel];
            int cost = upgradeData.upgradeCosts;
            string currency = upgradeData.currencyTag;
            float successPct = upgradeData.successRate;
            bool decOnFail = upgradeData.decreaseLevelIfFail;

            int balance = MonetizationManager.GetCurrency(currency);
            if (balance < cost)
                return RequestResult.Fail("1");                         // Not enough currency

            // Deduct currency (no push to backend needed in offline layer)
            MonetizationManager.SetCurrency(currency, balance - cost, pushToBackend: false);

            bool success = UnityEngine.Random.value <= successPct;

            if (success)
            {
                int newLevel = currentLevel + 1;
                ApplyLevelChange(uniqueItemGuid, purchased, newLevel);
                return RequestResult.Ok();                              // Success (Reason null)
            }

            // Failure path
            if (decOnFail && currentLevel > 0)
            {
                int newLevel = currentLevel - 1;
                ApplyLevelChange(uniqueItemGuid, purchased, newLevel);
                return RequestResult.Fail("3");                         // Level decreased
            }

            return RequestResult.Fail("2");                             // Failed, level unchanged
        }

        /// <summary>
        /// Persists the new level both to PlayerSave and to the cached purchased item list.
        /// </summary>
        private static void ApplyLevelChange(string guid, PurchasedInventoryItem purchased, int newLevel)
        {
            PlayerSave.SetItemUpgradeLevel(guid, newLevel);
            purchased.itemLevel = newLevel;
        }

        /// <summary>
        /// Attempts to unlock a character. Returns true if added successfully.
        /// </summary>
        public static bool TryUnlockCharacter(string characterId)
        {
            if (PlayerSave.IsCharacterPurchased(characterId))
                return false;

            PlayerSave.AddCharacter(new PurchasedCharacter { characterId = characterId });
            return true;
        }

        /// <summary>
        /// Attempts to unlock an icon. Returns true if added successfully.
        /// </summary>
        public static bool TryUnlockIcon(string iconId)
        {
            if (PlayerSave.IsIconPurchased(iconId))
                return false;

            PlayerSave.AddIcon(new PurchasedIcon { iconId = iconId });
            return true;
        }

        /// <summary>
        /// Attempts to unlock a frame. Returns true if added successfully.
        /// </summary>
        public static bool TryUnlockFrame(string frameId)
        {
            if (PlayerSave.IsFramePurchased(frameId))
                return false;

            PlayerSave.AddFrame(new PurchasedFrame { frameId = frameId });
            return true;
        }

        /// <summary>
        /// Attempts to unlock a shop item. Returns true if added successfully.
        /// </summary>
        public static bool TryUnlockShopItem(string shopItemId)
        {
            if (PlayerSave.IsShopItemPurchased(shopItemId))
                return false;

            PlayerSave.AddShopItem(new PurchasedShopItem { itemId = shopItemId });
            return true;
        }

        /// <summary>
        /// Attempts to unlock an inventory item using a new generated GUID.
        /// Returns true if added successfully.
        /// </summary>
        public static bool TryUnlockInventoryItem(string itemId)
        {
            if (PlayerSave.IsInventoryItemPurchased(itemId))
                return false;

            var item = new PurchasedInventoryItem
            {
                uniqueItemGuid = GenerateShortID(5),
                itemId = itemId,
                itemLevel = 0,
                upgrades = new Dictionary<int, int>()
            };

            PlayerSave.AddInventoryItem(item);
            return true;
        }

        /// <summary>
        /// Creates a short alphanumeric ID used for unique inventory GUIDs.
        /// </summary>
        private static string GenerateShortID(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            System.Random random = new();
            char[] buffer = new char[length];

            for (int i = 0; i < length; i++)
                buffer[i] = chars[random.Next(chars.Length)];

            return new string(buffer);
        }        
    }
}
