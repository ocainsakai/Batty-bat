#if FIREBASE

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;

namespace BulletHellTemplate
{
    /// <summary>
    /// Purchase/entitlement writes backed by Firestore with local-only validation.
    /// - No reads on purchase: uses PlayerSave as source-of-truth.
    /// - Writes currencies under /Players/{uid}/Currencies/{coinId} and mirrors
    ///   a "Currencies" map inside /Players/{uid} for 1-read loads later.
    /// - Persists entitlements in PlayerSave; writes legacy compatibility docs
    ///   (/PurchasedItems/{Characters|Icons|Frames|ShopItems}) and new items under
    ///   /PurchasedItems/Items/List/{guid}.
    /// </summary>
    public static class FirebasePurchasesHandler
    {
        private static FirebaseAuth Auth => FirebaseAuthHandler.Auth;
        private static FirebaseFirestore Db => FirebaseAuthHandler.Firestore;
        private static string UidOrNull() => Auth?.CurrentUser?.UserId;

        private const bool MIRROR_CURRENCIES_TO_ROOT = false;
        private static DocumentReference PlayerDoc(string uid) =>
            Db.Collection("Players").Document(uid);
        private static DocumentReference CurrencyDoc(string uid, string coinId) =>
            PlayerDoc(uid).Collection("Currencies").Document(coinId);
        private static CollectionReference PurchasedRoot(string uid) =>
            PlayerDoc(uid).Collection("PurchasedItems");
        private static DocumentReference PurchasedCharactersDoc(string uid) =>
            PurchasedRoot(uid).Document("Characters");
        private static DocumentReference PurchasedIconsDoc(string uid) =>
            PurchasedRoot(uid).Document("Icons");
        private static DocumentReference PurchasedFramesDoc(string uid) =>
            PurchasedRoot(uid).Document("Frames");
        private static DocumentReference PurchasedShopItemsDoc(string uid) =>
            PurchasedRoot(uid).Document("ShopItems");
        private static DocumentReference PurchasedItemsContainerDoc(string uid) =>
            PurchasedRoot(uid).Document("Items");
        private static CollectionReference PurchasedItemsListCol(string uid) =>
            PurchasedItemsContainerDoc(uid).Collection("List");

        /// <summary>Enqueues currency updates to batch (subcollection docs + root Currencies map).</summary>
        private static void EnqueueCurrenciesToBatch(WriteBatch batch, string uid, Dictionary<string, int> changes)
        {
            var rootMap = new Dictionary<string, object>();
            foreach (var kv in changes)
            {
                rootMap[kv.Key] = kv.Value;
                batch.Set(CurrencyDoc(uid, kv.Key), new Dictionary<string, object> { { "amount", kv.Value } }, SetOptions.MergeAll);
            }            
        }

        /// <summary>Enqueues legacy compatibility docs (arrays of JSON strings) to batch.</summary>
        private static void EnqueuePurchasedItemsCompat(WriteBatch batch, string uid)
        {
            List<string> ch = PlayerSave.GetCharacters().Select(c => JsonUtility.ToJson(c)).ToList();
            List<string> ic = PlayerSave.GetIcons().Select(i => JsonUtility.ToJson(i)).ToList();
            List<string> fr = PlayerSave.GetFrames().Select(f => JsonUtility.ToJson(f)).ToList();
            List<string> si = PlayerSave.GetShopItems().Select(s => JsonUtility.ToJson(s)).ToList();

            // subdocs (compat)
            batch.Set(PurchasedCharactersDoc(uid), new Dictionary<string, object> { { "Characters", ch } });
            batch.Set(PurchasedIconsDoc(uid), new Dictionary<string, object> { { "Icons", ic } });
            batch.Set(PurchasedFramesDoc(uid), new Dictionary<string, object> { { "Frames", fr } });
            batch.Set(PurchasedShopItemsDoc(uid), new Dictionary<string, object> { { "ShopItems", si } });         
        }

