#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports all IconItem and FrameItem data from GameInstance into JSON files.
    /// </summary>
    public class IconAndFrameExporter : MonoBehaviour
    {
        /// <summary>
        /// Exports icon and frame data to JSON files.
        /// </summary>
        public void ExportIconAndFrameData()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameInstance is not initialized.");
                return;
            }

            string exportPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            ExportIcons(exportPath);
            ExportFrames(exportPath);

            Debug.Log("Icon and Frame data exported successfully.");
        }

        /// <summary>
        /// Exports icon data.
        /// </summary>
        public static void ExportIcons(string exportPath)
        {
            List<IconExportData> iconList = new List<IconExportData>();

            foreach (var icon in GameInstance.Singleton.iconItems)
            {
                if (icon == null)
                    continue;

                iconList.Add(new IconExportData
                {
                    iconId = icon.iconId,
                    iconName = icon.iconName,
                    isUnlocked = icon.isUnlocked
                });
            }

            string iconsJson = JsonConvert.SerializeObject(iconList, Formatting.Indented);
            File.WriteAllText(Path.Combine(exportPath, "icons.json"), iconsJson);
        }

        /// <summary>
        /// Exports frame data.
        /// </summary>
        public static void ExportFrames(string exportPath)
        {
            List<FrameExportData> frameList = new List<FrameExportData>();

            foreach (var frame in GameInstance.Singleton.frameItems)
            {
                if (frame == null)
                    continue;

                frameList.Add(new FrameExportData
                {
                    frameId = frame.frameId,
                    frameName = frame.frameName,
                    isUnlocked = frame.isUnlocked
                });
            }

            string framesJson = JsonConvert.SerializeObject(frameList, Formatting.Indented);
            File.WriteAllText(Path.Combine(exportPath, "frames.json"), framesJson);
        }

        /// <summary>
        /// Structure for exported icon data.
        /// </summary>
        [System.Serializable]
        private class IconExportData
        {
            public string iconId;
            public string iconName;
            public bool isUnlocked;
        }

        /// <summary>
        /// Structure for exported frame data.
        /// </summary>
        [System.Serializable]
        private class FrameExportData
        {
            public string frameId;
            public string frameName;
            public bool isUnlocked;
        }
    }
}
#endif