#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the export of QuestItem data into a simplified JSON format for server validation.
    /// Excludes translations, sprites, and Unity-specific objects.
    /// </summary>
    public static class QuestItemExporter
    {
        /// <summary>
        /// Exports all QuestItems from GameInstance into a simplified JSON format.
        /// </summary>
        public static void ExportQuests()
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.questData == null)
            {
                Debug.LogError("GameInstance or questData not found.");
                return;
            }

            List<SerializableQuestItem> exportList = new List<SerializableQuestItem>();

            foreach (var questItem in GameInstance.Singleton.questData)
            {
                if (questItem == null)
                    continue;

                var serializableQuest = new SerializableQuestItem
                {
                    questId = questItem.questId,
                    title = questItem.title,
                    description = questItem.description,
                    currencyReward = questItem.currencyReward,
                    currencyAmount = questItem.currencyAmount,
                    battlePassExp = questItem.battlePassExp,
                    accountExp = questItem.accountExp,
                    selectedCharacterExp = questItem.selectedCharacterExp,
                    questReward = questItem.questReward.ToString(),
                    rewardCharacterId = questItem.characterData != null ? questItem.characterData.characterId : (int?)null,
                    rewardIconId = questItem.iconItem != null ? questItem.iconItem.iconId : null,
                    rewardFrameId = questItem.frameItem != null ? questItem.frameItem.frameId : null,
                    rewardInventoryItemId = questItem.inventoryItem != null ? questItem.inventoryItem.itemId : null,
                    questType = questItem.questType.ToString(),
                    requirement = new SerializableQuestRequirement
                    {
                        requirementType = questItem.requirement.requirementType.ToString(),
                        targetAmount = questItem.requirement.targetAmount,
                        targetMapId = questItem.requirement.targetMap != null ? questItem.requirement.targetMap.mapId : (int?)null,
                        targetCharacterId = questItem.requirement.targetCharacter != null ? questItem.requirement.targetCharacter.characterId : (int?)null
                    }
                };

                exportList.Add(serializableQuest);
            }

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);

            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "QuestItems.json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Quest items exported successfully to {filePath}");
        }

        /// <summary>
        /// Simplified data structure for a quest item.
        /// </summary>
        private class SerializableQuestItem
        {
            public int questId;
            public string title;
            public string description;
            public string currencyReward;
            public int currencyAmount;
            public int battlePassExp;
            public int accountExp;
            public int selectedCharacterExp;
            public string questReward;
            public int? rewardCharacterId;
            public string rewardIconId;
            public string rewardFrameId;
            public string rewardInventoryItemId;
            public string questType;
            public SerializableQuestRequirement requirement;
        }

        /// <summary>
        /// Simplified structure for quest requirements.
        /// </summary>
        private class SerializableQuestRequirement
        {
            public string requirementType;
            public int targetAmount;
            public int? targetMapId;
            public int? targetCharacterId;
        }
    }
}
#endif