        /// <summary>Enqueues new purchased inventory items to batch (legacy layout).</summary>
        private static void EnqueueNewInventoryItems(WriteBatch batch, string uid, List<PurchasedInventoryItem> items)
        {
            foreach (var it in items)
            {
                var data = new Dictionary<string, object>{
            { "uniqueItemGuid", it.uniqueItemGuid },
            { "itemId", it.itemId },
            { "itemLevel", it.itemLevel },
            { "itemUpgrades", it.upgrades?.ToDictionary(k=>k.Key.ToString(), v=>(object)v.Value)
                               ?? new Dictionary<string, object>() }
        };

                batch.Set(PurchasedItemsContainerDoc(uid),
                   new Dictionary<string, object> { { "exists", true } },
                   SetOptions.MergeAll);

                batch.Set(PurchasedItemsListCol(uid).Document(it.uniqueItemGuid), data, SetOptions.MergeAll);
            }

        }

        private static List<Dictionary<string, object>> BuildInventoryMirrorFromLocal()
        {
            var list = new List<Dictionary<string, object>>();
            foreach (var it in PlayerSave.GetInventoryItems())
            {
                list.Add(new Dictionary<string, object>{
            { "uniqueItemGuid", it.uniqueItemGuid },
            { "itemId", it.itemId },
            { "itemLevel", it.itemLevel },
            { "itemUpgrades", it.upgrades?.ToDictionary(k=>k.Key.ToString(), v=>(object)v.Value)
                                ?? new Dictionary<string, object>() }
        });
            }
            return list;
        }

        /// <summary>
        /// Purchases a ShopItem using local PlayerSave state (no reads).
        /// - Currency packages: increases local balances and writes all coins in one batch.
        /// - Item bundles: checks local balance, deducts, grants entitlements locally
        ///   and writes: currency, compat docs and new inventory items in one batch.
        /// </summary>
        public static async UniTask<RequestResult> PurchaseShopItemAsync(ShopItem shopItem)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");
            if (shopItem == null) return RequestResult.Fail("0");

            if (shopItem.isCurrencyPackage)
            {
                var changed = new Dictionary<string, int>();
                foreach (var reward in shopItem.currencyRewards)
                {
                    var coinId = reward.currency.coinID;
                    var cur = MonetizationManager.GetCurrency(coinId);
                    var next = cur + Math.Max(0, reward.amount);
                    MonetizationManager.SetCurrency(coinId, next);
                    changed[coinId] = next;
                }

                var batch = Db.StartBatch();
                EnqueueCurrenciesToBatch(batch, uid, changed);
                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            else
            {
                int balance = MonetizationManager.GetCurrency(shopItem.currency);
                if (balance < shopItem.price) return RequestResult.Fail("1");

                int newBalance = balance - shopItem.price;
                MonetizationManager.SetCurrency(shopItem.currency, newBalance);

                bool compatChanged = false;

                compatChanged |= OfflinePurchasesHandler.TryUnlockShopItem(shopItem.itemId);
                foreach (var c in shopItem.characterData)
                    compatChanged |= OfflinePurchasesHandler.TryUnlockCharacter(c.characterId.ToString());
                foreach (var ic in shopItem.icons)
                    compatChanged |= OfflinePurchasesHandler.TryUnlockIcon(ic.iconId.ToString());
                foreach (var fr in shopItem.frames)
                    compatChanged |= OfflinePurchasesHandler.TryUnlockFrame(fr.frameId.ToString());

                var beforeGuids = new HashSet<string>(PlayerSave.GetInventoryItems().Select(i => i.uniqueItemGuid));
                foreach (var inv in shopItem.inventoryItems)
                    OfflinePurchasesHandler.TryUnlockInventoryItem(inv.itemId);
                var newItems = PlayerSave.GetInventoryItems().Where(i => !beforeGuids.Contains(i.uniqueItemGuid)).ToList();

                var batch = Db.StartBatch();
                EnqueueCurrenciesToBatch(batch, uid, new Dictionary<string, int> { { shopItem.currency, newBalance } });
                if (compatChanged) EnqueuePurchasedItemsCompat(batch, uid);
                if (newItems.Count > 0) EnqueueNewInventoryItems(batch, uid, newItems);
                await batch.CommitAsync();

                return RequestResult.Ok();
            }
        }

        /// <summary>
        /// Purchases a single inventory item using local state (no reads).
        /// Deducts currency, creates local item and writes currency + item in one batch.
        /// </summary>
        public static async UniTask<RequestResult> PurchaseInventoryItemAsync(string itemId, int price, string currencyId)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            if (PlayerSave.IsInventoryItemPurchased(itemId))
                return RequestResult.Fail("2");

