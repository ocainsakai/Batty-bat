#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the export of MapInfoData into a simplified JSON format for server use.
    /// Excludes translations, sprites, and Unity-specific references to keep the payload clean.
    /// </summary>
    public static class MapInfoExporter
    {
        /// <summary>
        /// Exports all MapInfoData from GameInstance into a simplified JSON format.
        /// </summary>
        public static void ExportMapInfos()
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.mapInfoData == null)
            {
                Debug.LogError("GameInstance or mapInfoData not found.");
                return;
            }

            List<SerializableMapInfo> exportList = new List<SerializableMapInfo>();

            foreach (var mapData in GameInstance.Singleton.mapInfoData)
            {
                if (mapData == null)
                    continue;

                var serializableMap = new SerializableMapInfo
                {
                    scene = mapData.scene,
                    mapId = mapData.mapId,
                    isUnlocked = mapData.isUnlocked,
                    mapName = mapData.mapName,
                    mapDescription = mapData.mapDescription,
                    difficultyRating = mapData.difficultyRating,
                    isRewardOnCompleteFirstTime = mapData.isRewardOnCompleteFirstTime,
                    rewards = ConvertRewards(mapData.WinMapRewards),
                    rewardType = mapData.rewardType.ToString(),
                    iconItemId = mapData.iconItem != null ? mapData.iconItem.iconId : null,
                    frameItemId = mapData.frameItem != null ? mapData.frameItem.frameId : null,
                    characterId = mapData.characterData != null ? mapData.characterData.characterId : (int?)null,
                    inventoryItemId = mapData.inventoryItem != null ? mapData.inventoryItem.itemId : null,
                    isNeedCurrency = mapData.isNeedCurrency,
                    currencyId = mapData.currency != null ? mapData.currency.coinID : null,
                    amount = mapData.amount,
                    canIgnoreMap = mapData.canIgnoreMap,
                    isEventMap = mapData.isEventMap,
                    eventIdName = mapData.eventIdName
                };

                exportList.Add(serializableMap);
            }

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);

            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "MapInfo.json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Map infos exported successfully to {filePath}");
        }

        private static List<SerializableMapReward> ConvertRewards(List<MapRewards> rewards)
        {
            List<SerializableMapReward> rewardList = new List<SerializableMapReward>();
            if (rewards != null)
            {
                foreach (var reward in rewards)
                {
                    rewardList.Add(new SerializableMapReward
                    {
                        currencyId = reward.currency != null ? reward.currency.coinID : null,
                        amount = reward.amount,
                        accountExp = reward.accountExp,
                        characterExp = reward.characterExp,
                        characterMasteryAmount = reward.characterMasteryAmount
                    });
                }
            }
            return rewardList;
        }

        /// <summary>
        /// Simplified data structure for MapInfo export.
        /// </summary>
        private class SerializableMapInfo
        {
            public string scene;
            public int mapId;
            public bool isUnlocked;
            public string mapName;
            public string mapDescription;
            public int difficultyRating;
            public bool isRewardOnCompleteFirstTime;
            public List<SerializableMapReward> rewards;
            public string rewardType;
            public string iconItemId;
            public string frameItemId;
            public int? characterId;
            public string inventoryItemId;
            public bool isNeedCurrency;
            public string currencyId;
            public int amount;
            public bool canIgnoreMap;
            public bool isEventMap;
            public string eventIdName;
        }

        /// <summary>
        /// Simplified reward structure for map rewards.
        /// </summary>
        private class SerializableMapReward
        {
            public string currencyId;
            public int amount;
            public int accountExp;
            public int characterExp;
            public int characterMasteryAmount;
        }
    }
}
#endif