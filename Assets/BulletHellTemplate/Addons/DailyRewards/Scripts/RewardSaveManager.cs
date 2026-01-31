using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles saving and loading reward data from Firebase and PlayerPrefs.
    /// Prevents multiple calls to Firebase during initialization.
    /// </summary>
    public static class RewardSaveManager
    {
        /// <summary>
        /// Loads rewards from Firestore for a specified collection and invokes a callback with the data.
        /// </summary>
        public static void LoadRewards(string collectionName, System.Action<Dictionary<string, object>> onLoaded)
        {
           
        }

        /// <summary>
        /// Saves rewards to Firestore for a specified collection, and also stores them locally in PlayerPrefs.
        /// </summary>
        public static void SaveRewards(string collectionName, Dictionary<string, object> data)
        {           
            SaveRewardsLocally(collectionName, data);
        }

        /// <summary>
        /// Saves rewards data locally in PlayerPrefs as JSON.
        /// </summary>
        private static void SaveRewardsLocally(string collectionName, Dictionary<string, object> data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(collectionName, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads rewards data from PlayerPrefs if available.
        /// </summary>
        public static Dictionary<string, object> LoadRewardsLocally(string collectionName)
        {
            string json = PlayerPrefs.GetString(collectionName, null);
            return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<Dictionary<string, object>>(json);
        }
    }
}
