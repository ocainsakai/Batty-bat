using Cysharp.Threading.Tasks;
using GameDevWare.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handler for all server purchase routes.
    /// Validates locally, sends to server, and if successful, persists locally
    /// (including inventory item GUIDs returned by the server).
    /// </summary>
    public static class WebsocketPurchasesHandler
    {
        /// <summary>
        /// Purchases a ShopItem from the server and applies all results locally (shop, icons, frames, characters, inventory).
        /// Expects the server to return a list of { templateItemId, uniqueItemGuid } in the "inventory" field.
        /// </summary>
        public static async UniTask<RequestResult> PurchaseShopItemAsync(
            this WebSocketSqlBackendService svc,
            ShopItem shopItem)
        {
            if (shopItem == null)
                return RequestResult.Fail("0");
            int bal = MonetizationManager.GetCurrency(shopItem.currency);
            if (bal < shopItem.price)
                return RequestResult.Fail("1");
            if (PlayerSave.IsShopItemPurchased(shopItem.itemId))
                return RequestResult.Fail("2");

            var payload = new Dictionary<string, object> { ["itemId"] = shopItem.itemId };
            string raw = await svc.Auth.Http.Request("POST", "auth/purchase/shop", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var resp = Json.Deserialize<PurchaseResponseDto>(raw);
            if (!resp.success)
                return RequestResult.Fail(resp.reason);

            MonetizationManager.SetCurrency(shopItem.currency, bal - shopItem.price, pushToBackend: false);
            PlayerSave.AddShopItem(new PurchasedShopItem { itemId = shopItem.itemId });

            if (!shopItem.isCurrencyPackage)
            {
                // icons
                foreach (var ic in shopItem.icons)
                    PlayerSave.AddIcon(new PurchasedIcon { iconId = ic.iconId.ToString() });
                // frames
                foreach (var fr in shopItem.frames)
                    PlayerSave.AddFrame(new PurchasedFrame { frameId = fr.frameId.ToString() });
                // chars
                foreach (var c in shopItem.characterData)
                    PlayerSave.AddCharacter(new PurchasedCharacter { characterId = c.characterId.ToString() });
                // inventory 
                if (resp.inventory != null)
                {
                    foreach (var inv in resp.inventory)
                    {
                        var newItem = new PurchasedInventoryItem
                        {
                            uniqueItemGuid = inv.uniqueItemGuid,
                            itemId = inv.templateItemId,
                            itemLevel = inv.itemLevel,
                            upgrades = new Dictionary<int, int>()
                        };
                        Debug.Log("item added : " + newItem.uniqueItemGuid);
                        PlayerSave.AddInventoryItem(newItem);
                    }
                }
            }
            else
            {
                foreach (var rwd in shopItem.currencyRewards)
                {
                    int old = MonetizationManager.GetCurrency(rwd.currency.coinID);
                    MonetizationManager.SetCurrency(rwd.currency.coinID, old + rwd.amount, pushToBackend: false);
                }
            }

            return RequestResult.Ok();
        }

        /// <summary>
        /// Claims a Battle Pass reward from the server and persists it locally
        /// (including returned inventory GUIDs).
        /// </summary>
        public static async UniTask<RequestResult> ClaimBattlePassRewardAsync(
            this WebSocketSqlBackendService svc,
            BattlePassItem reward)
        {
            if (reward == null)
                return RequestResult.Fail("0");

            var payload = new Dictionary<string, object> { ["passId"] = reward.passId };
            string raw = await svc.Auth.Http.Request("POST", "auth/purchase/battlepass/claim", payload);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            Debug.Log(raw);

            var resp = Json.Deserialize<BattlePassResponseDto>(raw);
            if (!resp.success)
                return RequestResult.Fail(resp.reason);

            switch (reward.rewardType)
            {
                case BattlePassItem.RewardType.CurrencyReward:
                    {
                        var cr = reward.currencyReward;
                        int old = MonetizationManager.GetCurrency(cr.currency.coinID);
                        MonetizationManager.SetCurrency(cr.currency.coinID, old + cr.amount, pushToBackend: false);
                    }
                    break;
                case BattlePassItem.RewardType.IconReward:
                    PlayerSave.AddIcon(new PurchasedIcon { iconId = reward.iconReward.iconId.ToString() });
                    break;
                case BattlePassItem.RewardType.FrameReward:
                    PlayerSave.AddFrame(new PurchasedFrame { frameId = reward.frameReward.frameId.ToString() });
                    break;
                case BattlePassItem.RewardType.CharacterReward:
                    foreach (var c in reward.characterData)
                        PlayerSave.AddCharacter(new PurchasedCharacter { characterId = c.characterId.ToString() });
                    break;
            }

            if (resp.inventory != null)
            {
                foreach (var inv in resp.inventory)
                {
                    var newItem = new PurchasedInventoryItem
                    {
                        uniqueItemGuid = inv.uniqueItemGuid,
                        itemId = inv.templateItemId,
                        itemLevel = inv.itemLevel,
                        upgrades = new Dictionary<int, int>()
                    };
                    Debug.Log("battlepass item added: " + newItem.uniqueItemGuid);
                    PlayerSave.AddInventoryItem(newItem);
                }
            }

            PlayerSave.MarkBattlePassReward(reward.passId);
            return RequestResult.Ok();
        }

        /// <summary>
        /// Unlocks Battle-Pass Premium (server call + local commit on success).
        /// Also deducts the Battle-Pass cost from local currency cache.
        /// </summary>
        public static async UniTask<RequestResult> UnlockBattlePassPremiumAsync(
            this WebSocketSqlBackendService svc)
        {
            // --- 1) Server call -------------------------------------------------
            var payload = new Dictionary<string, object>(); // empty body
            string raw = await svc.Auth.Http.Request("POST", "auth/purchase/battlepass/premium", payload);

            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail(raw.Trim('"'));

            var dto = Json.Deserialize<Dictionary<string, object>>(raw);

            if (!(dto.TryGetValue("success", out var ok) && (bool)ok))
            {
                return RequestResult.Fail(
                    dto.TryGetValue("reason", out var c) ? c.ToString() : "err"
                );
            }

            // --- 2) Local currency debit ---------------------------------------
            var cfg = GameInstance.Singleton;
            string coin = cfg.battlePassCurrencyID;
            int price = cfg.battlePassPrice;
            int cur = MonetizationManager.GetCurrency(coin);
            int nxt = cur - price;
            if (nxt < 0) nxt = 0; 

            MonetizationManager.SetCurrency(coin, nxt);
            PlayerSave.BattlePassPremiumUnlock();
            BattlePassManager.Singleton.SyncFromPlayerSave();

            return RequestResult.Ok();
        }

        /// <summary>
        /// Synchronizes all owned data from the server.
        /// </summary>
        public static async UniTask<RequestResult> RefreshOwnedDataAsync(
            this WebSocketSqlBackendService svc)
        {
            string raw = await svc.Auth.Http.Request("GET", "auth/purchase/owned", null);
            if (!raw.TrimStart().StartsWith("{"))
                return RequestResult.Fail("bad_resp");

            var dto = Json.Deserialize<OwnedDto>(raw);

            // shop
            var shop = new List<PurchasedShopItem>();
            foreach (var id in dto.shopItemIds)
                shop.Add(new PurchasedShopItem { itemId = id });
            PlayerSave.SetShopItems(shop);

            // icons
            var icons = new List<PurchasedIcon>();
            foreach (var id in dto.iconIds)
                icons.Add(new PurchasedIcon { iconId = id });
            PlayerSave.SetIcons(icons);

            // frames
            var frames = new List<PurchasedFrame>();
            foreach (var id in dto.frameIds)
                frames.Add(new PurchasedFrame { frameId = id });
            PlayerSave.SetFrames(frames);

            // characters
            var chars = new List<PurchasedCharacter>();
            foreach (var id in dto.characterIds)
                chars.Add(new PurchasedCharacter { characterId = id.ToString() });
            PlayerSave.SetCharacters(chars);

            // inventory
            var invs = new List<PurchasedInventoryItem>();
            foreach (var inv in dto.inventoryItems)
                invs.Add(new PurchasedInventoryItem
                {
                    uniqueItemGuid = inv.uniqueItemGuid,
                    itemId = inv.templateItemId,
                    itemLevel = inv.itemLevel,
                    upgrades = new Dictionary<int, int>()
                });
            PlayerSave.SetInventoryItems(invs);

            return RequestResult.Ok();
        }
    }
}
