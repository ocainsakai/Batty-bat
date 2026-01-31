#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the export of InventoryItem data into a JSON file for server use.
    /// Excludes translations and icon data to keep the payload lightweight.
    /// </summary>
    public static class InventoryItemExporter
    {
        /// <summary>
        /// Exports all InventoryItems from GameInstance into a simplified JSON format.
        /// </summary>
        public static void ExportInventoryItems()
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.inventoryItems == null)
            {
                Debug.LogError("GameInstance or inventoryItems not found.");
                return;
            }

            List<SerializableInventoryItem> exportList = new List<SerializableInventoryItem>();

            foreach (var item in GameInstance.Singleton.inventoryItems)
            {
                if (item == null)
                    continue;

                var serializableItem = new SerializableInventoryItem
                {
                    itemId = item.itemId,
                    title = item.title,
                    description = item.description,
                    category = item.category,
                    rarity = item.rarity.ToString(),
                    slot = item.slot,
                    isUnlocked = item.isUnlocked,
                    itemStats = item.itemStats,
                    itemUpgrades = item.itemUpgrades
                };

                exportList.Add(serializableItem);
            }

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);

            // Save to file inside Assets/ExportedData
            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "InventoryItems.json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Inventory items exported successfully to {filePath}");
        }

        /// <summary>
        /// Represents a simplified version of InventoryItem for JSON export.
        /// </summary>
        private class SerializableInventoryItem
        {
            public string itemId;
            public string title;
            public string description;
            public string category;
            public string rarity;
            public string slot;
            public bool isUnlocked;
            public CharacterStats itemStats;
            public List<ItemUpgrade> itemUpgrades;
        }
    }
}
#endif