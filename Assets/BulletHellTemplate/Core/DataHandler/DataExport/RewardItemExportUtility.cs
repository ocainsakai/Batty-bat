#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Shared export helpers for converting <see cref="RewardItem"/> assets into a
    /// lightweight JSON schema suitable for backend validation.
    /// </summary>
    public static class RewardItemExportUtility
    {
        /// <summary>
        /// Converts and writes a list of <see cref="RewardItem"/> to JSON.
        /// </summary>
        /// <param name="items">List of reward items to export.</param>
        /// <param name="exportDirectory">Directory path; will be created if missing.</param>
        /// <param name="fileName">JSON filename, e.g. \"newPlayerRewards.json\".</param>
        public static void ExportRewardItems(
            IList<RewardItem> items,
            string exportDirectory,
            string fileName)
        {
            if (items == null)
            {
                Debug.LogError("RewardItemExportUtility.ExportRewardItems: items == null.");
                return;
            }

            if (string.IsNullOrEmpty(exportDirectory))
            {
                Debug.LogError("RewardItemExportUtility.ExportRewardItems: exportDirectory invalid.");
                return;
            }

            try
            {
                if (!Directory.Exists(exportDirectory))
                    Directory.CreateDirectory(exportDirectory);
            }
            catch (Exception e)
            {
                Debug.LogError($"RewardItemExportUtility: failed to create directory: {e}");
                return;
            }

            var exportList = ConvertRewardItems(items);

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);
            string path = Path.Combine(exportDirectory, fileName);

            try
            {
                File.WriteAllText(path, json);
                Debug.Log($"RewardItemExportUtility: wrote {items.Count} items to: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"RewardItemExportUtility: failed to write file: {e}");
            }
        }

        /// <summary>
        /// Converts a list of <see cref="RewardItem"/> into serializable DTOs.
        /// </summary>
        public static List<RewardItemExportData> ConvertRewardItems(IList<RewardItem> items)
        {
            var list = new List<RewardItemExportData>(items.Count);

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var dto = new RewardItemExportData
                {
                    rewardId = item.rewardId,
                    title = item.title,
                    description = item.description,
                    rewardType = item.rewardType.ToString(),
                    amount = item.amount,
                    currencyRewards = new List<CurrencyRewardDTO>(),
                    iconRewardIds = new List<string>(),
                    frameRewardIds = new List<string>(),
                    characterRewardIds = new List<string>(),
                    inventoryItemIds = new List<string>()
                };

                // Currency rewards
                if (item.currencyRewards != null)
                {
                    foreach (var c in item.currencyRewards)
                    {
                        if (c == null || c.coinID == null) continue;
                        dto.currencyRewards.Add(new CurrencyRewardDTO
                        {
                            coinId = c.coinID,
                            amount = dto.amount,
                        });
                    }
                }

                // Icon rewards
                if (item.iconRewards != null)
                {
                    foreach (var icon in item.iconRewards)
                    {
                        if (icon == null) continue;
                        dto.iconRewardIds.Add(icon.iconId);
                    }
                }

                // Frame rewards
                if (item.frameRewards != null)
                {
                    foreach (var frame in item.frameRewards)
                    {
                        if (frame == null) continue;
                        dto.frameRewardIds.Add(frame.frameId);
                    }
                }

                // Character rewards
                if (item.characterRewards != null)
                {
                    foreach (var ch in item.characterRewards)
                    {
                        if (ch == null) continue;
                        dto.characterRewardIds.Add(ch.characterId.ToString());
                    }
                }

                // Inventory item rewards
                if (item.inventoryItems != null)
                {
                    foreach (var inv in item.inventoryItems)
                    {
                        if (inv == null) continue;
                        dto.inventoryItemIds.Add(inv.itemId);
                    }
                }

                list.Add(dto);
            }

            return list;
        }

        /*────────────────────────── DTOs ──────────────────────────*/

        [Serializable]
        public sealed class RewardItemExportData
        {
            public string rewardId;
            public string title;
            public string description;
            public string rewardType;
            public int amount;

            public List<CurrencyRewardDTO> currencyRewards;
            public List<string> iconRewardIds;
            public List<string> frameRewardIds;
            public List<string> characterRewardIds;
            public List<string> inventoryItemIds;
        }

        [Serializable]
        public sealed class CurrencyRewardDTO
        {
            public string coinId;
            public int amount;
        }
    }
}
#endif