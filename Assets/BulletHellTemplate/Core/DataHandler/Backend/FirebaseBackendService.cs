using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Thin Firebase-backed IBackendService that delegates work to handler classes.
    /// </summary>
    public sealed class FirebaseBackendService : 
#if FIREBASE
        IBackendService
#else
        MonoBehaviour
#endif
    {
#if FIREBASE
        /*──────────────────── AUTH ───────────────*/
        public UniTask<RequestResult> AccountLogin(string email, string pass) =>
            FirebaseAuthHandler.LoginWithEmailAsync(email, pass);

        public UniTask<RequestResult> AccountRegister(string email, string pass, string confirmPass) =>
            FirebaseAuthHandler.RegisterWithEmailAsync(email, pass, confirmPass);

        public UniTask<RequestResult> PlayAsGuest() =>
            FirebaseAuthHandler.SignInAnonymouslyAsync();

        public UniTask<bool> TryAutoLoginAsync() =>
            FirebaseAuthHandler.TryAutoLoginAsync();

        public UniTask Logout() =>
            FirebaseAuthHandler.LogoutAsync();

        public UniTask<RequestResult> LoadAllAccountDataAsync() =>
            FirebaseAuthHandler.LoadAllAccountDataAsync();

        /*──────────────────── PROFILE ───────────────*/
        public UniTask<RequestResult> ChangePlayerNameAsync(string newName) =>
            FirebaseProfileHandler.ChangePlayerNameAsync(newName);

        public UniTask<RequestResult> ChangePlayerIconAsync(string iconId) =>
            FirebaseProfileHandler.ChangePlayerIconAsync(iconId);

        public UniTask<RequestResult> ChangePlayerFrameAsync(string frameId) =>
            FirebaseProfileHandler.ChangePlayerFrameAsync(frameId);

        /*──────────────────── PURCHASES ───────────────*/
        public UniTask<RequestResult> PurchaseShopItemAsync(ShopItem shopItem) =>
            FirebasePurchasesHandler.PurchaseShopItemAsync(shopItem);

        public UniTask<RequestResult> UpgradeInventoryItemAsync(string uniqueItemGuid, InventoryItem inventorySO) =>
            FirebasePurchasesHandler.UpgradeInventoryItemAsync(uniqueItemGuid, inventorySO);

        public UniTask<RequestResult> DeletePurchasedInventoryItemAsync(string uniqueItemGuid) =>
            FirebasePurchasesHandler.DeletePurchasedInventoryItemAsync(uniqueItemGuid);

        public UniTask<RequestResult> UnlockBattlePassPremiumAsync() =>
            FirebasePurchasesHandler.TryUnlockBattlePassPremiumAsync();

        public UniTask<RequestResult> ClaimBattlePassRewardAsync(BattlePassItem reward) =>
            FirebasePurchasesHandler.ClaimBattlePassRewardAsync(reward);

        public UniTask<RequestResult> CompleteGameSessionAsync(EndGameSessionData sessionData) =>
        FirebaseProgressHandler.CompleteGameSessionAsync(sessionData);

        public UniTask<RequestResult> CompleteQuestAsync(int questId) =>
            FirebaseProgressHandler.TryCompleteQuestAsync(questId);

        public UniTask<RequestResult> RefreshQuestLevelProgressAsync() =>
            FirebaseProgressHandler.RefreshLevelBasedProgressAsync();

        /*──────────────────── CHAR / ITEM STATE ─────────*/
        public UniTask<RequestResult> UpdateSelectedCharacterAsync(int characterId) =>
            FirebaseCharacterHandler.TrySelectCharacterAsync(characterId);

        public UniTask<RequestResult> UpdatePlayerCharacterFavouriteAsync(int characterId) =>
            FirebaseCharacterHandler.UpdatePlayerCharacterFavouriteAsync(characterId);

        public UniTask<RequestResult> UnlockCharacterSkin(int characterId, int skinId) =>
            FirebaseCharacterHandler.TryUnlockCharacterSkinAsync(characterId, skinId);

        public UniTask<RequestResult> UpdateCharacterSkin(int characterId, int skinId) =>
            FirebaseCharacterHandler.UpdateCharacterSkinAsync(characterId, skinId);

        public UniTask<RequestResult> UpdateCharacterLevelUP(int characterId) =>
            FirebaseCharacterHandler.TryCharacterLevelUpAsync(characterId);

        public UniTask<RequestResult> UpdateCharacterStatUpgradeAsync(StatUpgrade stat, CharacterStatsRuntime stats, int characterId) =>
            FirebaseCharacterHandler.TryUpgradeStatAsync(stat, stats, characterId);

        public UniTask<RequestResult> SetCharacterItemAsync(int characterId, string slotName, string uniqueItemGuid) =>
            FirebaseCharacterHandler.SetCharacterSlotItemAsync(characterId, slotName, uniqueItemGuid);

        public UniTask<int> GetPlayerRankAsync() =>
            FirebaseRankingHandler.GetPlayerRankAsync();

        public UniTask<List<Dictionary<string, object>>> GetTopPlayersAsync(int limit = 20) =>
            FirebaseRankingHandler.GetTopPlayersAsync(limit);
        public UniTask<RequestResult> InitCurrencyRechargeAsync() => UniTask.FromResult(RequestResult.Fail("not_implemented"));
       
        /*──────────────────── REWARDS ───────────────*/
        public UniTask<RequestResult> ClaimDailyRewardAsync(int day, BattlePassItem reward) =>
            FirebaseRewardsHandler.ClaimDailyRewardAsync(day, reward);

        public UniTask<RequestResult> ClaimNewPlayerRewardAsync(int day, BattlePassItem reward) =>
            FirebaseRewardsHandler.ClaimNewPlayerRewardAsync(day, reward);

        public UniTask<RequestResult> ApplyMapRewardsAsync(MapCompletionRewardData data) =>
            FirebaseRewardsHandler.ApplyMapRewardsAsync(data);

        public UniTask<RequestResult> RedeemCouponAsync(string code) =>
            FirebaseRewardsHandler.RedeemCouponAsync(code);

        /*──────────────────── DEBUG ───────────────*/
        public UniTask<RequestResult> TestAddAccountExpAsync(int exp) =>
            FirebaseExpHandler.AddAccountExpAsync(exp);

        public UniTask<RequestResult> TestAddCharacterExpAsync(int cid, int exp) =>
            FirebaseExpHandler.AddCharacterExpAsync(cid, exp);

        public UniTask<RequestResult> TestAddCharacterMasteryExpAsync(int cid, int exp) =>
            FirebaseExpHandler.AddCharacterMasteryExpAsync(cid, exp);

        public UniTask<RequestResult> TestAddBattlePassXpAsync(int amount) =>
            FirebaseExpHandler.AddBattlePassXpAsync(amount);
#endif
    }
}
