#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using BulletHellTemplate; // for EditorUIUtils

namespace LanguageManager
{
    /// <summary>
    /// Custom Editor for the LanguageManager component.
    /// Provides a user interface in the Unity Inspector for managing languages and text entries,
    /// importing/exporting data, and automatically displays any newly added serialized fields.
    /// </summary>
    [CustomEditor(typeof(LanguageManager))]
    public class LanguageManagerEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        private SerializedProperty languageModeProp;
        private SerializedProperty enableTextFormattingProp;
        private SerializedProperty disableTMPWarningsProp;
        private SerializedProperty currentLanguageIDProp;
        private SerializedProperty languagesProp;
        private SerializedProperty textEntriesProp;

        // Dialog flags
        private bool openExportJSONDialog;
        private bool openImportJSONDialog;
        private bool openExportCSVDialog;
        private bool openImportCSVDialog;
        private bool openExportXMLDialog;
        private bool openImportXMLDialog;
        private bool openExportBaseModelJSONDialog;
        private bool openExportBaseModelCSVDialog;
        private bool openExportBaseModelXMLDialog;

        /* ---------- Handled Properties ---------- */
        private HashSet<string> handledProps;

        /* ---------- Styling ---------- */
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);
        private const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";

        void OnEnable()
        {
            languageModeProp = serializedObject.FindProperty("languageMode");
            disableTMPWarningsProp = serializedObject.FindProperty("disableTMPWarnings");
            enableTextFormattingProp = serializedObject.FindProperty("enableTextFormatting");
            currentLanguageIDProp = serializedObject.FindProperty("currentLanguageID");
            languagesProp = serializedObject.FindProperty("Languages");
            textEntriesProp = serializedObject.FindProperty("TextEntries");

            handledProps = new HashSet<string>
            {
                "m_Script",
                languageModeProp.name,
                disableTMPWarningsProp.name,
                enableTextFormattingProp.name,
                currentLanguageIDProp.name,
                languagesProp.name,
                textEntriesProp.name
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Title
            EditorUIUtils.DrawTitleHeader("Language Manager", logoPath, headerColor);

            // Debug Settings
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Debug Settings", labelColor);
            EditorGUILayout.PropertyField(disableTMPWarningsProp, new GUIContent("Disable TMP Warnings", "Disable TextMeshPro warnings on Awake."));
            EditorGUILayout.PropertyField(enableTextFormattingProp, new GUIContent("Enable Text Formatting", "Enable processing of formatting tags in GetTextEntryByID."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Mode Settings
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Mode Settings", labelColor);
            EditorGUILayout.PropertyField(languageModeProp, new GUIContent("Language Mode", "Mode for loading and displaying translations."));
            EditorGUILayout.PropertyField(currentLanguageIDProp, new GUIContent("Current Language ID", "ID of the currently selected language."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Internal Database Data
            var targetMgr = (LanguageManager)target;
            if (targetMgr.languageMode == LanguageMode.InternalDatabase)
            {
                EditorGUILayout.BeginVertical("box");
                EditorUIUtils.DrawSubHeader("Language Data", labelColor);
                EditorGUILayout.PropertyField(languagesProp, new GUIContent("Languages", "List of supported languages."), true);
                EditorGUILayout.PropertyField(textEntriesProp, new GUIContent("Text Entries", "List of text entries with translations."), true);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // Import / Export
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Import / Export", labelColor);
            EditorGUILayout.HelpBox("It's recommended to export a backup before importing.", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export JSON", GUILayout.Height(22))) openExportJSONDialog = true;
            if (GUILayout.Button("Import JSON", GUILayout.Height(22))) openImportJSONDialog = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export CSV", GUILayout.Height(22))) openExportCSVDialog = true;
            if (GUILayout.Button("Import CSV", GUILayout.Height(22))) openImportCSVDialog = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export XML", GUILayout.Height(22))) openExportXMLDialog = true;
            if (GUILayout.Button("Import XML", GUILayout.Height(22))) openImportXMLDialog = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Export Base Model
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Export Base Model", labelColor);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Base Model JSON", GUILayout.Height(22))) openExportBaseModelJSONDialog = true;
            if (GUILayout.Button("Base Model CSV", GUILayout.Height(22))) openExportBaseModelCSVDialog = true;
            if (GUILayout.Button("Base Model XML", GUILayout.Height(22))) openExportBaseModelXMLDialog = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Other Settings
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Other Settings", labelColor);
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(false))
            {
                if (handledProps.Contains(prop.name)) continue;
                EditorGUILayout.PropertyField(prop, true);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            // Dialogs
            if (openExportJSONDialog) { openExportJSONDialog = false; var fp = EditorUtility.SaveFilePanel("Export JSON", "", "languages.json", "json"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ExportLanguagesToJSON(fp); EditorUtility.DisplayDialog("Export JSON", "Exported to JSON successfully.", "OK"); } }
            if (openImportJSONDialog) { openImportJSONDialog = false; var fp = EditorUtility.OpenFilePanel("Import JSON", "", "json"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ImportLanguagesFromJSON(fp); EditorUtility.DisplayDialog("Import JSON", "Imported from JSON successfully.", "OK"); } }
            if (openExportCSVDialog) { openExportCSVDialog = false; var fp = EditorUtility.SaveFilePanel("Export CSV", "", "languages.csv", "csv"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ExportLanguagesToCSV(fp); EditorUtility.DisplayDialog("Export CSV", "Exported to CSV successfully.", "OK"); } }
            if (openImportCSVDialog) { openImportCSVDialog = false; var fp = EditorUtility.OpenFilePanel("Import CSV", "", "csv"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ImportLanguagesFromCSV(fp); EditorUtility.DisplayDialog("Import CSV", "Imported from CSV successfully.", "OK"); } }
            if (openExportXMLDialog) { openExportXMLDialog = false; var fp = EditorUtility.SaveFilePanel("Export XML", "", "languages.xml", "xml"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ExportLanguagesToXML(fp); EditorUtility.DisplayDialog("Export XML", "Exported to XML successfully.", "OK"); } }
            if (openImportXMLDialog) { openImportXMLDialog = false; var fp = EditorUtility.OpenFilePanel("Import XML", "", "xml"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ImportLanguagesFromXML(fp); EditorUtility.DisplayDialog("Import XML", "Imported from XML successfully.", "OK"); } }
            if (openExportBaseModelJSONDialog) { openExportBaseModelJSONDialog = false; var fp = EditorUtility.SaveFilePanel("Export Base Model JSON", "", "base_model.json", "json"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ExportBaseModel("JSON", fp); EditorUtility.DisplayDialog("Base Model JSON", "Exported base model to JSON successfully.", "OK"); } }
            if (openExportBaseModelCSVDialog) { openExportBaseModelCSVDialog = false; var fp = EditorUtility.SaveFilePanel("Export Base Model CSV", "", "base_model.csv", "csv"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ExportBaseModel("CSV", fp); EditorUtility.DisplayDialog("Base Model CSV", "Exported base model to CSV successfully.", "OK"); } }
            if (openExportBaseModelXMLDialog) { openExportBaseModelXMLDialog = false; var fp = EditorUtility.SaveFilePanel("Export Base Model XML", "", "base_model.xml", "xml"); if (!string.IsNullOrEmpty(fp)) { targetMgr.ExportBaseModel("XML", fp); EditorUtility.DisplayDialog("Base Model XML", "Exported base model to XML successfully.", "OK"); } }
        }
    }
}
#endif
