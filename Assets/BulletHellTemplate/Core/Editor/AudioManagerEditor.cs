#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="AudioManager"/>.
    /// Displays grouped audio settings and automatically renders
    /// any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(AudioManager))]
    public class AudioManagerEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        private SerializedProperty masterAudioSourceProp;
        private SerializedProperty ambientAudioSourceProp;
        private SerializedProperty loadingAudioSourceProp;

        private SerializedProperty masterVolumeProp;
        private SerializedProperty vfxVolumeProp;
        private SerializedProperty ambienceVolumeProp;
        private SerializedProperty customTagVolumesProp;

        private SerializedProperty maxConcurrentAudioProp;

        /* ---------- Handled Property Names ---------- */
        private HashSet<string> handledProps;

        /* ---------- Styling ---------- */
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            masterAudioSourceProp = serializedObject.FindProperty("masterAudioSource");
            ambientAudioSourceProp = serializedObject.FindProperty("ambientAudioSource");
            loadingAudioSourceProp = serializedObject.FindProperty("loadingAudioSource");

            masterVolumeProp = serializedObject.FindProperty("masterVolume");
            vfxVolumeProp = serializedObject.FindProperty("vfxVolume");
            ambienceVolumeProp = serializedObject.FindProperty("ambienceVolume");
            customTagVolumesProp = serializedObject.FindProperty("customTagVolumes");

            maxConcurrentAudioProp = serializedObject.FindProperty("maxConcurrentAudio");

            handledProps = new HashSet<string>
            {
                "m_Script",
                masterAudioSourceProp.name,
                ambientAudioSourceProp.name,
                loadingAudioSourceProp.name,
                masterVolumeProp.name,
                vfxVolumeProp.name,
                ambienceVolumeProp.name,
                customTagVolumesProp.name,
                maxConcurrentAudioProp.name
            };
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Audio Manager", logoPath, headerColor);

            DrawAudioSourceBox();
            DrawVolumeSettingsBox();
            DrawLimitationsBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Renders the Audio Source section.
        /// </summary>
        private void DrawAudioSourceBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Audio Source", labelColor);
            EditorGUILayout.PropertyField(masterAudioSourceProp, new GUIContent("Master Audio Source", "Main audio source for Master volume."));
            EditorGUILayout.PropertyField(ambientAudioSourceProp, new GUIContent("Ambient Audio Source", "Audio source for ambient sounds."));
            EditorGUILayout.PropertyField(loadingAudioSourceProp, new GUIContent("Loading Audio Source", "Audio source for loading menu sounds."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Renders the Volume Settings section.
        /// </summary>
        private void DrawVolumeSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Volume Settings", labelColor);
            EditorGUILayout.PropertyField(masterVolumeProp, new GUIContent("Master Volume", "Global volume for all sounds."));
            EditorGUILayout.PropertyField(vfxVolumeProp, new GUIContent("VFX Volume", "Volume for visual effects sounds."));
            EditorGUILayout.PropertyField(ambienceVolumeProp, new GUIContent("Ambient Volume", "Volume for ambient sounds."));
            EditorGUILayout.PropertyField(customTagVolumesProp, new GUIContent("Custom Tag Volumes", "List of custom audio tags with their volumes."), true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Renders the Audio Limitations section.
        /// </summary>
        private void DrawLimitationsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Audio Limitations", labelColor);
            EditorGUILayout.PropertyField(maxConcurrentAudioProp, new GUIContent("Max Concurrent Audio", "Maximum number of audios played at once (0 = unlimited)."));
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
            prop.NextVisible(true); // Skip script reference
            while (prop.NextVisible(false))
            {
                if (handledProps.Contains(prop.name))
                    continue;
                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
#endif
