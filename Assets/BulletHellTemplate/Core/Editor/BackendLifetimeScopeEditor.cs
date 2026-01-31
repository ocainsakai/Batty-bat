#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="BackendLifetimeScope"/>.
    /// Displays the backend settings and shows inherited LifetimeScope fields under a separate label.
    /// </summary>
    [CustomEditor(typeof(BackendLifetimeScope))]
    public class BackendLifetimeScopeEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        private SerializedProperty settingsProp;

        /* ---------- Handled Property Names ---------- */
        private HashSet<string> handledProps;

        /* ---------- Styling ---------- */
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            settingsProp = serializedObject.FindProperty("settings");
            handledProps = new HashSet<string>
            {
                "m_Script",
                settingsProp.name
            };
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Backend Lifetime Scope", logoPath, headerColor);

            // Backend Settings
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Backend Settings", labelColor);
            EditorGUILayout.PropertyField(settingsProp, new GUIContent(
                "Settings",
                "Configuration settings that bind to IBackendService implementation."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Inherited LifetimeScope fields
            EditorUIUtils.DrawSubHeader("Inherited Properties", labelColor);           
            EditorGUILayout.BeginVertical("box");
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true); // Skip script reference
            while (prop.NextVisible(false))
            {
                if (handledProps.Contains(prop.name))
                    continue;
                EditorGUILayout.PropertyField(prop, true);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
