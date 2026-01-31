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
    public static class FirebaseRewardsHandler
    {
        private static FirebaseAuth Auth => FirebaseAuthHandler.Auth;
        private static FirebaseFirestore Db => FirebaseAuthHandler.Firestore;

        private const bool MIRROR_CURRENCIES_TO_ROOT = false;
        private static DocumentReference PlayerDoc(string uid) =>
            Db.Collection("Players").Document(uid);
        private static DocumentReference CurrencyDoc(string uid, string coinId) =>
            PlayerDoc(uid).Collection("Currencies").Document(coinId);
        private static DocumentReference ProgressDoc(string uid, string id) =>
            PlayerDoc(uid).Collection("Progress").Document(id);

        private static void GrantNonCurrencyLocally(BattlePassItem reward)
        {
            switch (reward.rewardType)
            {
                case BattlePassItem.RewardType.CharacterReward:
                    foreach (var ch in reward.characterData)
                        if (!PlayerSave.IsCharacterPurchased(ch.characterId.ToString()))
                            PlayerSave.AddCharacter(new PurchasedCharacter { characterId = ch.characterId.ToString() });
                    break;
                case BattlePassItem.RewardType.IconReward:
                    if (reward.iconReward != null && !PlayerSave.IsIconPurchased(reward.iconReward.iconId.ToString()))
                        PlayerSave.AddIcon(new PurchasedIcon { iconId = reward.iconReward.iconId.ToString() });
                    break;
                case BattlePassItem.RewardType.FrameReward:
                    if (reward.frameReward != null && !PlayerSave.IsFramePurchased(reward.frameReward.frameId.ToString()))
                        PlayerSave.AddFrame(new PurchasedFrame { frameId = reward.frameReward.frameId.ToString() });
                    break;
                case BattlePassItem.RewardType.InventoryItemReward:
                    if (reward.inventoryItems != null)
                        foreach (var inv in reward.inventoryItems)
                            if (!PlayerSave.IsInventoryItemPurchased(inv.itemId))
                                PlayerSave.AddInventoryItem(new PurchasedInventoryItem
                                {
                                    uniqueItemGuid = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant(),
                                    itemId = inv.itemId,
                                    itemLevel = 0,
                                    upgrades = new Dictionary<int, int>()
                                });
                    break;
            }
        }

        public static async UniTask<RequestResult> ClaimDailyRewardAsync(int dayIndex, BattlePassItem reward)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = Auth.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");
            if (reward == null) return RequestResult.Fail("0");

            Dictionary<string, int> newBalances = null;

            try
            {
                newBalances = await Db.RunTransactionAsync<Dictionary<string, int>>(async tr =>
                {
                    var dailyDoc = ProgressDoc(uid, "DailyRewards");
                    var pDoc = PlayerDoc(uid);

                    var dSnap = await tr.GetSnapshotAsync(dailyDoc);

                    var claimed = new HashSet<int>();
                    DateTime? firstDate = null;

                    if (dSnap.Exists)
                    {
                        if (dSnap.ContainsField("claimedRewards"))
                            foreach (var o in dSnap.GetValue<IEnumerable<object>>("claimedRewards"))
                                claimed.Add(Convert.ToInt32(o));
                        if (dSnap.ContainsField("firstClaimDate"))
                        {
                            var ts = dSnap.GetValue<Timestamp>("firstClaimDate");
                            firstDate = ts.ToDateTime();
                        }
                    }

                    if (claimed.Contains(dayIndex))
                        throw new InvalidOperationException("ALREADY");

                    string coinId = null;
                    int addAmount = 0;
                    DocumentReference cDoc = null;
                    DocumentSnapshot cSnap = null;
                    int curAmount = 0;

                    if (reward.rewardType == BattlePassItem.RewardType.CurrencyReward && reward.currencyReward != null)
                    {
                        coinId = reward.currencyReward.currency.coinID;
                        addAmount = Mathf.Max(0, reward.currencyReward.amount);
                        cDoc = CurrencyDoc(uid, coinId);
                        cSnap = await tr.GetSnapshotAsync(cDoc);
                        curAmount = (cSnap.Exists && cSnap.ContainsField("amount")) ? cSnap.GetValue<int>("amount") : 0;
                    }

                    claimed.Add(dayIndex);

                    tr.Set(dailyDoc, new Dictionary<string, object> {
                        { "firstClaimDate", firstDate.HasValue ? (object)Timestamp.FromDateTime(firstDate.Value.ToUniversalTime()) : Timestamp.FromDateTime(DateTime.UtcNow) },
                        { "lastClaimDate",  Timestamp.FromDateTime(DateTime.UtcNow) },
                        { "claimedRewards", new List<int>(claimed) }
                    }, SetOptions.MergeAll);

                    var result = new Dictionary<string, int>();
                    if (!string.IsNullOrEmpty(coinId))
                    {
                        int final = curAmount + addAmount;
                        if (!cSnap.Exists)
                            tr.Set(cDoc, new Dictionary<string, object> { { "initialAmount", final }, { "amount", final } }, SetOptions.MergeAll);
                        else
                            tr.Update(cDoc, new Dictionary<string, object> { { "amount", final } });

                        result[coinId] = final;
                    }
                    return result;
                });

                if (newBalances != null)
                    foreach (var kv in newBalances)
                        MonetizationManager.SetCurrency(kv.Key, kv.Value);

                var local = PlayerSave.GetDailyRewardsLocal();
                if (!local.claimedRewards.Contains(dayIndex)) local.claimedRewards.Add(dayIndex);
                if (local.claimedRewards.Count == 1 && local.firstClaimDate == default) local.firstClaimDate = DateTime.Now.Date;
                local.lastClaimDate = DateTime.Now.Date;
                PlayerSave.SetDailyRewardsLocal(local);

                int chBefore = PlayerSave.GetCharacters().Count;
                int icBefore = PlayerSave.GetIcons().Count;
                int frBefore = PlayerSave.GetFrames().Count;
                int siBefore = PlayerSave.GetShopItems().Count;
                var beforeGuids = new HashSet<string>(PlayerSave.GetInventoryItems().Select(i => i.uniqueItemGuid));

                GrantNonCurrencyLocally(reward);

                bool compatChanged =
                    PlayerSave.GetCharacters().Count != chBefore ||
                    PlayerSave.GetIcons().Count != icBefore ||
                    PlayerSave.GetFrames().Count != frBefore ||
                    PlayerSave.GetShopItems().Count != siBefore;

                var newItems = PlayerSave.GetInventoryItems()
                                .Where(i => !beforeGuids.Contains(i.uniqueItemGuid))
                                .ToList();

                await PersistGrantedEntitlementsAsync(uid, compatChanged, newItems);

                return RequestResult.Ok();
            }
            catch (InvalidOperationException ioe) when (ioe.Message == "ALREADY")
            {
                return RequestResult.Fail("0");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        public static async UniTask<RequestResult> ClaimNewPlayerRewardAsync(int dayIndex, BattlePassItem reward)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = Auth.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");
            if (reward == null) return RequestResult.Fail("0");

            Dictionary<string, int> newBalances = null;

            try
            {
                newBalances = await Db.RunTransactionAsync<Dictionary<string, int>>(async tr =>
                {
                    var npDoc = ProgressDoc(uid, "NewPlayerRewards");
                    var pDoc = PlayerDoc(uid);

                    var nSnap = await tr.GetSnapshotAsync(npDoc);

                    var claimed = new HashSet<int>();
                    DateTime? accountCreation = null;

                    if (nSnap.Exists)
                    {
                        if (nSnap.ContainsField("claimedRewards"))
                            foreach (var o in nSnap.GetValue<IEnumerable<object>>("claimedRewards"))
                                claimed.Add(Convert.ToInt32(o));
                        if (nSnap.ContainsField("accountCreationDate"))
                        {
                            var ts = nSnap.GetValue<Timestamp>("accountCreationDate");
                            accountCreation = ts.ToDateTime();
                        }
                    }

                    if (claimed.Contains(dayIndex))
                        throw new InvalidOperationException("ALREADY");

                    string coinId = null;
                    int addAmount = 0;
                    DocumentReference cDoc = null;
                    DocumentSnapshot cSnap = null;
                    int curAmount = 0;

                    if (reward.rewardType == BattlePassItem.RewardType.CurrencyReward && reward.currencyReward != null)
                    {
                        coinId = reward.currencyReward.currency.coinID;
                        addAmount = Mathf.Max(0, reward.currencyReward.amount);
                        cDoc = CurrencyDoc(uid, coinId);
                        cSnap = await tr.GetSnapshotAsync(cDoc);
                        curAmount = (cSnap.Exists && cSnap.ContainsField("amount")) ? cSnap.GetValue<int>("amount") : 0;
                    }

                    claimed.Add(dayIndex);

                    tr.Set(npDoc, new Dictionary<string, object> {
                        { "accountCreationDate", accountCreation.HasValue ? (object)Timestamp.FromDateTime(accountCreation.Value.ToUniversalTime()) : Timestamp.FromDateTime(DateTime.UtcNow) },
                        { "lastClaimDate",       Timestamp.FromDateTime(DateTime.UtcNow) },
                        { "claimedRewards",      new List<int>(claimed) }
                    }, SetOptions.MergeAll);

                    var result = new Dictionary<string, int>();
                    if (!string.IsNullOrEmpty(coinId))
                    {
                        int final = curAmount + addAmount;
                        if (!cSnap.Exists)
                            tr.Set(cDoc, new Dictionary<string, object> { { "initialAmount", final }, { "amount", final } }, SetOptions.MergeAll);
                        else
                            tr.Update(cDoc, new Dictionary<string, object> { { "amount", final } });

                        result[coinId] = final;
                    }
                    return result;
                });

                if (newBalances != null)
                    foreach (var kv in newBalances)
                        MonetizationManager.SetCurrency(kv.Key, kv.Value);

                var local = PlayerSave.GetNewPlayerRewardsLocal();
                if (!local.claimedRewards.Contains(dayIndex)) local.claimedRewards.Add(dayIndex);
                if (local.accountCreationDate == default) local.accountCreationDate = DateTime.Now.Date;
                local.lastClaimDate = DateTime.Now.Date;
                PlayerSave.SetNewPlayerRewardsLocal(local);

                int chBefore = PlayerSave.GetCharacters().Count;
                int icBefore = PlayerSave.GetIcons().Count;
                int frBefore = PlayerSave.GetFrames().Count;
                int siBefore = PlayerSave.GetShopItems().Count;
                var beforeGuids = new HashSet<string>(PlayerSave.GetInventoryItems().Select(i => i.uniqueItemGuid));

                GrantNonCurrencyLocally(reward);
                return RequestResult.Ok();
            }
            catch (InvalidOperationException ioe) when (ioe.Message == "ALREADY")
            {
                return RequestResult.Fail("0");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        public static async UniTask<RequestResult> ApplyMapRewardsAsync(MapCompletionRewardData data)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = Auth.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var map = GameInstance.Singleton.GetMapInfoDataById(data.mapId);
            if (map == null) return RequestResult.Fail("map_not_found");

            try
            {
                var adds = new Dictionary<string, int>();
                foreach (var r in map.WinMapRewards)
                    if (r.currency != null && r.amount > 0)
                        adds[r.currency.coinID] = adds.TryGetValue(r.currency.coinID, out var cur) ? cur + r.amount : r.amount;

                Dictionary<string, int> newBalances = null;

                if (adds.Count > 0)
                {
                    newBalances = await Db.RunTransactionAsync<Dictionary<string, int>>(async tr =>
                    {
                        var pDoc = PlayerDoc(uid);

                        var cSnaps = new Dictionary<string, (DocumentReference doc, DocumentSnapshot snap, int cur)>();
                        foreach (var kv in adds)
                        {
                            var cDoc = CurrencyDoc(uid, kv.Key);
                            var cSnap = await tr.GetSnapshotAsync(cDoc);
                            int cur = (cSnap.Exists && cSnap.ContainsField("amount")) ? cSnap.GetValue<int>("amount") : 0;
                            cSnaps[kv.Key] = (cDoc, cSnap, cur);
                        }

                        var result = new Dictionary<string, int>();
                        foreach (var kv in adds)
                        {
                            var tuple = cSnaps[kv.Key];
                            int final = tuple.cur + Mathf.Max(0, kv.Value);

                            if (!tuple.snap.Exists)
                                tr.Set(tuple.doc, new Dictionary<string, object> { { "initialAmount", final }, { "amount", final } }, SetOptions.MergeAll);
                            else
                                tr.Update(tuple.doc, new Dictionary<string, object> { { "amount", final } });

                            return result;
                        }
                        return result;
                    });
                }

                if (newBalances != null)
                    foreach (var kv in newBalances)
                        MonetizationManager.SetCurrency(kv.Key, kv.Value);

                int chBefore = PlayerSave.GetCharacters().Count;
                int icBefore = PlayerSave.GetIcons().Count;
                int frBefore = PlayerSave.GetFrames().Count;
                int siBefore = PlayerSave.GetShopItems().Count;
                var beforeGuids = new HashSet<string>(PlayerSave.GetInventoryItems().Select(i => i.uniqueItemGuid));

                if (map.rewardType != MapRewardType.None)
                    GrantNonCurrencyLocally(OfflineRewardsHandler.CreateBattlePassItem(map));

                bool compatChanged =
                    PlayerSave.GetCharacters().Count != chBefore ||
                    PlayerSave.GetIcons().Count != icBefore ||
                    PlayerSave.GetFrames().Count != frBefore ||
                    PlayerSave.GetShopItems().Count != siBefore;

                var newItems = PlayerSave.GetInventoryItems()
                                         .Where(i => !beforeGuids.Contains(i.uniqueItemGuid))
                                         .ToList();

                await PersistGrantedEntitlementsAsync(uid, compatChanged, newItems);

                int accExp = map.WinMapRewards.Sum(w => Mathf.Max(0, w.accountExp));
                int charExp = map.WinMapRewards.Sum(w => Mathf.Max(0, w.characterExp));
                int charMastery = map.WinMapRewards.Sum(w => Mathf.Max(0, w.characterMasteryAmount));

                if (accExp > 0) await FirebaseExpHandler.AddAccountExpAsync(accExp);
                if (charExp > 0) await FirebaseExpHandler.AddCharacterExpAsync(data.characterId, charExp);
                if (charMastery > 0) await FirebaseExpHandler.AddCharacterMasteryExpAsync(data.characterId, charMastery);

                return RequestResult.Ok();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        public static async UniTask<RequestResult> RedeemCouponAsync(string code)
        {
            await FirebaseAuthHandler.EnsureInitializedAsync();
            var uid = Auth.CurrentUser?.UserId;
            if (uid == null) return RequestResult.Fail("invalid_credentials");

            var coupon = ResolveCouponByUserInput(code);
            if (coupon == null) return RequestResult.Fail("1");
            if (PlayerSave.IsCouponUsed(coupon.idCoupon)) return RequestResult.Fail("0");

            try
            {
                var balances = await Db.RunTransactionAsync<Dictionary<string, int>>(async tr =>
                {
                    var pDoc = PlayerDoc(uid);

                    var pSnap = await tr.GetSnapshotAsync(pDoc);
                    bool alreadyUsed =
                        pSnap.ContainsField("CouponsUsed") &&
                        pSnap.GetValue<Dictionary<string, object>>("CouponsUsed")
                             .TryGetValue(coupon.idCoupon, out var used) &&
                        used is bool b && b;
                    if (alreadyUsed) throw new InvalidOperationException("ALREADY_USED");

                    var cDoc = CurrencyDoc(uid, coupon.currencyRewardId);
                    var cSnap = await tr.GetSnapshotAsync(cDoc);
                    int cur = (cSnap.Exists && cSnap.ContainsField("amount")) ? cSnap.GetValue<int>("amount") : 0;

                    tr.Set(pDoc, new Dictionary<string, object> { { $"CouponsUsed.{coupon.idCoupon}", true } }, SetOptions.MergeAll);

                    int final = cur + Mathf.Max(0, coupon.currencyAmount);
                    if (!cSnap.Exists)
                        tr.Set(cDoc, new Dictionary<string, object> { { "initialAmount", final }, { "amount", final } }, SetOptions.MergeAll);
                    else
                        tr.Update(cDoc, new Dictionary<string, object> { { "amount", final } });

                    return new Dictionary<string, int> { { coupon.currencyRewardId, final } };
                });

                foreach (var kv in balances)
                   MonetizationManager.SetCurrency(kv.Key, kv.Value);

                PlayerSave.MarkCouponAsUsed(coupon.idCoupon);
                return RequestResult.Ok($"{coupon.currencyRewardId}|{coupon.currencyAmount}");
            }
            catch (InvalidOperationException ioe) when (ioe.Message == "ALREADY_USED")
            {
                return RequestResult.Fail("0");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return RequestResult.Fail("generic_error");
            }
        }

        private static CouponItem ResolveCouponByUserInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            var gi = GameInstance.Singleton;

            var byId = gi?.GetCouponItemById(input);
            if (byId != null) return byId;

            try
            {
                var list = gi?.couponData; 
                if (list != null)
                {
                    string n = input.Trim();
                    return list.FirstOrDefault(c =>
                        string.Equals(c.couponCode, n, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.idCoupon, n, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch { }
            return null;
        }

        private static List<int> ToIntList(IEnumerable<object> e)
        {
            var list = new List<int>();
            foreach (var o in e) { try { list.Add(Convert.ToInt32(o)); } catch { } }
            return list;
        }

       
        private static async UniTask PersistGrantedEntitlementsAsync(string uid, bool compatChanged, List<PurchasedInventoryItem> newItems)
        {
            if (!compatChanged && (newItems == null || newItems.Count == 0))
                return;

            var db = FirebaseAuthHandler.Firestore;
            var batch = db.StartBatch();

            if (compatChanged)
                FirebasePurchasesHandler.InvokeEnqueuePurchasedItemsCompat(batch, uid); // ver nota abaixo

            if (newItems != null && newItems.Count > 0)
                FirebasePurchasesHandler.InvokeEnqueueNewInventoryItems(batch, uid, newItems);

            await batch.CommitAsync();
        }



    }
}
#endif