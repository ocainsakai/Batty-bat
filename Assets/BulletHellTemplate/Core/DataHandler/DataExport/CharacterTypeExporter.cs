#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the export of CharacterTypeData into a simplified JSON format for server use.
    /// Excludes translations and icon data to keep the payload lightweight.
    /// </summary>
    public static class CharacterTypeExporter
    {
        /// <summary>
        /// Exports all CharacterTypeData from GameInstance into a simplified JSON format.
        /// </summary>
        public static void ExportCharacterTypes()
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterTypes == null)
            {
                Debug.LogError("GameInstance or characterTypes not found.");
                return;
            }

            List<SerializableCharacterType> exportList = new List<SerializableCharacterType>();

            foreach (var typeData in GameInstance.Singleton.characterTypes)
            {
                if (typeData == null)
                    continue;

                var serializableType = new SerializableCharacterType
                {
                    typeName = typeData.typeName,
                    weaknesses = GetTypeNames(typeData.weaknesses),
                    strengths = GetTypeNames(typeData.strengths)
                };

                exportList.Add(serializableType);
            }

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);

            // Save to file inside Assets/ExportedData
            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "CharacterTypes.json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Character types exported successfully to {filePath}");
        }

        /// <summary>
        /// Converts an array of CharacterTypeData into a list of type names.
        /// </summary>
        private static List<string> GetTypeNames(CharacterTypeData[] types)
        {
            List<string> names = new List<string>();
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type != null)
                        names.Add(type.typeName);
                }
            }
            return names;
        }

        /// <summary>
        /// Represents a simplified version of CharacterTypeData for JSON export.
        /// </summary>
        private class SerializableCharacterType
        {
            public string typeName;
            public List<string> weaknesses;
            public List<string> strengths;
        }
    }
}
#endif