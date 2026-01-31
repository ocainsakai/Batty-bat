#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="SceneAmbientAudio"/>.
    /// Displays ambient audio settings and automatically renders
    /// any newly added serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(SceneAmbientAudio))]
    public class SceneAmbientAudioEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        private SerializedProperty ambientClipProp;
        private SerializedProperty ambientTagProp;

        /* ---------- Handled Property Names ---------- */
        private HashSet<string> handledProps;

        /* ---------- Styling ---------- */
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);
        private const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";

        private void OnEnable()
        {
            ambientClipProp = serializedObject.FindProperty("ambientClip");
            ambientTagProp = serializedObject.FindProperty("ambientTag");

            handledProps = new HashSet<string>
            {
                "m_Script",
                ambientClipProp.name,
                ambientTagProp.name
            };
        }

        /// <inheritdoc />
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Title Header
            EditorUIUtils.DrawTitleHeader("Scene Ambient Audio", logoPath, headerColor);

            // Ambient Audio Settings
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Ambient Audio Settings", labelColor);
            EditorGUILayout.PropertyField(ambientClipProp,
                new GUIContent("Ambient Clip", "The ambient audio clip to play when the scene starts."));
            EditorGUILayout.PropertyField(ambientTagProp,
                new GUIContent("Ambient Tag", "AudioManager tag for ambient sounds (default 'ambient')."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // Other Settings (dynamic)
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Other Settings", labelColor);
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true); // skip script reference
            while (prop.NextVisible(false))
            {
                if (handledProps.Contains(prop.name))
                    continue;
                EditorGUILayout.PropertyField(prop, true);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
