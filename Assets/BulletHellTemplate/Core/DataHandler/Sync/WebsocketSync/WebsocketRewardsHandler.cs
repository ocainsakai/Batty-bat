using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public static class WebsocketRewardsHandler
    {
        public static async UniTask<RequestResult> RedeemCouponAsync(
           this WebSocketSqlBackendService svc,
           string code)
        {
            var payload = new Dictionary<string, object> { ["code"] = code };
            string raw = await svc.Auth.Http.Request("POST", "auth/coupon/redeem", payload);

            if (!raw.TrimStart().StartsWith("{"))
            {
                return RequestResult.Fail(raw.Trim('"'));
            }

            Dictionary<string, object> dto;
            try
            {
                dto = Json.Deserialize<Dictionary<string, object>>(raw);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RedeemCoupon] JSON parse fail: {ex}");
                return RequestResult.Fail("err");
            }

            if (!(dto.TryGetValue("success", out var okObj) && okObj is bool ok && ok))
            {
                // error codes expected: "0" (used), "1" (invalid)
                string codeStr = dto.TryGetValue("reason", out var rObj) ? rObj?.ToString() : "err";
                return RequestResult.Fail(codeStr);
            }

            // --- sucess ---
            // preferred: parse structured fields; fallback reason string
            string cid = null;
            int amount = 0;

            if (dto.TryGetValue("currency", out var cObj))
                cid = cObj?.ToString();
            if (dto.TryGetValue("amount", out var aObj))
                int.TryParse(aObj?.ToString(), out amount);

            // fallback parse reason "CID|N"
            if ((cid == null || amount == 0) &&
                dto.TryGetValue("reason", out var rsObj))
            {
                var parts = rsObj?.ToString()?.Split('|');
                if (parts != null && parts.Length == 2)
                {
                    cid = cid ?? parts[0];
                    if (amount == 0) int.TryParse(parts[1], out amount);
                }
            }

            if (!string.IsNullOrEmpty(cid) && amount > 0)
            {
                int cur = MonetizationManager.GetCurrency(cid);
                MonetizationManager.SetCurrency(cid, cur + amount);
            }

            if (dto.TryGetValue("couponId", out var idObj))
            {
                PlayerSave.MarkCouponAsUsed(idObj?.ToString());
            }
            else
            {
                PlayerSave.MarkCouponAsUsed(code);
            }
            string reason = $"{cid}|{amount}";
            return RequestResult.Ok(reason);
        }

        public static async UniTask<RequestResult> ClaimNewPlayerRewardAsync(
         this WebSocketSqlBackendService svc,
         int dayIndex,
         BattlePassItem rewardTemplate)
        {
            // --- Local fast validation (1/day, sequential catch-up) ---
            var local = PlayerSave.GetNewPlayerRewardsLocal();
            int maxDays = GameInstance.Singleton.newPlayerRewardItems?.Length ?? int.MaxValue;
            if (maxDays <= 0) maxDays = int.MaxValue;

            // Invalid index?
            if (dayIndex < 0 || dayIndex >= maxDays)
                return RequestResult.Fail("1");

            // Already claimed?
            if (local.claimedRewards.Contains(dayIndex))
                return RequestResult.Fail("0");

            // Already claimed a new-player reward today?
            if (local.lastClaimDate != DateTime.MinValue &&
                local.lastClaimDate.Date == DateTime.Now.Date)
                return RequestResult.Fail("0"); // 1/day gating

            // Must be next unclaimed in sequence (catch-up)
            int expected = local.claimedRewards.Count;
            if (dayIndex != expected)
                return RequestResult.Fail("1");

            // Day must be unlocked by time since account creation (soft check; server validates)
            if (local.accountCreationDate != DateTime.MinValue)
            {
                int daysSince = (int)(DateTime.Now.Date - local.accountCreationDate.Date).TotalDays;
                if (dayIndex > daysSince)
                    return RequestResult.Fail("1");
            }

            // --- Server validation ---
            string raw;
            try
            {
                var payload = new System.Collections.Generic.Dictionary<string, object> { ["dayIndex"] = dayIndex };
                raw = await svc.Auth.Http.Request("POST", "auth/rewards/new-player/claim", payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebsocketRewards] NP claim net error: {e}");
                return RequestResult.Fail("2");
            }

            var resp = Json.Deserialize<ClaimServerDto>(raw);
            if (!resp.success)
                return RequestResult.Fail(MapServerReasonToOfflineCode(resp.reason));

            // --- Apply reward locally (template path) ---
            var applyRes = OfflinePurchasesHandler.ClaimBattlePassReward(rewardTemplate);
            if (!applyRes.Success)
                return RequestResult.Fail("2");

            // --- Update local cache ---
            if (!local.claimedRewards.Contains(dayIndex))
                local.claimedRewards.Add(dayIndex);
            local.lastClaimDate = DateTime.Now.Date;
            PlayerSave.SetNewPlayerRewardsLocal(local);

            return RequestResult.Ok();
        }


        public static async UniTask<RequestResult> ClaimDailyRewardAsync(
            this WebSocketSqlBackendService svc,
            int dayIndex,
            BattlePassItem rewardTemplate)
        {
            var local = PlayerSave.GetDailyRewardsLocal();
            int cycleLen = GameInstance.Singleton.dailyRewardItems?.Length ?? int.MaxValue;

            // --- Local cycle reset (preguiçoso) ---
            if (cycleLen > 0 &&
                local.claimedRewards.Count >= cycleLen &&
                local.lastClaimDate != DateTime.MinValue &&
                (DateTime.Now.Date - local.lastClaimDate.Date).TotalDays >= 1)
            {
                local.claimedRewards.Clear();
                local.firstClaimDate = DateTime.Now.Date;
                local.lastClaimDate = DateTime.MinValue;
                PlayerSave.SetDailyRewardsLocal(local);
            }

            // --- Local fast validation ---
            if (!CanClaimDaily(local, dayIndex, cycleLen, out var fail))
                return RequestResult.Fail(fail); // no server call

            // --- Server validation ---
            string raw;
            try
            {
                var payload = new Dictionary<string, object> { ["dayIndex"] = dayIndex };
                raw = await svc.Auth.Http.Request("POST", "auth/rewards/daily/claim", payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"[WebsocketRewards] Daily claim net error: {e}");
                return RequestResult.Fail("2");
            }

            var resp = Json.Deserialize<ClaimServerDto>(raw);
            if (!resp.success)
                return RequestResult.Fail(MapServerReasonToOfflineCode(resp.reason));

            // --- Apply reward locally (template) ---
            var applyRes = OfflinePurchasesHandler.ClaimBattlePassReward(rewardTemplate);
            if (!applyRes.Success)
                return RequestResult.Fail("2");

            // --- Update local cache ---
            if (!local.claimedRewards.Contains(dayIndex))
                local.claimedRewards.Add(dayIndex);

            if (local.claimedRewards.Count == 1)
                local.firstClaimDate = DateTime.Now.Date;

            local.lastClaimDate = DateTime.Now.Date;
            PlayerSave.SetDailyRewardsLocal(local);

            // (Opcional) Proximo reset rápido:
            PlayerSave.SetNextDailyReset(DateTime.Now.Date.AddDays(1));

            return RequestResult.Ok();
        }

        public static async UniTask<RequestResult> ApplyMapRewardsAsync(
          this WebSocketSqlBackendService svc,
          MapCompletionRewardData data)
        {
            // short-circuit if already claimed local
            if (RewardManagerPopup.Singleton != null &&
                RewardManagerPopup.Singleton.HasLocalMapClaimed(data.mapId))
            {
                return RequestResult.Fail("0"); // already local
            }

            string raw;
            try
            {
                var payload = new Dictionary<string, object>
                {
                    ["mapId"] = data.mapId,
                    ["characterId"] = data.characterId,
                };
                raw = await svc.Auth.Http.Request("POST", "auth/map-rewards/claim", payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapRewards] claim net error: {e}");
                return RequestResult.Fail("2");
            }

            var resp = Json.Deserialize<MapClaimServerDto>(raw);
            if (resp == null || !resp.success || resp.reward == null || !resp.reward.granted)
                return RequestResult.Fail(MapMapServerReasonToOfflineCode(resp?.reason ?? resp?.reward?.reason));

            // Apply economy deltas locally (simple add; no offlineExp simulation)
            ApplyMapRewardEchoLocal(resp.reward, data.characterId);

            // mark local
            RewardManagerPopup.Singleton?.MarkRewardClaimed(resp.reward.mapId);

            return RequestResult.Ok();
        }

        static string MapMapServerReasonToOfflineCode(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return "2";
            switch (reason)
            {
                case "map_not_found":
                case "no_reward":
                case "already_claimed":
                    return "0";
                case "bad_map":
                case "bad_character":
                    return "1";
                default:
                    return "2";
            }
        }

        /// <summary>
        /// Validate a Daily reward claim locally.
        /// Enforces 1 claim per calendar day, sequential order, and optional cycle-length clamp.
        /// failCode: "0" already claimed / already today; "1" not available yet/bad index.
        /// </summary>
        public static bool CanClaimDaily(
            PlayerSave.DailyRewardsData local,
            int dayIndex,
            int cycleLength,
            out string failCode)
        {
            failCode = null;

            if (cycleLength <= 0)
                cycleLength = int.MaxValue; // no configured limit; skip clamp

            if (dayIndex < 0 || dayIndex >= cycleLength)
            {
                failCode = "1";
                return false;
            }

            if (local.claimedRewards.Contains(dayIndex))
            {
                failCode = "0";
                return false;
            }

            // 1x per calendar day
            if (local.lastClaimDate != DateTime.MinValue &&
                local.lastClaimDate.Date == DateTime.Now.Date)
            {
                failCode = "0"; // already claimed today
                return false;
            }

            // Sequential gating
            int expected = local.claimedRewards.Count;
            if (dayIndex > expected)
            {
                failCode = "1";
                return false;
            }

            return true;
        }

        [Serializable]
        class ClaimServerDto
        {
            public bool success;
            public string reason;
        }

        static string MapServerReasonToOfflineCode(string reason)
        {
            if (string.IsNullOrEmpty(reason))
                return "2";
            switch (reason)
            {
                case "already_claimed":
                case "already_claimed_today":
                case "already_today":
                    return "0";
                case "not_available_yet":
                case "invalid_index":
                case "bad_index":
                    return "1";
                default:
                    return "2";
            }
        }

        static void ApplyMapRewardEchoLocal(MapClaimRewardDto dto, int characterId)
        {
            if (dto == null) return;

            // Currency
            if (dto.currencyGrants != null)
            {
                foreach (var cg in dto.currencyGrants)
                {
                    if (string.IsNullOrEmpty(cg.currencyId)) continue;
                    int current = MonetizationManager.GetCurrency(cg.currencyId);
                    MonetizationManager.SetCurrency(cg.currencyId, current + cg.amount);
                }
            }

            // Account EXP (simple add; no level calc)
            if (dto.accountExp > 0)
            {
                int lvl, cur;
                (lvl, cur) = (PlayerSave.GetAccountLevel(), PlayerSave.GetAccountCurrentExp());
                PlayerSave.SetAccountCurrentExp(cur + dto.accountExp);                        
            }

            // Character EXP
            if (dto.characterExp > 0 && characterId >= 0)
            {
                int cur = PlayerSave.GetCharacterCurrentExp(characterId);
                PlayerSave.SetCharacterCurrentExp(characterId, cur + dto.characterExp);
            }

            // Mastery EXP
            if (dto.masteryExp > 0 && characterId >= 0)
            {
                int cur = PlayerSave.GetCharacterCurrentMasteryExp(characterId);
                PlayerSave.SetCharacterCurrentMasteryExp(characterId, cur + dto.masteryExp);
            }

            // Special
            if (dto.special != null)
            {
                switch (dto.special.type)
                {
                    case "Icon":
                        PlayerSave.AddIcon(new PurchasedIcon { iconId = dto.special.id });
                        break;
                    case "Frame":
                        PlayerSave.AddFrame(new PurchasedFrame { frameId = dto.special.id });
                        break;
                    case "Character":
                        PlayerSave.AddCharacter(new PurchasedCharacter { characterId = dto.special.id.ToString() });
                        break;
                    case "InventoryItem":
                        PlayerSave.AddInventoryItem(new PurchasedInventoryItem
                        {
                            uniqueItemGuid = Guid.NewGuid().ToString("N").Substring(0, 8),
                            itemId = dto.special.id,
                            itemLevel = 1,
                            upgrades = new Dictionary<int, int>()
                        });
                        break;
                }
            }
        }
    }
}
