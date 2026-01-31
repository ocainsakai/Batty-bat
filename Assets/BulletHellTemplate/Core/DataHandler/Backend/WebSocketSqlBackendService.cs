using BulletHellTemplate;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public sealed class WebSocketSqlBackendService : IBackendService
    {
        private readonly WebsocketAuthHandler _auth;

        public WebsocketAuthHandler Auth => _auth;

        public WebSocketSqlBackendService(BackendSettings cfg)
        {
            _auth = new WebsocketAuthHandler(cfg.serverUrl);
        }

        /*──────────── AUTH ───────────*/
        public UniTask<RequestResult> AccountRegister(string email, string pass, string confirm) =>
         WebsocketDataLoadHandler.RegisterAndLoadAsync(this, email, pass, confirm);

        public UniTask<RequestResult> AccountLogin(string email, string pass) =>
            WebsocketDataLoadHandler.LoginAndLoadAsync(this, email, pass);

        public UniTask<RequestResult> PlayAsGuest() =>
            WebsocketDataLoadHandler.GuestAndLoadAsync(this);

        public UniTask<bool> TryAutoLoginAsync() =>
            WebsocketDataLoadHandler.TryAutoLoginAndLoadAsync(this);

        public UniTask Logout()
        {
            _auth.LogOut();
            return UniTask.CompletedTask;
        }

        /*──────────────────── LOAD ────────────────────*/
        public UniTask<RequestResult> LoadAllAccountDataAsync() =>
           WebsocketDataLoadHandler.ApplyToPlayerSaveAsync(this);

        /*──────────────────── ACCOUNT ────────────────────*/
        public async UniTask<RequestResult> ChangePlayerFrameAsync(string frameId) =>
            await WebsocketProfileHandler.ChangePlayerFrameAsync(this, frameId);

        public async UniTask<RequestResult> ChangePlayerIconAsync(string iconId) =>
            await WebsocketProfileHandler.ChangePlayerIconAsync(this, iconId);

        public async UniTask<RequestResult> ChangePlayerNameAsync(string newName) =>
            await WebsocketProfileHandler.ChangePlayerNameAsync(this, newName);

        /*──────────────────── CURRENCY ───────────────────*/

        /*──────────────────── CHARACTERS & ITEMS ─────────*/

        public UniTask<RequestResult> UpdateSelectedCharacterAsync(int characterId) =>
            WebsocketCharacterHandler.SelectCharacterAsync(this, characterId);

        public UniTask<RequestResult> UpdatePlayerCharacterFavouriteAsync(int characterId) =>
            WebsocketCharacterHandler.FavouriteCharacterAsync(this, characterId);

        public UniTask<RequestResult> UnlockCharacterSkin(int characterId, int skinId) =>
            WebsocketCharacterHandler.UnlockCharacterSkinAsync(this, characterId, skinId);

        public UniTask<RequestResult> UpdateCharacterSkin(int characterId, int skinId) =>
            WebsocketCharacterHandler.SetCharacterSkinAsync(this, characterId, skinId);

        public UniTask<RequestResult> UpdateCharacterLevelUP(int characterId) =>
            WebsocketCharacterHandler.LevelUpCharacterAsync(this, characterId);

        public UniTask<RequestResult> UpdateCharacterStatUpgradeAsync(StatUpgrade stat, CharacterStatsRuntime stats, int characterId) =>
            WebsocketCharacterHandler.TryUpgradeStatAsync(this, stat, stats, characterId);

        public UniTask<RequestResult> SetCharacterItemAsync(int characterId, string slotName, string uniqueItemGuid) =>
            WebsocketCharacterHandler.SetCharacterSlotItemAsync(this, characterId, slotName, uniqueItemGuid);

        /*──────────────────── PURCHASING ───────────────────*/

        public UniTask<RequestResult> PurchaseShopItemAsync(ShopItem shopItem) =>
            WebsocketPurchasesHandler.PurchaseShopItemAsync(this, shopItem);

        public UniTask<RequestResult> ClaimBattlePassRewardAsync(BattlePassItem reward) =>
            WebsocketPurchasesHandler.ClaimBattlePassRewardAsync(this, reward);

        public UniTask<RequestResult> DeletePurchasedInventoryItemAsync(string uniqueItemGuid) =>
           WebsocketCharacterHandler.DeletePurchasedItemAsync(this, uniqueItemGuid);
        /*──────────────────── MAP / SCORE ────────────────*/

        public UniTask<RequestResult> CompleteGameSessionAsync(EndGameSessionData data) =>
           WebsocketProgressHandler.CompleteGameSessionAsync(this, data);

        /*──────────────────── QUESTS / COUPON ────────────*/

        public UniTask<RequestResult> CompleteQuestAsync(int questId) =>
            WebsocketProgressHandler.CompleteQuestAsync(this, questId);

        public UniTask<RequestResult> RefreshQuestLevelProgressAsync() =>
            WebsocketProgressHandler.RefreshQuestLevelProgressAsync(this);

        public UniTask<RequestResult> RedeemCouponAsync(string code) =>
          WebsocketRewardsHandler.RedeemCouponAsync(this, code);

        /*──────────────────── BATTLE-PASS ───────────────*/

        public UniTask<RequestResult> UnlockBattlePassPremiumAsync() =>
            WebsocketPurchasesHandler.UnlockBattlePassPremiumAsync(this);

        /*──────────────────── INVENTORY ───────────────*/

        public UniTask<RequestResult> UpgradeInventoryItemAsync(string uniqueItemGuid, InventoryItem inventorySO) =>
            WebsocketInventoryHandler.UpgradeInventoryItemAsync(this, uniqueItemGuid, inventorySO);


        public UniTask<RequestResult> ApplyMapRewardsAsync(MapCompletionRewardData mapCompletionData) =>
            WebsocketRewardsHandler.ApplyMapRewardsAsync(this, mapCompletionData);

        public UniTask<RequestResult> InitCurrencyRechargeAsync()
        {
            throw new NotImplementedException();
        }
        /*──────────────────── REWARDS (daily/new) ───────*/
        public UniTask<RequestResult> ClaimDailyRewardAsync(int day, BattlePassItem reward) =>
           WebsocketRewardsHandler.ClaimDailyRewardAsync(this, day, reward);

        public UniTask<RequestResult> ClaimNewPlayerRewardAsync(int day, BattlePassItem reward) =>
            WebsocketRewardsHandler.ClaimNewPlayerRewardAsync(this, day, reward);

        /*──────────────────── LEADERBOARD ───────────────*/
        public UniTask<int> GetPlayerRankAsync() =>
            WebsocketProgressHandler.GetPlayerRankAsync(this);

        public UniTask<List<Dictionary<string, object>>> GetTopPlayersAsync(int limit = 20) =>
            WebsocketProgressHandler.GetTopPlayersAsync(this);

        /*──────────────────── DEBUG ───────────────*/
        public UniTask<RequestResult> TestAddAccountExpAsync(int exp) =>
        WebsocketExpHandler.AddAccountExpAsync(this, exp);

        public UniTask<RequestResult> TestAddCharacterExpAsync(int cid, int exp) =>
            WebsocketExpHandler.AddCharacterExpAsync(this, cid, exp);

        public UniTask<RequestResult> TestAddCharacterMasteryExpAsync(int cid, int exp) =>
            WebsocketExpHandler.AddCharacterMasteryExpAsync(this, cid, exp);

        public UniTask<RequestResult> TestAddBattlePassXpAsync(int amount) =>
            WebsocketExpHandler.AddBattlePassXpAsync(this, amount);


    }
}