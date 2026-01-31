#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="ExportAllButton"/>.
    /// Provides a button to export all game data and displays a warning when not in play mode.
    /// </summary>
    [CustomEditor(typeof(ExportAllButton))]
    public class ExportAllButtonEditor : Editor
    {
        /* ---------- Styling ---------- */
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw title header with logo
            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Export All Data", logoPath, headerColor);

            // Warning section
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Usage Warning", labelColor);
            EditorGUILayout.HelpBox(
                "Use this button only when the game is running and in the Login scene.",
                MessageType.Warning);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Export button
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button(new GUIContent(
                "Export All Game Data",
                "Exports all game data to external storage.")))
            {
                GameDataExportAll.ExportAll();
            }
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
