#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports all ShopItem data from GameInstance into a JSON file.
    /// </summary>
    public class ShopItemExporter : MonoBehaviour
    {
        /// <summary>
        /// Exports all ShopItem data to a JSON file.
        /// </summary>
        public void ExportShopItems()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameInstance is not initialized.");
                return;
            }

            string exportPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            ExportShopItemData(exportPath);

            Debug.Log("Shop item data exported successfully.");
        }

        /// <summary>
        /// Exports shop item data with all relevant IDs.
        /// </summary>
        public static void ExportShopItemData(string exportPath)
        {
            List<ShopItemExportData> shopList = new List<ShopItemExportData>();

            foreach (var item in GameInstance.Singleton.shopData)
            {
                if (item == null)
                    continue;

                ShopItemExportData exportData = new ShopItemExportData
                {
                    itemId = item.itemId,
                    itemTitle = item.itemTitle,
                    itemDescription = item.itemDescription,
                    price = item.price,
                    currency = item.currency,
                    category = item.category,
                    isCurrencyPackage = item.isCurrencyPackage,
                    iconIds = new List<string>(),
                    frameIds = new List<string>(),
                    characterIds = new List<string>(),
                    inventoryItemIds = new List<string>(),
                    currencyPackageRewards = new List<CurrencyRewardExport>()
                };

                if (item.icons != null)
                {
                    foreach (var icon in item.icons)
                    {
                        if (icon != null)
                            exportData.iconIds.Add(icon.iconId);
                    }
                }

                if (item.frames != null)
                {
                    foreach (var frame in item.frames)
                    {
                        if (frame != null)
                            exportData.frameIds.Add(frame.frameId);
                    }
                }

                if (item.characterData != null)
                {
                    foreach (var character in item.characterData)
                    {
                        if (character != null)
                            exportData.characterIds.Add(character.characterId.ToString());
                    }
                }

                if (item.inventoryItems != null)
                {
                    foreach (var invItem in item.inventoryItems)
                    {
                        if (invItem != null)
                            exportData.inventoryItemIds.Add(invItem.itemId);
                    }
                }

                if (item.isCurrencyPackage && item.currencyRewards != null)
                {
                    foreach (var reward in item.currencyRewards)
                    {
                        if (reward != null && reward.currency != null)
                        {
                            exportData.currencyPackageRewards.Add(new CurrencyRewardExport
                            {
                                coinId = reward.currency.coinID,
                                amount = reward.amount
                            });
                        }
                    }
                }

                shopList.Add(exportData);
            }

            string shopJson = JsonConvert.SerializeObject(shopList, Formatting.Indented);
            File.WriteAllText(Path.Combine(exportPath, "shopItems.json"), shopJson);
        }

        /// <summary>
        /// Structure for exported shop item data with unlockable content references.
        /// </summary>
        [System.Serializable]
        private class ShopItemExportData
        {
            public string itemId;
            public string itemTitle;
            public string itemDescription;
            public int price;
            public string currency;
            public string category;
            public bool isCurrencyPackage;
            public List<string> iconIds;
            public List<string> frameIds;
            public List<string> characterIds;
            public List<string> inventoryItemIds;
            public List<CurrencyRewardExport> currencyPackageRewards;
        }

        /// <summary>
        /// Structure for exported currency rewards (used when isCurrencyPackage is true).
        /// </summary>
        [System.Serializable]
        private class CurrencyRewardExport
        {
            public string coinId;
            public int amount;
        }
    }
}
#endif