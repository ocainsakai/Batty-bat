#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports all Battle Pass items from GameInstance into a JSON file.
    /// </summary>
    public class BattlePassItemExporter : MonoBehaviour
    {
        /// <summary>
        /// Initiates the export process for Battle Pass items.
        /// </summary>
        public void ExportBattlePassItems()
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.battlePassData == null)
            {
                Debug.LogError("GameInstance or battlePassData is not initialized.");
                return;
            }

            string exportPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            ExportBattlePassItemData(exportPath);
            Debug.Log("Battle pass data exported successfully.");
        }

        /// <summary>
        /// Exports Battle Pass item data to the specified directory.
        /// </summary>
        /// <param name="exportPath">The directory path to save the JSON file.</param>
        public static void ExportBattlePassItemData(string exportPath)
        {
            List<BattlePassItemExportData> exportList = new List<BattlePassItemExportData>();

            foreach (var passItem in GameInstance.Singleton.battlePassData)
            {
                if (passItem == null)
                    continue;

                var data = new BattlePassItemExportData
                {
                    passId = passItem.passId,
                    itemTitle = passItem.itemTitle,
                    itemDescription = passItem.itemDescription,
                    rewardType = passItem.rewardType.ToString(),
                    rewardTier = passItem.rewardTier.ToString(),
                    characterRewardIds = new List<string>(),
                    currencyReward = null,
                    iconRewardId = null,
                    frameRewardId = null,
                    inventoryRewardIds = new List<string>()
                };

                // Character rewards
                if (passItem.rewardType == BattlePassItem.RewardType.CharacterReward && passItem.characterData != null)
                {
                    foreach (var character in passItem.characterData)
                    {
                        if (character != null)
                            data.characterRewardIds.Add(character.characterId.ToString());
                    }
                }

                // Currency reward
                if (passItem.rewardType == BattlePassItem.RewardType.CurrencyReward && passItem.currencyReward != null)
                {
                    data.currencyReward = new CurrencyRewardExport
                    {
                        coinId = passItem.currencyReward.currency.coinID,
                        amount = passItem.currencyReward.amount
                    };
                }

                // Icon reward
                if (passItem.rewardType == BattlePassItem.RewardType.IconReward && passItem.iconReward != null)
                {
                    data.iconRewardId = passItem.iconReward.iconId;
                }

                // Frame reward
                if (passItem.rewardType == BattlePassItem.RewardType.FrameReward && passItem.frameReward != null)
                {
                    data.frameRewardId = passItem.frameReward.frameId;
                }

                // Inventory items reward
                if (passItem.rewardType == BattlePassItem.RewardType.InventoryItemReward && passItem.inventoryItems != null)
                {
                    foreach (var inv in passItem.inventoryItems)
                    {
                        if (inv != null)
                            data.inventoryRewardIds.Add(inv.itemId);
                    }
                }

                exportList.Add(data);
            }

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);
            File.WriteAllText(Path.Combine(exportPath, "battlePassItems.json"), json);
        }

        /// <summary>
        /// Structure for exporting Battle Pass item data.
        /// </summary>
        [Serializable]
        private class BattlePassItemExportData
        {
            public string passId;
            public string itemTitle;
            public string itemDescription;
            public string rewardType;
            public string rewardTier;
            public List<string> characterRewardIds;
            public CurrencyRewardExport currencyReward;
            public string iconRewardId;
            public string frameRewardId;
            public List<string> inventoryRewardIds;
        }

        /// <summary>
        /// Structure for exporting currency reward details.
        /// </summary>
        [Serializable]
        private class CurrencyRewardExport
        {
            public string coinId;
            public int amount;
        }
    }
}
#endif