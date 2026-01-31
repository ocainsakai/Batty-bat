using UnityEngine;
using UnityEditor;

namespace LanguageManager
{
    [CustomEditor(typeof(AudioWarper))]
    public class AudioWarperEditor : Editor
    {
        SerializedProperty audioClipsProp;
        SerializedProperty changeAudioSourceClipProp;
        SerializedProperty audioSourceProp;

        void OnEnable()
        {
            audioClipsProp = serializedObject.FindProperty("audioClips");
            changeAudioSourceClipProp = serializedObject.FindProperty("changeAudioSourceClip");
            audioSourceProp = serializedObject.FindProperty("audioSource");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the list of audio clips
            EditorGUILayout.PropertyField(audioClipsProp, true);

            EditorGUILayout.Space();

            // Draw the changeAudioSourceClip toggle
            EditorGUILayout.PropertyField(changeAudioSourceClipProp);

            // If changeAudioSourceClip is true, show the audioSource field
            if (changeAudioSourceClipProp.boolValue)
            {
                EditorGUILayout.PropertyField(audioSourceProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
