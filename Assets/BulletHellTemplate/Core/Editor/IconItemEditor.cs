// IconItemEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="IconItem"/>.
    /// Displays icon item properties in grouped boxes and automatically renders
    /// any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(IconItem))]
    public class IconItemEditor : Editor
    {
        private SerializedProperty iconNameProp;
        private SerializedProperty iconNameTranslatedProp;
        private SerializedProperty iconIdProp;
        private SerializedProperty iconSpriteProp;
        private SerializedProperty isUnlockedProp;

        private HashSet<string> handledProps;
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            iconNameProp = serializedObject.FindProperty("iconName");
            iconNameTranslatedProp = serializedObject.FindProperty("iconNameTranslated");
            iconIdProp = serializedObject.FindProperty("iconId");
            iconSpriteProp = serializedObject.FindProperty("icon");
            isUnlockedProp = serializedObject.FindProperty("isUnlocked");

            handledProps = new HashSet<string>
            {
                "m_Script",
                iconNameProp.name,
                iconNameTranslatedProp.name,
                iconIdProp.name,
                iconSpriteProp.name,
                isUnlockedProp.name
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Icon Item", logoPath, headerColor);

            DrawIconSettingsBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawIconSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Icon Item Settings", labelColor);
            EditorGUILayout.PropertyField(iconNameProp, new GUIContent("Icon Name", "Display name of the icon."));
            EditorGUILayout.PropertyField(iconNameTranslatedProp, new GUIContent("Translated Names", "Translated names of the icon."));
            EditorGUILayout.PropertyField(iconIdProp, new GUIContent("Icon ID", "Unique identifier for the icon."));
            EditorGUILayout.PropertyField(iconSpriteProp, new GUIContent("Icon Sprite", "Icon sprite asset."));
            EditorGUILayout.PropertyField(isUnlockedProp, new GUIContent("Unlocked", "Indicates if the icon is unlocked."));
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
