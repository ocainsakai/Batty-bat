#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports <see cref="GameInstance.dailyRewardItems"/> to JSON for backend validation.
    /// </summary>
    public sealed class DailyRewardExporter : MonoBehaviour
    {
        [Tooltip("Optional override for export directory. Leave empty to use Assets/ExportedData.")]
        public string overrideExportDirectory;

        [Tooltip("Output JSON filename.")]
        public string fileName = "dailyRewards.json";

        /// <summary>
        /// Export button entry point (call from inspector or menu).
        /// </summary>
        public void ExportDailyRewards()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("DailyRewardExporter: GameInstance not initialized.");
                return;
            }

            var list = GameInstance.Singleton.dailyRewardItems;
            if (list == null)
            {
                Debug.LogError("DailyRewardExporter: dailyRewardItems is null.");
                return;
            }

            string dir = string.IsNullOrEmpty(overrideExportDirectory)
                ? Path.Combine(Application.dataPath, "ExportedData")
                : overrideExportDirectory;

            RewardItemExportUtility.ExportRewardItems(list, dir, fileName);
        }
    }
}
#endif