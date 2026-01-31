// WebsocketExpHandler.cs
using BulletHellTemplate;
using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public static class WebsocketExpHandler
    {
        /// <summary>
        /// Adds EXP to the account: performs local level-up logic, sends to server and reapplies locally on success.
        /// </summary>
        public static async UniTask<RequestResult> AddAccountExpAsync(
            this WebSocketSqlBackendService svc,
            int exp)
        {

            var cfg = GameInstance.Singleton.accountLevels;
            int level = PlayerSave.GetAccountLevel();
            int maxLevel = cfg.accountMaxLevel;
            if (level >= maxLevel)
                return RequestResult.Fail("max");

            int expPool = PlayerSave.GetAccountCurrentExp() + exp;
            int levelUps = 0;
            while (level < maxLevel)
            {
                int req = GameInstance.Singleton.GetAccountExpForLevel(level);
                if (expPool < req) break;
                expPool -= req;
                level++;
                levelUps++;
            }
            if (level >= maxLevel) expPool = 0;

            var payload = new Dictionary<string, object> { ["amount"] = exp };
            string raw = await svc.Auth.Http.Request("POST", "auth/exp/account", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
            {
                var code = dto.TryGetValue("code", out var c) ? c.ToString() : "err";
                return RequestResult.Fail(code);
            }

            PlayerSave.SetAccountLevel(level);
            PlayerSave.SetAccountCurrentExp(expPool);
            return levelUps > 0
                ? RequestResult.Ok($"up:{levelUps}")
                : RequestResult.Ok();
        }

        /// <summary>
        /// Adds EXP to a character: local logic, server call, reapply on success.
        /// </summary>
        public static async UniTask<RequestResult> AddCharacterExpAsync(this WebSocketSqlBackendService svc,int cid, int exp)
        {
            var cd = GameInstance.Singleton.GetCharacterDataById(cid);
            if (cd == null) return RequestResult.Fail("invalid");
            int lvl = PlayerSave.GetCharacterLevel(cid);
            if (lvl >= cd.maxLevel) return RequestResult.Fail("max");

            int newPool = PlayerSave.GetCharacterCurrentExp(cid) + exp;

            // 2) Server
            var payload = new Dictionary<string, object>
            {
                ["characterId"] = cid,
                ["amount"] = exp
            };
            string raw = await svc.Auth.Http.Request("POST", "auth/exp/character", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
            {
                var code = dto.TryGetValue("code", out var c) ? c.ToString() : "err";
                return RequestResult.Fail(code);
            }

            // 3) Commit local
            PlayerSave.SetCharacterCurrentExp(cid, newPool);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Adds mastery EXP: local logic, server call, reapply on success.
        /// </summary>
        public static async UniTask<RequestResult> AddCharacterMasteryExpAsync(
            this WebSocketSqlBackendService svc,
            int cid, int exp)
        {
            // 1) Local
            var masteryCfg = GameInstance.Singleton.characterMastery;
            int maxM = masteryCfg.maxMasteryLevel - 1;
            int lvl = PlayerSave.GetCharacterMasteryLevel(cid);
            if (lvl >= maxM) return RequestResult.Fail("max");

            int pool = PlayerSave.GetCharacterCurrentMasteryExp(cid) + exp;
            int ups = 0;
            while (lvl < maxM)
            {
                int req = GameInstance.Singleton.GetMasteryExpForLevel(lvl);
                if (pool < req) break;
                pool -= req;
                lvl++;
                ups++;
            }
            if (lvl >= maxM) pool = 0;

            // 2) Server
            var payload = new Dictionary<string, object>
            {
                ["characterId"] = cid,
                ["amount"] = exp
            };
            string raw = await svc.Auth.Http.Request("POST", "auth/exp/mastery", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
            {
                var code = dto.TryGetValue("code", out var c) ? c.ToString() : "err";
                return RequestResult.Fail(code);
            }
            PlayerSave.SetCharacterMasteryLevel(cid, lvl);
            PlayerSave.SetCharacterCurrentMasteryExp(cid, pool);
            return ups > 0
                ? RequestResult.Ok($"up:{ups}")
                : RequestResult.Ok();
        }
        /// <summary>
        /// Adds battle pass XP: local logic, server call, reapply on success.
        /// </summary>
        public static async UniTask<RequestResult> AddBattlePassXpAsync(this WebSocketSqlBackendService svc,int amount)
        {
            var bpCfg = GameInstance.Singleton;
            (int xp, int lvl, bool prem) = PlayerSave.GetBattlePassProgress();
            int maxLvl = bpCfg.maxLevelPass;
            xp += amount;
            while (lvl < maxLvl && xp >= XPForLevel(lvl))
            {
                xp -= XPForLevel(lvl);
                lvl++;
            }
            if (lvl >= maxLvl) xp = XPForLevel(maxLvl);

            var payload = new Dictionary<string, object> { ["amount"] = amount };
            string raw = await svc.Auth.Http.Request("POST", "auth/exp/battlepass", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(
                    dto.TryGetValue("code", out var c) ? c.ToString() : "err"
                );

            PlayerSave.SetBattlePassProgress(xp, lvl, prem);

            if (BattlePassManager.Singleton != null)
                BattlePassManager.Singleton.SyncFromPlayerSave();

            return RequestResult.Ok();
        }


        private static int XPForLevel(int lvl) =>
            Mathf.FloorToInt(
                GameInstance.Singleton.baseExpPass *
                Mathf.Pow(1f + GameInstance.Singleton.incPerLevelPass, lvl - 1)
            );
    }
}
