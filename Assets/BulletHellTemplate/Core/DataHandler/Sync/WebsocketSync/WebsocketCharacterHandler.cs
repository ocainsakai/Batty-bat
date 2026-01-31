using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System.Collections.Generic;
using System.Linq;


namespace BulletHellTemplate
{
    public static class WebsocketCharacterHandler
    {
        /// <summary>
        /// Selects a character for play
        /// </summary>
        public static async UniTask<RequestResult> SelectCharacterAsync(
            this WebSocketSqlBackendService svc,
            int characterId)
        {
            var def = GameInstance.Singleton.GetCharacterDataById(characterId);
            if (def == null) return RequestResult.Fail("0");

            bool owned = def.CheckUnlocked || PlayerSave.IsCharacterPurchased(characterId.ToString());
            if (!owned) return RequestResult.Fail("1");

            var payload = new Dictionary<string, object> { ["characterId"] = characterId };
            string raw = await svc.Auth.Http.Request("POST", "auth/character/select", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            PlayerSave.SetSelectedCharacter(characterId);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Marks a character as favorite
        /// </summary>
        public static async UniTask<RequestResult> FavouriteCharacterAsync(
            this WebSocketSqlBackendService svc,
            int characterId)
        {

            var def = GameInstance.Singleton.GetCharacterDataById(characterId);
            if (def == null) return RequestResult.Fail("0");

            bool owned = def.CheckUnlocked || PlayerSave.IsCharacterPurchased(characterId.ToString());
            if (!owned) return RequestResult.Fail("1");

            var payload = new Dictionary<string, object> { ["characterId"] = characterId };
            string raw = await svc.Auth.Http.Request("POST", "auth/character/favourite", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            PlayerSave.SetFavouriteCharacter(characterId);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Unlocks a character skin
        /// </summary>
        public static async UniTask<RequestResult> UnlockCharacterSkinAsync(
            this WebSocketSqlBackendService svc,
            int characterId, int skinIndex)
        {
            var def = GameInstance.Singleton.GetCharacterDataById(characterId);
            if (def == null || def.characterSkins == null || skinIndex < 0 || skinIndex >= def.characterSkins.Count())
                return RequestResult.Fail("0");

            var skin = def.characterSkins[skinIndex];
            var unlockedList = PlayerSave.LoadCharacterUnlockedSkins(characterId);
            bool already = skin.isUnlocked || unlockedList.Contains(skinIndex);
            if (already) return RequestResult.Fail("2");

            // check currency
            int bal = MonetizationManager.GetCurrency(skin.unlockCurrencyId);
            if (bal < skin.unlockPrice) return RequestResult.Fail("1");

            var payload = new Dictionary<string, object>
            {
                ["characterId"] = characterId,
                ["skinIndex"] = skinIndex
            };
            string raw = await svc.Auth.Http.Request("POST", "auth/character/unlock-skin", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            // 3) commit local
            MonetizationManager.SetCurrency(skin.unlockCurrencyId, bal - skin.unlockPrice, pushToBackend: false);
            unlockedList.Add(skinIndex);
            PlayerSave.SaveCharacterUnlockedSkins(characterId, unlockedList);
            PlayerSave.SetCharacterSkin(characterId, skinIndex);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Sets an active skin
        /// </summary>
        public static async UniTask<RequestResult> SetCharacterSkinAsync(
            this WebSocketSqlBackendService svc,
            int characterId, int skinIndex)
        {

            var def = GameInstance.Singleton.GetCharacterDataById(characterId);
            var unlockedList = PlayerSave.LoadCharacterUnlockedSkins(characterId);
            if (def == null
                || !def.characterSkins[skinIndex].isUnlocked
                   && !unlockedList.Contains(skinIndex))
                return RequestResult.Fail("0");

            var payload = new Dictionary<string, object>
            {
                ["characterId"] = characterId,
                ["skinIndex"] = skinIndex
            };
            string raw = await svc.Auth.Http.Request("POST", "auth/character/set-skin", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            PlayerSave.SetCharacterSkin(characterId, skinIndex);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Attempts to upgrade a character stat: validates locally, sends request to server, and commits local save on success.
        /// </summary>
        public static async UniTask<RequestResult> TryUpgradeStatAsync(
            this WebSocketSqlBackendService svc,
            StatUpgrade statUpgrade,
            CharacterStatsRuntime stats,
            int characterId)
        {
         
            int currentLevel = PlayerSave.GetCharacterUpgradeLevel(characterId, statUpgrade.statType);
            int nextLevel = currentLevel + 1;

            if (currentLevel >= statUpgrade.upgradeMaxLevel)
                return RequestResult.Fail("0");

            int cost = statUpgrade.GetUpgradeCost(nextLevel);
            int balance = MonetizationManager.GetCurrency(statUpgrade.currencyTag);
            if (balance < cost)
                return RequestResult.Fail("1");

            string statType = statUpgrade.statType.ToString();
            string endpoint = $"auth/character/{characterId}/upgrade/{statType}";
            string raw = await svc.Auth.Http.Request("POST", endpoint, null);

            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            MonetizationManager.SetCurrency(statUpgrade.currencyTag, balance - cost, pushToBackend: false);
            statUpgrade.ApplyUpgrade(stats, nextLevel);
            PlayerSave.SetCharacterUpgradeLevel(characterId, statUpgrade.statType, nextLevel);

            return RequestResult.Ok();
        }

        /// <summary>
        /// Performs character level-up
        /// </summary>
        public static async UniTask<RequestResult> LevelUpCharacterAsync(
            this WebSocketSqlBackendService svc,
            int characterId)
        {
            var def = GameInstance.Singleton.GetCharacterDataById(characterId);
            if (def == null) return RequestResult.Fail("0");

            int lvl = PlayerSave.GetCharacterLevel(characterId);
            int curExp = PlayerSave.GetCharacterCurrentExp(characterId);
            if (lvl >= def.maxLevel) return RequestResult.Fail("3");

            int reqExp = def.expPerLevel[lvl];
            int cost = def.upgradeCostPerLevel[lvl];
            int bal = MonetizationManager.GetCurrency(def.currencyId);
            if (curExp < reqExp) return RequestResult.Fail("1");
            if (bal < cost) return RequestResult.Fail("2");

            var payload = new Dictionary<string, object> { ["characterId"] = characterId };
            string raw = await svc.Auth.Http.Request("POST", "auth/character/levelup", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            MonetizationManager.SetCurrency(def.currencyId, bal - cost, pushToBackend: false);
            PlayerSave.SetCharacterCurrentExp(characterId, curExp - reqExp);
            PlayerSave.SetCharacterLevel(characterId, lvl + 1);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Equips/unequips an inventory item: validates local, calls server, applies local.
        /// </summary>
        public static async UniTask<RequestResult> SetCharacterSlotItemAsync(
            this WebSocketSqlBackendService svc,
            int characterId,
            string slotName,
            string uniqueGuid)
        {
            // Local validation: if equipping, make sure item exists in local inventory
            if (!string.IsNullOrEmpty(uniqueGuid))
            {
                var inv = PlayerSave.GetInventoryItems().Find(i => i.uniqueItemGuid == uniqueGuid);
                if (inv == null) return RequestResult.Fail("0");
            }
            // *** IMPORTANT: backend expects 'guid' (not uniqueGuid). Send both for safety. ***
            var payload = new Dictionary<string, object>
            {
                ["characterId"] = characterId,
                ["slotName"] = slotName,
                ["guid"] = uniqueGuid ?? "",
                ["uniqueGuid"] = uniqueGuid ?? "" // backward compat, can remove later
            };

            string raw = await svc.Auth.Http.Request("POST", "auth/character/slot", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            // Local apply
            PlayerSave.SetCharacterSlotItem(characterId, slotName, uniqueGuid);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Deletes a purchased inventory item (server first, then local).
        /// </summary>
        public static async UniTask<RequestResult> DeletePurchasedItemAsync(
            this WebSocketSqlBackendService svc,
            string uniqueGuid)
        {
            string raw;
            try
            {
                raw = await svc.Auth.Http.Request("DELETE", $"auth/character/inventory/{uniqueGuid}", null);
            }
            catch
            {
                var payload = new Dictionary<string, object> { ["guid"] = uniqueGuid };
                raw = await svc.Auth.Http.Request("POST", "auth/character/delete-item", payload);
            }

            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);
            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
                return RequestResult.Fail(dto.TryGetValue("code", out var c) ? c.ToString() : "err");

            PlayerSave.RemoveInventoryItem(uniqueGuid);
            return RequestResult.Ok();
        }

    }
}
