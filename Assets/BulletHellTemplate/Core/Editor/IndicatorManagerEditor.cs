#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom editor for the IndicatorManager to control gizmo visibility, rotation, size settings, and cast indicator references.
    /// </summary>
    [CustomEditor(typeof(IndicatorManager))]
    public class IndicatorManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw properties except for gizmo and cast indicator settings.
            DrawPropertiesExcluding(serializedObject, new string[]
            {
                "showCircleGizmo", "showArrowGizmo", "showConeGizmo",
                "arrowRotation", "arrowSize", "coneRotation", "coneSize",
                "castCircleIndicator", "castArrowIndicator", "castConeIndicator", "castDamageIndicator"
            });

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Gizmo Controls", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showCircleGizmo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showArrowGizmo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showConeGizmo"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("arrowRotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("arrowSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coneRotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("coneSize"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cast Indicator References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castCircleIndicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castArrowIndicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castConeIndicator"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("castDamageIndicator"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("aoeCurveLine"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
