using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    public static class OfflineRewardsHandler
    {
        public static RequestResult ClaimNewPlayerReward(int dayIndex, BattlePassItem reward)
        {
            NewPlayerRewardsData localNewPlayerData = PlayerSave.GetNewPlayerRewardsLocal();

            TimeSpan timeSinceCreation = DateTime.Now.Date - localNewPlayerData.accountCreationDate.Date;
            int currentDayIndex = (int)timeSinceCreation.TotalDays;

            if (localNewPlayerData.claimedRewards.Contains(dayIndex))
                return RequestResult.Fail("0"); // Already claimed

            if (dayIndex > currentDayIndex)
                return RequestResult.Fail("1"); // Not available yet

            localNewPlayerData.claimedRewards.Add(dayIndex);
            PlayerSave.SetNewPlayerRewardsLocal(localNewPlayerData);

            RequestResult rewardResult = OfflinePurchasesHandler.ClaimBattlePassReward(reward);
            if (!rewardResult.Success)
                return RequestResult.Fail("2");

            PlayerSave.SetNewPlayerRewardsLocal(localNewPlayerData);
            return RequestResult.Ok(); 
        }

        public static RequestResult ClaimDailyReward(int dayIndex, BattlePassItem reward)
        {
            DailyRewardsData localDailyData = PlayerSave.GetDailyRewardsLocal();

            TimeSpan timeSinceFirstClaim = DateTime.Now.Date - localDailyData.firstClaimDate.Date;
            int currentDayIndex = (int)timeSinceFirstClaim.TotalDays;

            if (localDailyData.claimedRewards.Contains(dayIndex))
                return RequestResult.Fail("0"); // Already claimed

            if (dayIndex > currentDayIndex)
                return RequestResult.Fail("1"); // Not available yet

            localDailyData.claimedRewards.Add(dayIndex);
            PlayerSave.SetDailyRewardsLocal(localDailyData);

            RequestResult rewardResult = OfflinePurchasesHandler.ClaimBattlePassReward(reward);
            if (!rewardResult.Success)
                return RequestResult.Fail("2"); // Reward failed to apply

            PlayerSave.SetDailyRewardsLocal(localDailyData);
            return RequestResult.Ok();
        }

        public static RequestResult ApplyMapRewards(MapCompletionRewardData data)
        {
            var mapData = GameInstance.Singleton.GetMapInfoDataById(data.mapId);
            if (mapData == null)
                return RequestResult.Fail("map_not_found");

            // Currency / EXP
            foreach (var reward in mapData.WinMapRewards)
            {
                if (reward.currency != null)
                {
                    int current = MonetizationManager.GetCurrency(reward.currency.coinID);
                    MonetizationManager.SetCurrency(reward.currency.coinID, current + reward.amount);
                }

                if (reward.accountExp > 0)
                {
                    UniTask.FromResult(OfflineExpHandler.AddAccountExp(reward.accountExp));
                }

                if (reward.characterExp > 0)
                {
                    UniTask.FromResult(OfflineExpHandler.AddCharacterExp(data.characterId, reward.characterExp));
                }

                if (reward.characterMasteryAmount > 0)
                {
                    UniTask.FromResult(OfflineExpHandler.AddCharacterMasteryExp(data.characterId, reward.characterMasteryAmount));                   
                }
            }
            // Special item
            var tempItem = CreateBattlePassItem(mapData);
            MonetizationManager.Singleton.ClaimBattlePassRewardAsync(tempItem);
            return RequestResult.Ok();
        }

        public static RequestResult RedeemCoupon(string code)
        {
            CouponItem coupon = GameInstance.Singleton.GetCouponItemById(code);

            if (coupon == null) return RequestResult.Fail("1");

            if (PlayerSave.IsCouponUsed(coupon.idCoupon))
                return RequestResult.Fail("0");

            // Apply reward
            string cid = coupon.currencyRewardId;
            int amount = coupon.currencyAmount;
            int cur = MonetizationManager.GetCurrency(cid);
            MonetizationManager.SetCurrency(cid, cur + amount, pushToBackend: false);

            // Mark used
            PlayerSave.MarkCouponAsUsed(coupon.idCoupon);

            // Encode reward in Reason for easy UI display
            return RequestResult.Ok($"{cid}|{amount}");
        }

        /// <summary>
        /// Grants the reward to the player via MonetizationManager or other systems,
        /// and returns a fully constructed BattlePassItem for visual or log purposes.
        /// </summary>
        public static BattlePassItem CreateBattlePassItem(MapInfoData reward)
        {
            // Create and configure the reward item object
            BattlePassItem tempPassItem = ScriptableObject.CreateInstance<BattlePassItem>();
            tempPassItem.passId = "Reward_" + reward.mapId;
            tempPassItem.itemTitle = reward.mapName;
            tempPassItem.itemDescription = reward.mapDescription ?? "Reward from New Player Rewards.";
            tempPassItem.itemIcon = reward.mapPreviewImage;
            tempPassItem.rewardTier = BattlePassItem.RewardTier.Free;

            switch (reward.rewardType)
            {                
                case MapRewardType.Icon:
                    tempPassItem.rewardType = BattlePassItem.RewardType.IconReward;
                    tempPassItem.iconReward = reward.iconItem;
                    break;

                case MapRewardType.Frame:
                    tempPassItem.rewardType = BattlePassItem.RewardType.FrameReward;
                    tempPassItem.frameReward = reward.frameItem;
                    break;

                case MapRewardType.Character:
                    tempPassItem.rewardType = BattlePassItem.RewardType.CharacterReward;
                    tempPassItem.characterData = new CharacterData[] { reward.characterData };
                    break;

                case MapRewardType.InventoryItem:
                    tempPassItem.rewardType = BattlePassItem.RewardType.InventoryItemReward;
                    tempPassItem.inventoryItems = new InventoryItem[] { reward.inventoryItem };
                    break;
            }
            return tempPassItem;
        }
    }
}
