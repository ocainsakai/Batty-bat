using Colyseus;
using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

namespace BulletHellTemplate
{
    public static class WebsocketDataLoadHandler
    {
        static async UniTask<InitDto> FetchInitialDataAsync(this WebsocketAuthHandler auth)
        {
            string raw = await auth.Http.Request("GET", "auth/init", null);
            Debug.Log($"[auth/init] raw response → {raw}");
            return Json.Deserialize<InitDto>(raw);
        }

        public static async UniTask<RequestResult> ApplyToPlayerSaveAsync(this WebSocketSqlBackendService svc)
        {
            try
            {
                var init = await svc.Auth.FetchInitialDataAsync();

                // ---- profile ----
                PlayerSave.SetPlayerName(init.profile.displayName);
                PlayerSave.SetPlayerIcon(init.profile.iconId);
                PlayerSave.SetPlayerFrame(init.profile.frameId);
                PlayerSave.SetSelectedCharacter(init.profile.selectedCharacterId);
                PlayerSave.SetFavouriteCharacter(init.profile.favouriteCharacterId);
                PlayerSave.SetAccountLevel(init.profile.accountLevel);
                PlayerSave.SetAccountCurrentExp(init.profile.accountCurrentExp);

                // ---- currencies ----
                foreach (var cur in init.currencies)
                    MonetizationManager.SetCurrency(cur.code, cur.balance);

                // ---- purchases ----
                foreach (var id in init.owned.shopItemIds)
                    PlayerSave.AddShopItem(new PurchasedShopItem { itemId = id });
                foreach (var id in init.owned.iconIds)
                    PlayerSave.AddIcon(new PurchasedIcon { iconId = id });
                foreach (var id in init.owned.frameIds)
                    PlayerSave.AddFrame(new PurchasedFrame { frameId = id });
                foreach (var cid in init.owned.characterIds)
                    PlayerSave.AddCharacter(new PurchasedCharacter { characterId = cid.ToString() });
                foreach (var inv in init.owned.inventoryItems)
                {
                    PlayerSave.AddInventoryItem(new PurchasedInventoryItem
                    {
                        uniqueItemGuid = inv.uniqueItemGuid,
                        itemId = inv.templateItemId,
                        itemLevel = inv.itemLevel,
                        upgrades = new Dictionary<int, int>()
                    });
                    PlayerSave.SetItemUpgradeLevel(inv.uniqueItemGuid, inv.itemLevel);
                }                   
                PlayerSave.LoadAllPurchased();
                // ---- charactersData ----
                foreach (var c in init.charactersData)
                {
                    PlayerSave.SetCharacterLevel(c.characterId, c.level);
                    PlayerSave.SetCharacterCurrentExp(c.characterId, c.currentExp);
                    PlayerSave.SetCharacterMasteryLevel(c.characterId, c.masteryLevel);
                    PlayerSave.SetCharacterCurrentMasteryExp(c.characterId, c.masteryCurrentExp);
                    PlayerSave.SaveCharacterUnlockedSkins(c.characterId, c.unlockedSkins);

                    if (int.TryParse(c.selectedSkinId, out var skinIdx))
                        PlayerSave.SetCharacterSkin(c.characterId, skinIdx);

                    foreach (var kv in c.slots)
                        PlayerSave.SetCharacterSlotItem(c.characterId, kv.Key, kv.Value);

                    foreach (var kv in c.upgrades)
                    {
                        StatType statType;

                        if (!Enum.TryParse<StatType>(kv.Key, ignoreCase: true, out statType))
                        {
                            if (int.TryParse(kv.Key, out int index)
                                && Enum.IsDefined(typeof(StatType), index))
                            {
                                statType = (StatType)index;
                            }
                            else
                            {
                                Debug.LogWarning(
                                   $"[DataLoad] Unknown stat key '{kv.Key}' for character {c.characterId}");
                                continue;
                            }
                        }
                        PlayerSave.SetCharacterUpgradeLevel(c.characterId, statType, kv.Value);
                    }
                }
                var pr = init.progress;
                // Score
                PlayerSave.SetScore(pr.score);

                // Battle‑Pass
                PlayerSave.SetBattlePassProgress( pr.battlePass.currentXp, pr.battlePass.level, pr.battlePass.premium);

                // --- Battle‑Pass Season Meta (season number, full start datetime, duration) ---
                if (pr.battlePassMeta != null)
                {
                    DateTime startUtc;
                    string s = pr.battlePassMeta.seasonStartUtc;

                    // Strict parse(s)
                    string[] isoFormats = {
                        "o",
                        "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'",
                        "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'",
                        "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff",
                        "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    };

                    bool parsed = DateTime.TryParseExact(
                        s, isoFormats,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out startUtc);

                    if (!parsed)
                    {
                        parsed = DateTime.TryParse(
                            s,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.RoundtripKind,
                            out startUtc);
                    }

                    if (!parsed)
                    {
                        Debug.LogWarning($"[DataLoad] Invalid seasonStartUtc '{s}'");
                        startUtc = DateTime.UtcNow;
                    }

                    PlayerSave.SetBattlePassSeasonMeta(
                        pr.battlePassMeta.season,
                        startUtc,
                        pr.battlePassMeta.durationDays);
                }
                else
                {
                    Debug.LogWarning("[DataLoad] Missing battlePassMeta in init payload; using defaults.");
                }

                ulong bits = pr.battlePass.claimedBits;
                var passItems = GameInstance.Singleton.battlePassData;

                for (int i = 0; i < passItems.Length && i < 64; i++)
                {
                    if ((bits & (1UL << i)) != 0)      
                        PlayerSave.MarkBattlePassReward(passItems[i].passId);
                }

                // Unlocked maps
                PlayerSave.SetUnlockedMaps(pr.unlockedMaps);

                // Quests
                foreach (var q in pr.quests)
                {
                    PlayerSave.SaveQuestProgress(q.questId, q.progress);
                    if (q.completed && q.questType != "Repeat")
                        PlayerSave.SaveQuestCompletion(q.questId);
                }

                // ---- rewards progress ----
                if (init.rewards != null)
                {
                    /* New-Player */
                    var np = init.rewards.newPlayer;
                    if (np != null)
                    {
                        var npData = PlayerSave.GetNewPlayerRewardsLocal();

                        if (!string.IsNullOrEmpty(np.joinedAt) &&
                            DateTime.TryParse(np.joinedAt, out var join))
                            npData.accountCreationDate = join.Date;

                        if (!string.IsNullOrEmpty(np.lastClaimed) &&
                            DateTime.TryParse(np.lastClaimed, out var lc))
                            npData.lastClaimDate = lc.Date;

                        npData.claimedRewards = BitMaskToList(np.claimedBits);
                        PlayerSave.SetNewPlayerRewardsLocal(npData);
                    }

                    /* Daily */
                    var dl = init.rewards.daily;
                    if (dl != null)
                    {
                        var dlData = PlayerSave.GetDailyRewardsLocal();
                        if (!string.IsNullOrEmpty(dl.firstClaimed) &&
                            DateTime.TryParse(dl.firstClaimed, out var fc))
                            dlData.firstClaimDate = fc.Date;

                        if (!string.IsNullOrEmpty(dl.lastClaimed) &&
                            DateTime.TryParse(dl.lastClaimed, out var lc))
                            dlData.lastClaimDate = lc.Date;

                        long rewardbits = 0;
                        if (!string.IsNullOrEmpty(dl.claimedBits))
                            long.TryParse(dl.claimedBits, out rewardbits);

                        dlData.claimedRewards = BitMaskToList(rewardbits);
                        PlayerSave.SetDailyRewardsLocal(dlData);
                    }
                }

                // ---- used coupons ----
                if (init.usedCoupons != null && init.usedCoupons.Count > 0)
                {
                    foreach (var uc in init.usedCoupons)
                        PlayerSave.MarkCouponAsUsed(uc);
                }

                if (init.claimedMapRewards != null)
                    PlayerSave.SaveClaimedMapRewards(init.claimedMapRewards);

                BattlePassManager.Singleton.SyncFromPlayerSave();

                return RequestResult.Ok();
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataLoad] failed: {e}");
                return RequestResult.Fail("load_failed");
            }
        }

