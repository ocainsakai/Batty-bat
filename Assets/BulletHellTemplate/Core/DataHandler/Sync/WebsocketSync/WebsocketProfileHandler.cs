using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    public static class WebsocketProfileHandler
    {
        /// <summary>
        /// Change player's nickname: validate locally, call server, then apply locally.
        /// </summary>
        public static async UniTask<RequestResult> ChangePlayerNameAsync(
            this WebSocketSqlBackendService svc,
            string newName)
        {
            var cfg = GameInstance.Singleton;
            if (string.IsNullOrEmpty(newName)
                || newName.Length < cfg.minNameLength
                || newName.Length > cfg.maxNameLength)
            {
                return RequestResult.Fail("0");
            }
            if (PlayerSave.GetPlayerName()
                .Equals(newName, System.StringComparison.OrdinalIgnoreCase))
            {
                return RequestResult.Fail("2");
            }
            if (cfg.needTicket)
            {
                int balance = MonetizationManager.GetCurrency(cfg.changeNameTick);
                if (balance < cfg.ticketsToChange)
                {
                    return RequestResult.Fail("1");
                }
            }

            var payload = new Dictionary<string, object> { ["newName"] = newName };
            string raw = await svc.Auth.Http.Request("POST", "auth/profile/name", payload);
            if (!raw.TrimStart().StartsWith("{"))
            {
                return RequestResult.Fail(raw.Trim('"'));
            }
            var res = Json.Deserialize<Dictionary<string, object>>(raw);
            bool ok = res.TryGetValue("success", out var s) && (bool)s;
            if (!ok)
            {
                var reason = res.TryGetValue("reason", out var r) ? r.ToString() : "UNKNOWN";
                return RequestResult.Fail(reason);
            }

            if (cfg.needTicket)
            {
                int balance = MonetizationManager.GetCurrency(cfg.changeNameTick);
                MonetizationManager.SetCurrency(
                    cfg.changeNameTick,
                    balance - cfg.ticketsToChange,
                    pushToBackend: false
                );
            }
            PlayerSave.SetPlayerName(newName);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Change player's icon: validate locally, call server, then apply locally.
        /// </summary>
        public static async UniTask<RequestResult> ChangePlayerIconAsync(
            this WebSocketSqlBackendService svc,
            string iconId)
        {
            // 1) Local validation
            var item = GameInstance.Singleton.GetIconItemById(iconId);
            if (item == null) return RequestResult.Fail("INVALID_ICON");
            bool owned = item.isUnlocked || PlayerSave.IsIconPurchased(iconId);
            if (!owned) return RequestResult.Fail("NOT_OWNED");

            // 2) Server
            var payload = new Dictionary<string, object> { ["iconId"] = iconId };
            string raw = await svc.Auth.Http.Request("POST", "auth/profile/icon", payload);
            if (!raw.TrimStart().StartsWith("{"))
            {
                return RequestResult.Fail(raw.Trim('"'));
            }
            var res = Json.Deserialize<Dictionary<string, object>>(raw);
            bool ok = res.TryGetValue("success", out var s) && (bool)s;
            if (!ok)
            {
                var reason = res.TryGetValue("reason", out var r) ? r.ToString() : "UNKNOWN";
                return RequestResult.Fail(reason);
            }

            // 3) Commit local
            PlayerSave.SetPlayerIcon(iconId);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Change player's frame: validate locally, call server, then apply locally.
        /// </summary>
        public static async UniTask<RequestResult> ChangePlayerFrameAsync(
            this WebSocketSqlBackendService svc,
            string frameId)
        {
            // 1) Local validation
            var item = GameInstance.Singleton.GetFrameItemById(frameId);
            if (item == null) return RequestResult.Fail("INVALID_FRAME");
            bool owned = item.isUnlocked || PlayerSave.IsFramePurchased(frameId);
            if (!owned) return RequestResult.Fail("NOT_OWNED");

            // 2) Server
            var payload = new Dictionary<string, object> { ["frameId"] = frameId };
            string raw = await svc.Auth.Http.Request("POST", "auth/profile/frame", payload);
            if (!raw.TrimStart().StartsWith("{"))
            {
                return RequestResult.Fail(raw.Trim('"'));
            }
            var res = Json.Deserialize<Dictionary<string, object>>(raw);
            bool ok = res.TryGetValue("success", out var s) && (bool)s;
            if (!ok)
            {
                var reason = res.TryGetValue("reason", out var r) ? r.ToString() : "UNKNOWN";
                return RequestResult.Fail(reason);
            }

            // 3) Commit local
            PlayerSave.SetPlayerFrame(frameId);
            return RequestResult.Ok();
        }
    }
}
