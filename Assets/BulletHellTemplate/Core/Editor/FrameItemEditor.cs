#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="FrameItem"/>.
    /// Displays frame item properties in grouped boxes and automatically renders
    /// any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(FrameItem))]
    public class FrameItemEditor : Editor
    {
        private SerializedProperty frameNameProp;
        private SerializedProperty frameNameTranslatedProp;
        private SerializedProperty frameIdProp;
        private SerializedProperty iconProp;
        private SerializedProperty isUnlockedProp;

        private HashSet<string> handledProps;
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            frameNameProp = serializedObject.FindProperty("frameName");
            frameNameTranslatedProp = serializedObject.FindProperty("frameNameTranslated");
            frameIdProp = serializedObject.FindProperty("frameId");
            iconProp = serializedObject.FindProperty("icon");
            isUnlockedProp = serializedObject.FindProperty("isUnlocked");

            handledProps = new HashSet<string>
            {
                "m_Script",
                frameNameProp.name,
                frameNameTranslatedProp.name,
                frameIdProp.name,
                iconProp.name,
                isUnlockedProp.name
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Frame Item", logoPath, headerColor);

            DrawFrameSettingsBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFrameSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Frame Item Settings", labelColor);
            EditorGUILayout.PropertyField(frameNameProp, new GUIContent("Frame Name", "Display name of the frame."));
            EditorGUILayout.PropertyField(frameNameTranslatedProp, new GUIContent("Translated Names", "Translated names of the frame."));
            EditorGUILayout.PropertyField(frameIdProp, new GUIContent("Frame ID", "Unique identifier for the frame."));
            EditorGUILayout.PropertyField(iconProp, new GUIContent("Icon", "Icon sprite for the frame."));
            EditorGUILayout.PropertyField(isUnlockedProp, new GUIContent("Unlocked", "Indicates if the frame is unlocked."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawOtherSettingsBox()
        {
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
        }
    }
}
#endif
