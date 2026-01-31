using BulletHellTemplate;
using Colyseus;
using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// All server‑side progress operations (battle‑pass season, quests, end‑game session, …).
    /// </summary>
    public static class WebsocketProgressHandler
    {
        /*──────────────────── End‑game Session ───────────────────*/

        /// <summary>
        /// Sends end-of-session stats to the backend and applies minimal local feedback.
        /// Registers pending "first completion" map reward (session), and unlocks next map if appropriate.
        /// Does <b>not</b> grant map rewards here; that is handled by the Map Rewards popup flow.
        /// </summary>
        public static async UniTask<RequestResult> CompleteGameSessionAsync(
            this WebSocketSqlBackendService svc,
            EndGameSessionData data,
            bool autoShowMapRewardsPopup = true)
        {
            // ----- Local sanity -----
            if (data.monstersKilled < 0 || data.gainedGold < 0)
                return RequestResult.Fail("bad_data");

            // ----- Build payload -----
            var payload = new Dictionary<string, object>
            {
                ["mapId"] = data.mapId,
                ["characterId"] = data.characterId,
                ["monstersKilled"] = data.monstersKilled,
                ["gainedGold"] = data.gainedGold,
                ["won"] = data.won
            };

            string raw;
            try
            {
                raw = await svc.Auth.Http.Request("POST", "auth/progress/session", payload);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CompleteGameSessionAsync] Net error: {e}");
                return RequestResult.Fail("net");
            }

            // Defensive: ensure JSON
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);

            if (!(dto.TryGetValue("success", out var okObj) && okObj is bool ok && ok))
            {
                string rsn = dto.TryGetValue("reason", out var rObj) ? rObj?.ToString() ?? "err" : "err";
                return RequestResult.Fail(rsn);
            }

            // ----- Local feedback: score -----
            int newScore = PlayerSave.GetScore() + data.monstersKilled;
            PlayerSave.SetScore(newScore);

            // ----- Local feedback: gold -----
            string goldCur = GameInstance.Singleton.goldCurrency;
            int bal = MonetizationManager.GetCurrency(goldCur) + data.gainedGold;
            // pushToBackend:false -> server already updated
            MonetizationManager.SetCurrency(goldCur, bal, pushToBackend: false);

            // ----- Win path: register pending map reward & unlock progression -----
            if (data.won)
            {
                var mapData = GameInstance.Singleton.GetMapInfoDataById(data.mapId);
                if (mapData != null
                    && mapData.isRewardOnCompleteFirstTime
                    && (mapData.WinMapRewards.Count > 0 || mapData.rewardType != MapRewardType.None)
                    && !(RewardManagerPopup.Singleton?.HasLocalMapClaimed(data.mapId) ?? false))
                {
                    RewardManagerPopup.Singleton?.RegisterMapCompleted(data.mapId);
                }

                // progression unlock (now in-context helper)
                TryUnlockProgressionForMap(data.mapId);
            }

            // ----- Apply any inventory items granted by session endpoint -----
            if (dto.TryGetValue("newInventoryItems", out var arrObj) && arrObj is List<object> arr)
            {
                foreach (var o in arr)
                {
                    if (o is Dictionary<string, object> itm)
                    {
                        string guid = itm.TryGetValue("guid", out var g) ? g?.ToString() : Guid.NewGuid().ToString("N").Substring(0, 8);
                        string tpl = itm.TryGetValue("templateItemId", out var t) ? t?.ToString() : "UNKNOWN";
                        int lvl = 1;
                        if (itm.TryGetValue("itemLevel", out var ilv) && ilv != null && int.TryParse(ilv.ToString(), out var parsedLvl))
                            lvl = parsedLvl;

                        PlayerSave.AddInventoryItem(new PurchasedInventoryItem
                        {
                            uniqueItemGuid = guid,
                            itemId = tpl,
                            itemLevel = lvl,
                            upgrades = new Dictionary<int, int>()
                        });
                    }
                }
            }

            // ----- Optionally show Map Rewards popup if pending -----
            if (autoShowMapRewardsPopup)
                TryShowMapRewardsPopup();

            return RequestResult.Ok();
        }

        /// <summary>
        /// If there are pending first-completion map rewards, request the popup to show.
        /// Safe even if popup not yet spawned (will defer through RewardManagerPopup).
        /// </summary>
        private static void TryShowMapRewardsPopup()
        {
            if (UIRewardManagerPopup.Singleton != null)
            {
                UIRewardManagerPopup.Singleton.ShowPendingRewards();
                return;
            }

            if (RewardManagerPopup.Singleton != null &&
                RewardManagerPopup.Singleton.gameObject != null &&
                RewardManagerPopup.Singleton.gameObject.activeInHierarchy)
            {
                RewardManagerPopup.Singleton.StartSyncFromServer();
            }
            
        }

        /// <summary>
        /// Called after a successful win to unlock the next map in progression,
        /// but only if the just-completed map is currently the last unlocked map in PlayerSave.
        /// </summary>
        private static void TryUnlockProgressionForMap(int justCompletedMapId)
        {
            if (!PlayerSave.IsLatestUnlockedMap(justCompletedMapId))
                return;

            UnlockNextMap();
        }

        /// <summary>
        /// Unlocks the next map in sequence based on <see cref="GameInstance.Singleton.mapInfoData"/>.
        /// Uses <see cref="PlayerSave.GetUnlockedMaps"/> / <see cref="PlayerSave.SetUnlockedMaps"/> for persistence.
        /// </summary>
        private static void UnlockNextMap()
        {
            var gi = GameInstance.Singleton;
            if (gi == null || gi.mapInfoData == null || gi.mapInfoData.Length == 0)
            {
                Debug.LogWarning("[WebsocketProgressHandler] UnlockNextMap called but GameInstance.mapInfoData missing.");
                return;
            }

            List<int> unlockedMaps = PlayerSave.GetUnlockedMaps();
            MapInfoData[] allMaps = gi.mapInfoData;

            // Determine last unlocked
            int lastUnlockedMapId;
            if (unlockedMaps.Count > 0)
            {
                lastUnlockedMapId = unlockedMaps[unlockedMaps.Count - 1];
            }
            else
            {
                // fallback: first map in data that is flagged unlocked in def
                lastUnlockedMapId = 0;
                for (int i = 0; i < allMaps.Length; i++)
                {
                    if (allMaps[i].isUnlocked)
                    {
                        lastUnlockedMapId = allMaps[i].mapId;
                        break;
                    }
                }
                if (lastUnlockedMapId == 0)
                {
                    // nothing flagged; bail
                    return;
                }
                if (!unlockedMaps.Contains(lastUnlockedMapId))
                    unlockedMaps.Add(lastUnlockedMapId);
            }

            // Find its index; unlock the next
            for (int i = 0; i < allMaps.Length; i++)
            {
                if (allMaps[i].mapId == lastUnlockedMapId && i + 1 < allMaps.Length)
                {
                    int nextMapId = allMaps[i + 1].mapId;
                    if (!unlockedMaps.Contains(nextMapId))
                    {
                        unlockedMaps.Add(nextMapId);
                        PlayerSave.SetUnlockedMaps(unlockedMaps);
                    }
                    break;
                }
            }
        }

        /*──────────────────── Quests ───────────────────*/

        public static async UniTask<RequestResult> CompleteQuestAsync(
            this WebSocketSqlBackendService svc,
            int questId)
        {
            var payload = new Dictionary<string, object> { ["questId"] = questId };
            string raw = await svc.Auth.Http.Request("POST", "auth/progress/quest/complete", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("reason", out var r) ? r.ToString() : "err");

            PlayerSave.SaveQuestCompletion(questId);

            if (dto.TryGetValue("currencies", out var curArr) && curArr is List<object> curList)
            {
                foreach (Dictionary<string, object> c in curList)
                {
                    string code = c["code"].ToString();
                    int amt = Convert.ToInt32(c["amount"]);
                    int bal = MonetizationManager.GetCurrency(code) + amt;
                    MonetizationManager.SetCurrency(code, bal, pushToBackend: false);
                }
            }

            if (dto.TryGetValue("newInventoryItems", out var invArr) && invArr is List<object> invList)
            {
                foreach (Dictionary<string, object> itm in invList)
                {
                    PlayerSave.AddInventoryItem(new PurchasedInventoryItem
                    {
                        uniqueItemGuid = itm["guid"].ToString(),
                        itemId = itm["templateItemId"].ToString(),
                        itemLevel = 1,
                        upgrades = new Dictionary<int, int>()
                    });
                }
            }
            return RequestResult.Ok();
        }

        public static async UniTask<RequestResult> RefreshQuestLevelProgressAsync(this WebSocketSqlBackendService svc)
        {
            string raw = await svc.Auth.Http.Request(
                "POST",
                "auth/progress/quest/refresh", 
                null);

            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"')); 

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (dto.TryGetValue("success", out var ok) && (bool)ok)
                return RequestResult.Ok();

            string reason = dto.TryGetValue("reason", out var r) ? r.ToString() : "err";
            return RequestResult.Fail(reason);
        }

        public static async UniTask<List<Dictionary<string, object>>> GetTopPlayersAsync(this WebSocketSqlBackendService svc,int limit = 20)
        {
            string raw = await svc.Auth.Http.Request("GET", $"auth/ranking/top?limit={limit}", null);
            if (!raw.TrimStart().StartsWith("["))
            {
                Debug.LogWarning($"[GetTopPlayers] bad payload: {raw}");
                return new List<Dictionary<string, object>>();
            }
            return Json.Deserialize<List<Dictionary<string, object>>>(raw);
        }

        public static async UniTask<int> GetPlayerRankAsync(this WebSocketSqlBackendService svc)
        {
            string raw = await svc.Auth.Http.Request("GET", "auth/ranking/my", null);
            if (!raw.TrimStart().StartsWith("{"))
            {
                Debug.LogWarning($"[GetPlayerRank] bad payload: {raw}");
                return 0;
            }
            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (dto.TryGetValue("rank", out var rObj))
            {
                if (rObj is double d) return (int)d;
                if (rObj is int i) return i;
                if (int.TryParse(rObj?.ToString(), out var p)) return p;
            }
            return 0;
        }

    }
}
