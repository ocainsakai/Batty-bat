#if UNITY_EDITOR
using UnityEngine;
using System.IO;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports all game data categories into JSON files for server-side validation.
    /// </summary>
    public static class GameDataExportAll
    {
        /// <summary>
        /// Exports all categories of game data into individual JSON files.
        /// </summary>
        [ContextMenu("Export All Game Data")]
        public static void ExportAll()
        {
            if (!Directory.Exists(ExportDirectory))
                Directory.CreateDirectory(ExportDirectory);

            ExportCharacterData();
            ExportIconItemData();
            ExportFrameItemData();
            ExportInventoryItemData();
            ExportCharacterTypeData();
            ExportMapInfoData();
            ExportQuestItemData();
            ExportCouponItemData();
            ExportProjectSettings();
            ExportCurrencyData();
            ExportShopItemData();
            ExportBattlePassItemData();
            ExportNewPlayerRewardData();
            ExportDailyRewardData();

            Debug.Log("All game data exported successfully!");
        }

        private static string ExportDirectory => Path.Combine(Application.dataPath, "ExportedData");

        private static void ExportCharacterData()
        {
            CharacterExporter.ExportCharacterData();
        }

        private static void ExportIconItemData()
        {
            IconAndFrameExporter.ExportIcons(ExportDirectory);
        }

        private static void ExportFrameItemData()
        {
            IconAndFrameExporter.ExportFrames(ExportDirectory);
        }

        private static void ExportInventoryItemData()
        {
            InventoryItemExporter.ExportInventoryItems();
        }

        private static void ExportCharacterTypeData()
        {
            CharacterTypeExporter.ExportCharacterTypes();
        }

        private static void ExportMapInfoData()
        {
            MapInfoExporter.ExportMapInfos();
        }

        private static void ExportQuestItemData()
        {
            QuestItemExporter.ExportQuests();
        }

        private static void ExportCouponItemData()
        {
            CouponItemExporter.ExportCoupons();
        }

        private static void ExportProjectSettings()
        {
            ProjectConfigExporter.ExportProjectConfig();
        }

        private static void ExportCurrencyData()
        {
            CurrencyDataExporter.ExportCurrencyData();
        }
        private static void ExportShopItemData()
        {
            ShopItemExporter.ExportShopItemData(ExportDirectory);
        }
        private static void ExportBattlePassItemData()
        {
            BattlePassItemExporter.ExportBattlePassItemData(ExportDirectory);
        }

        /// <summary>
        /// Exports <see cref="GameInstance.newPlayerRewardItems"/> to JSON for server-side validation.
        /// </summary>
        private static void ExportNewPlayerRewardData()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameDataExportAll: GameInstance not initialized (New Player Rewards).");
                return;
            }

            var list = GameInstance.Singleton.newPlayerRewardItems;
            if (list == null)
            {
                Debug.LogWarning("GameDataExportAll: newPlayerRewardItems is null – exporting empty array.");
                RewardItemExportUtility.ExportRewardItems(new RewardItem[0], ExportDirectory, "newPlayerRewards.json");
                return;
            }

            RewardItemExportUtility.ExportRewardItems(list, ExportDirectory, "newPlayerRewards.json");
        }

        /// <summary>
        /// Exports <see cref="GameInstance.dailyRewardItems"/> to JSON for server-side validation.
        /// </summary>
        private static void ExportDailyRewardData()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameDataExportAll: GameInstance not initialized (Daily Rewards).");
                return;
            }

            var list = GameInstance.Singleton.dailyRewardItems;
            if (list == null)
            {
                Debug.LogWarning("GameDataExportAll: dailyRewardItems is null – exporting empty array.");
                RewardItemExportUtility.ExportRewardItems(new RewardItem[0], ExportDirectory, "dailyRewards.json");
                return;
            }

            RewardItemExportUtility.ExportRewardItems(list, ExportDirectory, "dailyRewards.json");
        }
    }
}
#endif