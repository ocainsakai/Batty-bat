#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="AuthManager"/>.
    /// Displays grouped UI references in separate boxes and automatically renders
    /// any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(AuthManager))]
    public class AuthManagerEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        private SerializedProperty loginEmailFieldProp;
        private SerializedProperty loginPasswordFieldProp;
        private SerializedProperty createEmailFieldProp;
        private SerializedProperty createPasswordFieldProp;
        private SerializedProperty createPasswordConfirmFieldProp;

        private SerializedProperty uiLoginProp;
        private SerializedProperty uiCreateAccountProp;
        private SerializedProperty uiLoadingProp;
        private SerializedProperty loadingTextProp;

        private SerializedProperty homeSceneNameProp;

        private HashSet<string> handledProps;

        private Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            loginEmailFieldProp = serializedObject.FindProperty("loginEmailInputField");
            loginPasswordFieldProp = serializedObject.FindProperty("loginPasswordInputField");
            createEmailFieldProp = serializedObject.FindProperty("createEmailInputField");
            createPasswordFieldProp = serializedObject.FindProperty("createPasswordInputField");
            createPasswordConfirmFieldProp = serializedObject.FindProperty("createPasswordConfirmInputField");

            uiLoginProp = serializedObject.FindProperty("UILogin");
            uiCreateAccountProp = serializedObject.FindProperty("UICreateAccount");
            uiLoadingProp = serializedObject.FindProperty("UILoading");
            loadingTextProp = serializedObject.FindProperty("loadingText");

            homeSceneNameProp = serializedObject.FindProperty("HomeSceneName");

            handledProps = new HashSet<string>
            {
                "m_Script",
                loginEmailFieldProp.name,
                loginPasswordFieldProp.name,
                createEmailFieldProp.name,
                createPasswordFieldProp.name,
                createPasswordConfirmFieldProp.name,
                uiLoginProp.name,
                uiCreateAccountProp.name,
                uiLoadingProp.name,
                loadingTextProp.name,
                homeSceneNameProp.name
            };
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Auth Manager", logoPath, headerColor);

            DrawInputFieldsBox();
            DrawScreensBox();
            DrawLoadingUIBox();
            DrawSettingsBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInputFieldsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Input Fields", labelColor);
            PropertyField(loginEmailFieldProp, new GUIContent("Login Email Field", "Reference to the email input field for login."));
            PropertyField(loginPasswordFieldProp, new GUIContent("Login Password Field", "Reference to the password input field for login."));
            PropertyField(createEmailFieldProp, new GUIContent("Create Email Field", "Reference to the email input field for account creation."));
            PropertyField(createPasswordFieldProp, new GUIContent("Create Password Field", "Reference to the password input field for account creation."));
            PropertyField(createPasswordConfirmFieldProp, new GUIContent("Confirm Password Field", "Reference to the password confirmation field."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawScreensBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Screens", labelColor);
            PropertyField(uiLoginProp, new GUIContent("Login Screen", "GameObject for the login UI."));
            PropertyField(uiCreateAccountProp, new GUIContent("Create Account Screen", "GameObject for the account creation UI."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawLoadingUIBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Loading UI", labelColor);
            PropertyField(uiLoadingProp, new GUIContent("Loading Container", "GameObject for loading UI."));
            PropertyField(loadingTextProp, new GUIContent("Loading Text", "Text element that displays loading messages."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Settings", labelColor);
            PropertyField(homeSceneNameProp, new GUIContent("Home Scene Name", "Name of the scene to load after successful authentication."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

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

        private void PropertyField(SerializedProperty prop, GUIContent label)
        {
            EditorGUILayout.PropertyField(prop, label);
        }
    }
}
#endif
