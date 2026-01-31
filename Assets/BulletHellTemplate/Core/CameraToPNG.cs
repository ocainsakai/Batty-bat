using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BulletHellTemplate
{
    /// <summary>
    /// Captures the view of a camera into a transparent PNG, using a Render Texture.
    /// </summary>
    public class CameraToPNG : MonoBehaviour
    {
        [Tooltip("The camera used for capturing the screenshot.")]
        public Camera targetCamera;

        [Tooltip("The Render Texture for capturing the camera view.")]
        public RenderTexture renderTexture;

        [Tooltip("The file path where the PNG will be saved.")]
        public string savePath = "Assets/MinimapScreenshot.png";

        /// <summary>
        /// Captures the current view of the camera into a PNG file with transparency.
        /// </summary>
        public void CaptureToPNG()
        {
            if (targetCamera == null || renderTexture == null)
            {
                Debug.LogError("Camera or RenderTexture is not assigned.");
                return;
            }

            // Backup camera settings
            RenderTexture originalTargetTexture = targetCamera.targetTexture;

            // Assign the Render Texture to the camera
            targetCamera.targetTexture = renderTexture;

            // Reduce quality settings momentarily if necessary
            int originalQuality = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel(0, false); // Adjust to the lowest quality

            // Render the camera's view to the Render Texture
            targetCamera.Render();

            // Create a Texture2D to read the Render Texture
            Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenshot.Apply();

            // Save the image as PNG
            byte[] bytes = screenshot.EncodeToPNG();
            System.IO.File.WriteAllBytes(savePath, bytes);

            // Restore settings
            targetCamera.targetTexture = originalTargetTexture;
            RenderTexture.active = null;
            QualitySettings.SetQualityLevel(originalQuality, false);

            Debug.Log($"Screenshot saved to {savePath}");
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Adds a button in the Unity Editor to capture the camera view into a PNG.
    /// </summary>
    [CustomEditor(typeof(CameraToPNG))]
    public class CameraToPNGEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            CameraToPNG cameraToPNG = (CameraToPNG)target;

            if (GUILayout.Button("Capture To PNG"))
            {
                cameraToPNG.CaptureToPNG();
            }
        }
    }
#endif
}
