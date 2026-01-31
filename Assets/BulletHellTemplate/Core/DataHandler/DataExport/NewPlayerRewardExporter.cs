#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports <see cref="GameInstance.newPlayerRewardItems"/> to JSON for backend validation.
    /// </summary>
    public sealed class NewPlayerRewardExporter : MonoBehaviour
    {
        [Tooltip("Optional override for export directory. Leave empty to use Assets/ExportedData.")]
        public string overrideExportDirectory;

        [Tooltip("Output JSON filename.")]
        public string fileName = "newPlayerRewards.json";

        /// <summary>
        /// Export button entry point (call from inspector or menu).
        /// </summary>
        public void ExportNewPlayerRewards()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("NewPlayerRewardExporter: GameInstance not initialized.");
                return;
            }

            var list = GameInstance.Singleton.newPlayerRewardItems;
            if (list == null)
            {
                Debug.LogError("NewPlayerRewardExporter: newPlayerRewardItems is null.");
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