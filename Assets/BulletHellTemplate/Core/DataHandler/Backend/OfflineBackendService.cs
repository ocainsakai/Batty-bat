using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace BulletHellTemplate
{
    public sealed class OfflineBackendService : IBackendService
    {
        /*──────────────────── AUTH ────────────────────*/
        public UniTask<RequestResult> AccountLogin(string email, string pass) => throw new NotImplementedException();      
        public UniTask<RequestResult> AccountRegister(string email, string pass, string confirmPass) => throw new NotImplementedException();
        public UniTask<RequestResult> PlayAsGuest() => UniTask.FromResult(RequestResult.Ok());
        public async UniTask<bool> TryAutoLoginAsync()
        {
            var playerName = PlayerSave.GetPlayerName();
            if (string.IsNullOrWhiteSpace(playerName))
            {
                string guestName = $"Guest-{UnityEngine.Random.Range(10000, 99999)}";
                PlayerSave.SetPlayerName(guestName);

                var unlockedIcon = GameInstance.Singleton.iconItems.FirstOrDefault(icon => icon.isUnlocked);
                if (unlockedIcon != null)
                    PlayerSave.SetPlayerIcon(unlockedIcon.iconId);

                var unlockedFrame = GameInstance.Singleton.frameItems.FirstOrDefault(frame => frame.isUnlocked);
                if (unlockedFrame != null)
                    PlayerSave.SetPlayerFrame(unlockedFrame.frameId);

                foreach (var currency in GameInstance.Singleton.currencyData)
                {
                    MonetizationManager.SetCurrency(currency.coinID, currency.initialAmount);
                }
            }
            await UniTask.Yield(); 
            return true;
        }
        public UniTask Logout() => throw new NotImplementedException();

        /*──────────────────── LOAD ────────────────────*/
        public UniTask<RequestResult> LoadAllAccountDataAsync() => UniTask.FromResult(RequestResult.Ok());
        /*──────────────────── ACCOUNT ────────────────────*/
        public async UniTask<RequestResult> ChangePlayerNameAsync(string newName) => 
            await UniTask.FromResult(OfflineProfileHandler.ChangePlayerName(newName));

        public async UniTask<RequestResult> ChangePlayerIconAsync(string iconId) =>
            await UniTask.FromResult(OfflineProfileHandler.ChangePlayerIcon(iconId));       

        public async UniTask<RequestResult> ChangePlayerFrameAsync(string frameId) =>
            await UniTask.FromResult(OfflineProfileHandler.ChangePlayerFrame(frameId));
        
        /*──────────────────── CURRENCY ───────────────────*/
        public async UniTask<RequestResult> InitCurrencyRechargeAsync() => await OfflineRechargeHandler.StartAsync();       

        /*──────────────────── CHAR / ITEM STATE ─────────*/     
        public async UniTask<RequestResult> UpdateSelectedCharacterAsync(int cid) => await UniTask.FromResult(OfflineCharacterHandler.TrySelectCharacterAsync(cid));
        public async UniTask<RequestResult> UpdatePlayerCharacterFavouriteAsync(int cid) => await UniTask.FromResult(OfflineCharacterHandler.UpdatePlayerCharacterFavouriteAsync(cid));
        public async UniTask<RequestResult> UnlockCharacterSkin(int characterId, int skinId) => await UniTask.FromResult(OfflineCharacterHandler.TryUnlockCharacterSkin(characterId, skinId));
        public async UniTask<RequestResult> UpdateCharacterSkin(int characterId, int skinId) => await UniTask.FromResult(OfflineCharacterHandler.UpdateCharacterSkin(characterId, skinId));
        public async UniTask<RequestResult> UpdateCharacterLevelUP(int characterId) => await UniTask.FromResult(OfflineCharacterHandler.TryCharacterLevelUp(characterId));
        public UniTask<RequestResult> UpdateCharacterStatUpgradeAsync(StatUpgrade stat, CharacterStatsRuntime stats, int cid) => UniTask.FromResult(OfflineCharacterHandler.TryUpgradeStat(stat, stats, cid));

        /*──────────────────── SECURE PURCHASES ──────────*/
        public async UniTask<RequestResult> PurchaseShopItemAsync(ShopItem shopItem) =>
           await UniTask.FromResult(OfflinePurchasesHandler.PurchaseShopItem(shopItem));

        public async UniTask<RequestResult> ClaimBattlePassRewardAsync(BattlePassItem reward) =>
            await UniTask.FromResult(OfflinePurchasesHandler.ClaimBattlePassReward(reward));

        public async UniTask<RequestResult> UnlockBattlePassPremiumAsync()
            => await UniTask.FromResult(
                OfflinePurchasesHandler.TryUnlockBattlePassPremium(
                
                ));

        /*──────────────────── MAP / SCORE ───────────────*/
        public async UniTask<RequestResult> CompleteGameSessionAsync(EndGameSessionData sessionData) => 
            await UniTask.FromResult(OfflineProgressHandler.CompleteGameSession(sessionData));

        /*──────────────────── QUESTS / COUPON ───────────*/
        public async UniTask<RequestResult> CompleteQuestAsync(int questId) =>
            await UniTask.FromResult(OfflineProgressHandler.TryCompleteQuest(questId));

        public async UniTask<RequestResult> RefreshQuestLevelProgressAsync()
        {
            OfflineProgressHandler.RefreshLevelBasedProgress();
            return await UniTask.FromResult(RequestResult.Ok());
        }
        public async UniTask<RequestResult> RedeemCouponAsync(string code) => 
            await UniTask.FromResult(OfflineRewardsHandler.RedeemCoupon(code));

        /*──────────────────── INVENTORY ───────────────────*/

        public async UniTask<RequestResult> UpgradeInventoryItemAsync(string uniqueItemGuid, InventoryItem inventorySO) =>
            await UniTask.FromResult(OfflinePurchasesHandler.TryUpgradeItem(uniqueItemGuid,inventorySO));      

        public async UniTask<RequestResult> SetCharacterItemAsync(int characterId, string slotName,string uniqueItemGuid) =>
           await UniTask.FromResult(OfflineCharacterHandler.SetCharacterSlotItem(characterId, slotName, uniqueItemGuid));
        
        public async UniTask<RequestResult> DeletePurchasedInventoryItemAsync(string uniqueItemGuid) =>
            await UniTask.FromResult(OfflineCharacterHandler.DeletePurchasedItem(uniqueItemGuid));            

        /*──────────────────── BATTLE-PASS ───────────────*/
        public async UniTask<DateTime> GetSeasonEndUtcAsync() => await UniTask.FromResult(OfflineProgressHandler.GetSeasonEndUtc());

        /*──────────────────── REWARDS ───────────────────*/
        public async UniTask<RequestResult> ClaimDailyRewardAsync(int day, BattlePassItem reward)
        {
            RequestResult result = OfflineRewardsHandler.ClaimDailyReward(day, reward);
            return await UniTask.FromResult(result);
        }
        public async UniTask<RequestResult> ClaimNewPlayerRewardAsync(int day, BattlePassItem reward)
        {
            RequestResult result = OfflineRewardsHandler.ClaimNewPlayerReward(day, reward);
            return await UniTask.FromResult(result);
        }
        public async UniTask<RequestResult> ApplyMapRewardsAsync(MapCompletionRewardData mapCompletionData) =>  
             await UniTask.FromResult(OfflineRewardsHandler.ApplyMapRewards(mapCompletionData));
        
        /*──────────────────── LEADERBOARD ───────────────*/
        public async UniTask<int> GetCurrentSeasonAsync() => await UniTask.FromResult(1);
        public async UniTask<List<Dictionary<string, object>>> GetTopPlayersAsync(int limit = 20)
        {
            var r = new Dictionary<string, object>
            {
                ["PlayerName"] = PlayerSave.GetPlayerName(),
                ["PlayerIcon"] = PlayerSave.GetPlayerIcon(),
                ["PlayerFrame"] = PlayerSave.GetPlayerFrame(),
                ["PlayerCharacterFavourite"] = PlayerSave.GetFavouriteCharacter(),
                ["score"] = PlayerSave.GetScore()
            };
            return await UniTask.FromResult(new List<Dictionary<string, object>> { r });
        }
        public async UniTask<int> GetPlayerRankAsync() => await UniTask.FromResult(1);

        /*──────────────────── DEBUG ───────────────*/
        public async UniTask<RequestResult> TestAddAccountExpAsync(int exp)
            => await UniTask.FromResult(OfflineExpHandler.AddAccountExp(exp));

        public async UniTask<RequestResult> TestAddCharacterExpAsync(int cid, int exp)
            => await UniTask.FromResult(OfflineExpHandler.AddCharacterExp(cid, exp));

        public async UniTask<RequestResult> TestAddCharacterMasteryExpAsync(int cid, int exp)
            => await UniTask.FromResult(OfflineExpHandler.AddCharacterMasteryExp(cid, exp));

        public async UniTask<RequestResult> TestAddBattlePassXpAsync(int amount) 
            => await UniTask.FromResult(OfflineExpHandler.AddBattlePassXp(amount));

       
    }
}
