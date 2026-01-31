#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="LoadingManager"/>.
    /// Displays grouped settings in separate boxes and automatically renders
    /// any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(LoadingManager))]
    public class LoadingManagerEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        private SerializedProperty loadingTextProp;
        private SerializedProperty uiLoadingProp;
        private SerializedProperty loadingBarProp;
        private SerializedProperty minTimeProp;
        private SerializedProperty audioClipProp;
        private SerializedProperty loadingTagProp;

        private HashSet<string> handledProps;

        private Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            loadingTextProp = serializedObject.FindProperty("loadingText");
            uiLoadingProp = serializedObject.FindProperty("UILoading");
            loadingBarProp = serializedObject.FindProperty("loadingProgressBar");
            minTimeProp = serializedObject.FindProperty("minimumLoadingTime");
            audioClipProp = serializedObject.FindProperty("loadingAudioClip");
            loadingTagProp = serializedObject.FindProperty("Loadingtag");

            handledProps = new HashSet<string>
            {
                "m_Script",
                loadingTextProp.name,
                uiLoadingProp.name,
                loadingBarProp.name,
                minTimeProp.name,
                audioClipProp.name,
                loadingTagProp.name
            };
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Loading Manager", logoPath, headerColor);

            DrawUIElementsBox();
            DrawLoadingSettingsBox();
            DrawAudioSettingsBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUIElementsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("UI Elements", labelColor);
            PropertyField(loadingTextProp, new GUIContent("Loading Text", "Text component displaying the animated 'Loading' message."));
            PropertyField(uiLoadingProp, new GUIContent("Loading Screen", "UI container holding the loading screen."));
            PropertyField(loadingBarProp, new GUIContent("Progress Bar", "Slider displaying loading progress."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawLoadingSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Loading Settings", labelColor);
            PropertyField(minTimeProp, new GUIContent("Minimum Loading Time", "Minimum duration (in seconds) the loading screen should be displayed."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawAudioSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Audio Settings", labelColor);
            PropertyField(audioClipProp, new GUIContent("Loading Audio Clip", "Audio clip played during loading screen."));
            PropertyField(loadingTagProp, new GUIContent("Audio Tag", "Audio mixer tag used for loading audio."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Automatically displays any new serialized fields not explicitly handled above.
        /// </summary>
        private void DrawOtherSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Other Settings", labelColor);

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true); // Skip the script reference
            while (prop.NextVisible(false))
            {
                if (handledProps.Contains(prop.name))
                    continue;
                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void PropertyField(SerializedProperty prop, GUIContent label)
        {
            EditorGUILayout.PropertyField(prop, label);
        }
    }
}
#endif