        private static List<int> BitMaskToList(long bits)
        {
            var list = new List<int>(8);
            for (int i = 0; i < 64; ++i)
                if ((bits & (1L << i)) != 0)
                    list.Add(i);
            return list;
        }

        // --- orchestration methods ---

        public static async UniTask<RequestResult> RegisterAndLoadAsync(
            this WebSocketSqlBackendService svc,
            string email, string pass, string confirm)
        {
            if (pass != confirm)
                return RequestResult.Fail("PASSWORD_MISMATCH");

            var reg = await svc.Auth.RegisterAsync(email, pass);
            if (!reg.Success) return reg;

            var login = await svc.Auth.LoginAsync(email, pass);
            if (!login.Success) return login;

            return await svc.ApplyToPlayerSaveAsync();
        }

        public static async UniTask<RequestResult> LoginAndLoadAsync(
            this WebSocketSqlBackendService svc,
            string email, string pass)
        {
            var login = await svc.Auth.LoginAsync(email, pass);
            if (!login.Success) return login;
            return await svc.ApplyToPlayerSaveAsync();
        }

        public static async UniTask<RequestResult> GuestAndLoadAsync(
            this WebSocketSqlBackendService svc)
        {
            var guest = await svc.Auth.LoginGuestAsync();
            if (!guest.Success) return guest;
            return await svc.ApplyToPlayerSaveAsync();
        }

        public static async UniTask<bool> TryAutoLoginAndLoadAsync(
            this WebSocketSqlBackendService svc)
        {
            var ok = await svc.Auth.TryAutoLoginAsync();
            if (!ok) return false;
            var res = await svc.ApplyToPlayerSaveAsync();
            return res.Success;
        }
    }
}