            int balance = MonetizationManager.GetCurrency(currencyId);
            if (balance < price) return RequestResult.Fail("1");

            int newBalance = balance - price;
            MonetizationManager.SetCurrency(currencyId, newBalance);

            OfflinePurchasesHandler.TryUnlockInventoryItem(itemId);
            var created = PlayerSave.GetInventoryItems().Last(); 

            var batch = Db.StartBatch();
            EnqueueCurrenciesToBatch(batch, uid, new Dictionary<string, int> { { currencyId, newBalance } });
            EnqueueNewInventoryItems(batch, uid, new List<PurchasedInventoryItem> { created });
            await batch.CommitAsync();

            return RequestResult.Ok();
        }

        /// <summary>
        /// Upgrades an inventory item using local state:
        /// - Validates cost and success locally.
        /// - Updates local level and currency.
        /// - Writes currency and item level in one batch (no reads).
        /// </summary>
        public static async UniTask<RequestResult> UpgradeInventoryItemAsync(string uniqueItemGuid, InventoryItem inventorySO)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = UidOrNull();
            if (uid == null) return RequestResult.Fail("invalid_credentials");
            if (inventorySO == null) return RequestResult.Fail("1");

            int curLevel = PlayerSave.GetItemUpgradeLevel(uniqueItemGuid);
            int maxLevel = inventorySO.itemUpgrades.Count;
            if (curLevel >= maxLevel) return RequestResult.Fail("0");

            var up = inventorySO.itemUpgrades[curLevel];
            int cost = up.upgradeCosts;
            string currency = up.currencyTag;

            int balance = MonetizationManager.GetCurrency(currency);
            if (balance < cost) return RequestResult.Fail("1");

            bool success = UnityEngine.Random.value <= up.successRate;
            int newLevel = success ? curLevel + 1 : (up.decreaseLevelIfFail && curLevel > 0 ? curLevel - 1 : curLevel);

            int newBalance = balance - cost;
            MonetizationManager.SetCurrency(currency, newBalance);
            PlayerSave.SetItemUpgradeLevel(uniqueItemGuid, newLevel);

            var batch = Db.StartBatch();
            EnqueueCurrenciesToBatch(batch, uid, new Dictionary<string, int> { { currency, newBalance } });
            var itemDoc = PurchasedItemsListCol(uid).Document(uniqueItemGuid);
            batch.Set(itemDoc, new Dictionary<string, object> { { "itemLevel", newLevel } }, SetOptions.MergeAll);

            await batch.CommitAsync();

