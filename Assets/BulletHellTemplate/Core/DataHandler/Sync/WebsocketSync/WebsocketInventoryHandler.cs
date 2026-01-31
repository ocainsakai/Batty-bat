using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public static class WebsocketInventoryHandler
    {
        /* ------------------------------------------------------------------
          * UpgradeInventoryItemAsync
          * ------------------------------------------------------------------
          * Local pre-checks:
          *   - Item exists local? (else -> "1")
          *   - Not already max level? (-> "0")
          *   - Currency enough? (-> "1")
          * Server call only if passes all above.
          * On success: debit local currency, apply new level, refresh UI.
          * Error mapeado para 0/1/2.
          * ------------------------------------------------------------------ */
        public static async UniTask<RequestResult> UpgradeInventoryItemAsync(
            this WebSocketSqlBackendService svc,
            string uniqueItemGuid,
            InventoryItem inventorySO)
        {
            // ---------- Local lookups ----------
            var purchased = PlayerSave.GetInventoryItems()
                                      .Find(i => i.uniqueItemGuid == uniqueItemGuid);
            if (purchased == null)
            {
                // treat as "not owned / insufficient"
                return RequestResult.Fail("1");
            }

            // InventoryItem scriptable (def)
            var so = inventorySO ?? GameInstance.Singleton.GetInventoryItemById(purchased.itemId);
            if (so == null)
            {
                return RequestResult.Fail("0");
            }

            // Current + max levels
            int currentLevel = PlayerSave.GetItemUpgradeLevel(uniqueItemGuid);
            int maxLevel = so.itemUpgrades != null ? so.itemUpgrades.Count : 0;
            if (currentLevel >= maxLevel)
            {
                return RequestResult.Fail("0"); // max level
            }

            // Upgrade def for next level
            int upIdx = Mathf.Clamp(currentLevel, 0, maxLevel - 1);
            var upDef = so.itemUpgrades[upIdx];
            string coin = upDef.currencyTag;
            int cost = upDef.upgradeCosts;

            // Currency check
            int bal = MonetizationManager.GetCurrency(coin);
            if (bal < cost)
            {
                return RequestResult.Fail("1"); // insufficient currency
            }

            // ---------- Server call ----------
            var payload = new Dictionary<string, object> { ["guid"] = uniqueItemGuid };
            string raw;
            try
            {
                raw = await svc.Auth.Http.Request("POST", "auth/inventory/upgrade-item", payload);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UpgradeInventoryItemAsync] HTTP error: {e}");
                return RequestResult.Fail("2"); // generic fail
            }

            if (!raw.TrimStart().StartsWith("{"))
            {
                return RequestResult.Fail("2");
            }

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);

            // success?
            if (!(dto.TryGetValue("success", out var okObj) && okObj is bool ok && ok))
            {
                // Map server code -> our 0/1/2
                string mapped = MapServerFailCode(dto.TryGetValue("code", out var cObj) ? cObj?.ToString() : null);
                return RequestResult.Fail(mapped);
            }

            // ---------- Parse server newLevel ----------
            int newLevel = currentLevel;
            if (dto.TryGetValue("newLevel", out var nlObj))
            {
                if (nlObj is double d) newLevel = (int)d;
                else if (nlObj is int i) newLevel = i;
                else if (int.TryParse(nlObj?.ToString(), out var p)) newLevel = p;
            }
            else
            {
                newLevel = currentLevel + 1;
            }
            // ---------- Local currency debit ----------
            int balAfter = bal - cost;
            if (balAfter < 0) balAfter = 0; // clamp just in case
            MonetizationManager.SetCurrency(coin, balAfter);

            // ---------- Persist + UI ----------
            PlayerSave.SetItemUpgradeLevel(uniqueItemGuid, newLevel);
            UIInventoryMenu.Singleton?.UpdateInventoryUI();

            return RequestResult.Ok();
        }

        /// <summary>
        /// Normaliza códigos vindos do servidor para 0/1/2.
        /// </summary>
        private static string MapServerFailCode(string serverCode)
        {
            switch (serverCode)
            {
                // server "2" -> max level (map to "0")
                case "2": return "0";

                // server "3" -> insufficient currency (map to "1")
                // server "1" -> not owned ("1")
                case "3":
                case "1": return "1";

                // others -> generic "2"
                case "0": // invalid
                default: return "2";
            }
        }
    }
}