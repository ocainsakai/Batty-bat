#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Provides reusable methods to draw stylized headers in custom inspectors.
    /// </summary>
    public static class EditorUIUtils
    {
        /// <summary>
        /// Draws a prominent header bar with an icon and customizable background color.
        /// </summary>
        /// <param name="title">The text to display (will be uppercased).</param>
        /// <param name="iconPath">
        /// Relative path to the icon asset under Assets folder,
        /// e.g. "Assets/BulletHellTemplate/Core/Editor/EditorLogo/d_bizachi.png".
        /// </param>
        /// <param name="backgroundColor">Background color for the header bar.</param>
        public static void DrawTitleHeader(string title, string iconPath, Color backgroundColor)
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            var style = new GUIStyle(EditorStyles.largeLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                imagePosition = ImagePosition.ImageLeft,
                fontSize = 14,
                richText = false
            };

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label(new GUIContent("  " + title.ToUpper(), icon), style, GUILayout.Height(26));
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = prevColor;
            EditorGUILayout.Space(4);
        }

        /// <summary>
        /// Draws a simpler subheader with customizable background color.
        /// </summary>
        /// <param name="subtitle">Text to display in the subheader.</param>
        /// <param name="backgroundColor">Background color for the subheader bar.</param>
        public static void DrawSubHeader(string subtitle, Color backgroundColor)
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 11,
                richText = false
            };

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            EditorGUILayout.BeginHorizontal("box");
            GUILayout.Label("  " + subtitle, style, GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = prevColor;
            EditorGUILayout.Space(2);
        }
    }
}
#endif