            if (success) return RequestResult.Ok();
            if (up.decreaseLevelIfFail && curLevel > 0) return RequestResult.Fail("3");
            return RequestResult.Fail("2");
        }

        /// <summary>
        /// Unlocks Battle Pass premium using local state only (no reads).
        /// Validates local balance, deducts, flips the local premium flag,
        /// then writes currency and IsUnlocked to Firestore in a single batch.
        /// Error codes: "0" insufficient funds, "1" already unlocked.
        /// </summary>
        public static async UniTask<RequestResult> TryUnlockBattlePassPremiumAsync()
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = FirebaseAuthHandler.Auth?.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var gi = GameInstance.Singleton;
            string currencyId = gi != null ? gi.battlePassCurrencyID : "DM";
            int price = gi != null ? gi.battlePassPrice : 1000;

            if (PlayerSave.CheckBattlePassPremiumUnlocked())
                return RequestResult.Fail("1");

            int balance = MonetizationManager.GetCurrency(currencyId);
            if (balance < price)
                return RequestResult.Fail("0");

            int newBalance = balance - price;
            MonetizationManager.SetCurrency(currencyId, newBalance);
            PlayerSave.BattlePassPremiumUnlock();

            try
            {
                var db = FirebaseAuthHandler.Firestore;
                var batch = db.StartBatch();

                EnqueueCurrenciesToBatch(batch, uid, new Dictionary<string, int> { { currencyId, newBalance } });

                // Flag premium in /Players/{uid}/Progress/BattlePass
                var passDoc = PlayerDoc(uid).Collection("Progress").Document("BattlePass");
                batch.Set(passDoc, new Dictionary<string, object> { { "IsUnlocked", true } }, SetOptions.MergeAll);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Deletes a purchased inventory item using local state (no reads).
        /// Removes it locally and deletes the server document in one batch.
        /// Returns "1" if the GUID is not found locally.
        /// </summary>
        public static async UniTask<RequestResult> DeletePurchasedInventoryItemAsync(string uniqueItemGuid)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = FirebaseAuthHandler.Auth?.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var list = PlayerSave.GetInventoryItems();
            var found = list.Find(i => i.uniqueItemGuid == uniqueItemGuid);
            if (found == null) return RequestResult.Fail("1");

            // Remove local
            list.Remove(found);
            PlayerSave.SetInventoryItems(list);
            PlayerSave.SaveAllPurchased();

            try
            {
                var db = FirebaseAuthHandler.Firestore;
                var batch = db.StartBatch();

                var itemDoc = PurchasedItemsListCol(uid).Document(uniqueItemGuid);
                batch.Delete(itemDoc);

                await batch.CommitAsync();
                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        /// <summary>
        /// Claims a Battle Pass reward with local-only validation.
        /// Applies entitlements locally (characters/icons/frames/inventory/currency),
        /// then writes currency changes, compat docs and new inventory items in one batch,
        /// plus marks the reward as claimed remotely.
        /// </summary>
        public static async UniTask<RequestResult> ClaimBattlePassRewardAsync(BattlePassItem reward)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = FirebaseAuthHandler.Auth?.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");
            if (reward == null) return RequestResult.Fail("0");

            var currencyChanges = new Dictionary<string, int>();
            bool compatChanged = false;

            var beforeGuids = new HashSet<string>(PlayerSave.GetInventoryItems().Select(i => i.uniqueItemGuid));

            switch (reward.rewardType)
            {
                case BattlePassItem.RewardType.CharacterReward:
                    foreach (var ch in reward.characterData)
                        compatChanged |= OfflinePurchasesHandler.TryUnlockCharacter(ch.characterId.ToString());
                    break;

                case BattlePassItem.RewardType.IconReward:
                    compatChanged |= OfflinePurchasesHandler.TryUnlockIcon(reward.iconReward.iconId.ToString());
                    break;

                case BattlePassItem.RewardType.FrameReward:
                    compatChanged |= OfflinePurchasesHandler.TryUnlockFrame(reward.frameReward.frameId.ToString());
                    break;

                case BattlePassItem.RewardType.InventoryItemReward:
                    foreach (var inv in reward.inventoryItems)
                        OfflinePurchasesHandler.TryUnlockInventoryItem(inv.itemId);
                    break;

                case BattlePassItem.RewardType.CurrencyReward:
                    {
                        string cid = reward.currencyReward.currency.coinID;
                        int add = Math.Max(0, reward.currencyReward.amount);
                        int cur = MonetizationManager.GetCurrency(cid);
                        int next = cur + add;
                        MonetizationManager.SetCurrency(cid, next);
                        currencyChanges[cid] = next;
                    }
                    break;
            }

            var newItems = PlayerSave.GetInventoryItems()
                                     .Where(i => !beforeGuids.Contains(i.uniqueItemGuid))
                                     .ToList();

            // Cloud save (1 batch)
            try
            {
                var db = FirebaseAuthHandler.Firestore;
                var batch = db.StartBatch();

                if (currencyChanges.Count > 0)
                    EnqueueCurrenciesToBatch(batch, uid, currencyChanges);

                if (compatChanged)
                    EnqueuePurchasedItemsCompat(batch, uid);

                if (newItems.Count > 0)
                    EnqueueNewInventoryItems(batch, uid, newItems);

                var rewardsDoc = PlayerDoc(uid).Collection("Progress").Document("BattlePassRewards");
                batch.Set(rewardsDoc, new Dictionary<string, object> { { reward.passId, true } }, SetOptions.MergeAll);

                await batch.CommitAsync();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }

            // Marca local
            PlayerSave.MarkBattlePassReward(reward.passId);
            return RequestResult.Ok();
        }

        internal static void InvokeEnqueuePurchasedItemsCompat(WriteBatch batch, string uid)
            => EnqueuePurchasedItemsCompat(batch, uid);

        internal static void InvokeEnqueueNewInventoryItems(WriteBatch batch, string uid, List<PurchasedInventoryItem> items)
            => EnqueueNewInventoryItems(batch, uid, items);
    }
}
#endif