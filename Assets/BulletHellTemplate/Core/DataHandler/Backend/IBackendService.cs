using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.TextCore.Text;

namespace BulletHellTemplate
{
    public interface IBackendService
    {
        /*──────────────────── AUTH ────────────────────*/
        UniTask<RequestResult> AccountLogin(string email,string pass);
        UniTask<RequestResult> AccountRegister(string email, string pass, string confirmPass);
        UniTask<RequestResult> PlayAsGuest();
        UniTask<bool> TryAutoLoginAsync();
        UniTask Logout();
        /*──────────────────── LOAD ────────────────────*/
        UniTask<RequestResult> LoadAllAccountDataAsync();

        /*──────────────────── ACCOUNT ────────────────────*/
        UniTask<RequestResult> ChangePlayerNameAsync(string newName);
        UniTask<RequestResult> ChangePlayerIconAsync(string iconId);
        UniTask<RequestResult> ChangePlayerFrameAsync(string frameId);

        /*──────────────────── CURRENCY ───────────────────*/
        UniTask<RequestResult> InitCurrencyRechargeAsync();

        /*──────────────────── CHARACTERS & ITEMS ─────────*/      
        UniTask<RequestResult> UpdateSelectedCharacterAsync(int characterId);
        UniTask<RequestResult> UpdatePlayerCharacterFavouriteAsync(int characterId);
        UniTask<RequestResult> UnlockCharacterSkin(int characterId, int skinId);
        UniTask<RequestResult> UpdateCharacterSkin(int characterId, int skinId);
        UniTask<RequestResult> UpdateCharacterLevelUP(int characterId);
        UniTask<RequestResult> UpdateCharacterStatUpgradeAsync(StatUpgrade statUpgrade,CharacterStatsRuntime stats ,int characterId);

        /*──────────────────── MAP / SCORE ────────────────*/
        UniTask<RequestResult> CompleteGameSessionAsync(EndGameSessionData sessionData);
        /*──────────────────── QUESTS / COUPON ────────────*/
        UniTask<RequestResult> CompleteQuestAsync(int questId);
        UniTask<RequestResult> RefreshQuestLevelProgressAsync();
        UniTask<RequestResult> RedeemCouponAsync(string code);

        /*──────────────────── BATTLE-PASS ───────────────*/                 

        /*──────────────────── INVENTORY ───────────────────*/
        UniTask<RequestResult> UpgradeInventoryItemAsync(string uniqueItemGuid, InventoryItem inventorySO);
        UniTask<RequestResult> SetCharacterItemAsync(int characterId, string slotName, string uniqueItemGuid);     
        UniTask<RequestResult> DeletePurchasedInventoryItemAsync(string uniqueItemGuid);

        /*──────────────────── PURCHASING ───────────────────*/
        UniTask<RequestResult> PurchaseShopItemAsync(ShopItem shopItem);
        UniTask<RequestResult> UnlockBattlePassPremiumAsync();
        UniTask<RequestResult> ClaimBattlePassRewardAsync(BattlePassItem reward);

        /*──────────────────── REWARDS (daily/new) ───────*/
        UniTask<RequestResult> ClaimDailyRewardAsync(int day, BattlePassItem reward);
        UniTask<RequestResult> ClaimNewPlayerRewardAsync(int day, BattlePassItem reward);
        UniTask<RequestResult> ApplyMapRewardsAsync(MapCompletionRewardData mapCompletionData);

        /*──────────────────── LEADERBOARD ───────────────*/
        UniTask<List<Dictionary<string, object>>> GetTopPlayersAsync(int limit = 20);
        UniTask<int> GetPlayerRankAsync();
        /*──────────────────── DEBUG ───────────────*/
        UniTask<RequestResult> TestAddAccountExpAsync(int exp);
        UniTask<RequestResult> TestAddCharacterExpAsync(int cid, int exp);
        UniTask<RequestResult> TestAddCharacterMasteryExpAsync(int cid, int exp);
        UniTask<RequestResult> TestAddBattlePassXpAsync(int amount);
    }

    /// <summary>Uniform return for any attempt.</summary>
    public readonly struct RequestResult
    {
        public readonly bool Success;
        public readonly string Reason;     // “Not enough coins”, “Already owned”, etc.

        public static RequestResult Ok() => new(true, null);
        public static RequestResult Ok(string reasonMsg) => new(true, reasonMsg);
        public static RequestResult Fail() => new(false, null);
        public static RequestResult Fail(string reasonMsg) => new(false, reasonMsg);

        private RequestResult(bool success, string reason) { Success = success; Reason = reason; }
    }
}
