#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="UIProfileMenu"/>.
    /// Groups related UI elements, prefabs, translations, events and audio settings into organized sections,
    /// and automatically renders any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(UIProfileMenu))]
    public class UIProfileMenuEditor : Editor
    {
        // ---------- Serialized Properties ----------
        private SerializedProperty playerNameProp;
        private SerializedProperty playerIconProp;
        private SerializedProperty playerFrameProp;

        private SerializedProperty iconsEntryPrefabProp;
        private SerializedProperty framesEntryPrefabProp;
        private SerializedProperty containerProp;
        private SerializedProperty containerPrefProp;
        private SerializedProperty changeNamePrefProp;
        private SerializedProperty changeNameInputProp;
        private SerializedProperty statusTextProp;

        private SerializedProperty masteryIconProp;
        private SerializedProperty masteryProgressBarProp;
        private SerializedProperty currentMasteryExpProp;
        private SerializedProperty masteryNameProp;
        private SerializedProperty tempCharacterContainerProp;

        private SerializedProperty notEnoughTicketsProp;
        private SerializedProperty notEnoughTicketsTranslatedProp;
        private SerializedProperty nameLengthErrorProp;
        private SerializedProperty nameLengthErrorTranslatedProp;
        private SerializedProperty nameAlreadyTakenProp;
        private SerializedProperty nameAlreadyTakenTranslatedProp;
        private SerializedProperty nameChangeSuccessProp;
        private SerializedProperty nameChangeSuccessTranslatedProp;
        private SerializedProperty nameChangeFailProp;
        private SerializedProperty nameChangeFailTranslatedProp;

        private SerializedProperty OnOpenMenuProp;
        private SerializedProperty OnCloseMenuProp;
        private SerializedProperty OnChangeNicknameProp;
        private SerializedProperty OnChangeIconProp;
        private SerializedProperty OnChangeFrameProp;
        private SerializedProperty OnRedeemCouponProp;

        private SerializedProperty masterVolumeSliderProp;
        private SerializedProperty vfxVolumeSliderProp;
        private SerializedProperty ambienceVolumeSliderProp;

        // Names of all explicitly handled properties (including m_Script)
        private HashSet<string> handledProps;

        // Colors for header and labels
        private readonly Color titleColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            // Assign all serialized properties
            playerNameProp = serializedObject.FindProperty("playerName");
            playerIconProp = serializedObject.FindProperty("playerIcon");
            playerFrameProp = serializedObject.FindProperty("playerFrame");

            iconsEntryPrefabProp = serializedObject.FindProperty("iconsEntryPrefab");
            framesEntryPrefabProp = serializedObject.FindProperty("framesEntryPrefab");
            containerProp = serializedObject.FindProperty("container");
            containerPrefProp = serializedObject.FindProperty("containerPref");
            changeNamePrefProp = serializedObject.FindProperty("changeNamePref");
            changeNameInputProp = serializedObject.FindProperty("changeNameInput");
            statusTextProp = serializedObject.FindProperty("statusText");

            masteryIconProp = serializedObject.FindProperty("masteryIcon");
            masteryProgressBarProp = serializedObject.FindProperty("masteryProgressBar");
            currentMasteryExpProp = serializedObject.FindProperty("currentMasteryExp");
            masteryNameProp = serializedObject.FindProperty("masteryName");
            tempCharacterContainerProp = serializedObject.FindProperty("tempCharacterContainer");

            notEnoughTicketsProp = serializedObject.FindProperty("notEnoughTickets");
            notEnoughTicketsTranslatedProp = serializedObject.FindProperty("notEnoughTicketsTranslated");
            nameLengthErrorProp = serializedObject.FindProperty("nameLengthError");
            nameLengthErrorTranslatedProp = serializedObject.FindProperty("nameLengthErrorTranslated");
            nameAlreadyTakenProp = serializedObject.FindProperty("nameAlreadyTaken");
            nameAlreadyTakenTranslatedProp = serializedObject.FindProperty("nameAlreadyTakenTranslated");
            nameChangeSuccessProp = serializedObject.FindProperty("nameChangeSuccess");
            nameChangeSuccessTranslatedProp = serializedObject.FindProperty("nameChangeSuccessTranslated");
            nameChangeFailProp = serializedObject.FindProperty("nameChangeFail");
            nameChangeFailTranslatedProp = serializedObject.FindProperty("nameChangeFailTranslated");

            OnOpenMenuProp = serializedObject.FindProperty("OnOpenMenu");
            OnCloseMenuProp = serializedObject.FindProperty("OnCloseMenu");
            OnChangeNicknameProp = serializedObject.FindProperty("OnChangeNickname");
            OnChangeIconProp = serializedObject.FindProperty("OnChangeIcon");
            OnChangeFrameProp = serializedObject.FindProperty("OnChangeFrame");
            OnRedeemCouponProp = serializedObject.FindProperty("OnRedeemCoupon");

            masterVolumeSliderProp = serializedObject.FindProperty("masterVolumeSlider");
            vfxVolumeSliderProp = serializedObject.FindProperty("vfxVolumeSlider");
            ambienceVolumeSliderProp = serializedObject.FindProperty("ambienceVolumeSlider");

            // Initialize handledProps set
            handledProps = new HashSet<string>
            {
                "m_Script",
                playerNameProp.name,
                playerIconProp.name,
                playerFrameProp.name,
                iconsEntryPrefabProp.name,
                framesEntryPrefabProp.name,
                containerProp.name,
                containerPrefProp.name,
                changeNamePrefProp.name,
                changeNameInputProp.name,
                statusTextProp.name,
                masteryIconProp.name,
                masteryProgressBarProp.name,
                currentMasteryExpProp.name,
                masteryNameProp.name,
                tempCharacterContainerProp.name,
                notEnoughTicketsProp.name,
                notEnoughTicketsTranslatedProp.name,
                nameLengthErrorProp.name,
                nameLengthErrorTranslatedProp.name,
                nameAlreadyTakenProp.name,
                nameAlreadyTakenTranslatedProp.name,
                nameChangeSuccessProp.name,
                nameChangeSuccessTranslatedProp.name,
                nameChangeFailProp.name,
                nameChangeFailTranslatedProp.name,
                OnOpenMenuProp.name,
                OnCloseMenuProp.name,
                OnChangeNicknameProp.name,
                OnChangeIconProp.name,
                OnChangeFrameProp.name,
                OnRedeemCouponProp.name,
                masterVolumeSliderProp.name,
                vfxVolumeSliderProp.name,
                ambienceVolumeSliderProp.name
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            const string pathTitle = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("UI Profile Menu", pathTitle, titleColor);

            // --- UI Elements Section ---
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("UI Elements", labelColor);
            EditorGUILayout.PropertyField(playerNameProp);
            EditorGUILayout.PropertyField(playerIconProp);
            EditorGUILayout.PropertyField(playerFrameProp);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // --- UI Prefabs Section ---
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("UI Prefabs", labelColor);
            EditorGUILayout.PropertyField(iconsEntryPrefabProp);
            EditorGUILayout.PropertyField(framesEntryPrefabProp);
            EditorGUILayout.PropertyField(containerProp);
            EditorGUILayout.PropertyField(containerPrefProp);
            EditorGUILayout.PropertyField(changeNamePrefProp);
            EditorGUILayout.PropertyField(changeNameInputProp);
            EditorGUILayout.PropertyField(statusTextProp);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // --- Favorite Character Section ---
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Favorite Character", labelColor);
            EditorGUILayout.PropertyField(masteryIconProp);
            EditorGUILayout.PropertyField(masteryProgressBarProp);
            EditorGUILayout.PropertyField(currentMasteryExpProp);
            EditorGUILayout.PropertyField(masteryNameProp);
            EditorGUILayout.PropertyField(tempCharacterContainerProp);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // --- UI Translations Section ---
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("UI Translations", labelColor);
            EditorGUILayout.PropertyField(notEnoughTicketsProp);
            EditorGUILayout.PropertyField(notEnoughTicketsTranslatedProp, true);
            EditorGUILayout.PropertyField(nameLengthErrorProp);
            EditorGUILayout.PropertyField(nameLengthErrorTranslatedProp, true);
            EditorGUILayout.PropertyField(nameAlreadyTakenProp);
            EditorGUILayout.PropertyField(nameAlreadyTakenTranslatedProp, true);
            EditorGUILayout.PropertyField(nameChangeSuccessProp);
            EditorGUILayout.PropertyField(nameChangeSuccessTranslatedProp, true);
            EditorGUILayout.PropertyField(nameChangeFailProp);
            EditorGUILayout.PropertyField(nameChangeFailTranslatedProp, true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // --- Audio Settings Section ---
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Audio Settings", labelColor);
            EditorGUILayout.PropertyField(masterVolumeSliderProp);
            EditorGUILayout.PropertyField(vfxVolumeSliderProp);
            EditorGUILayout.PropertyField(ambienceVolumeSliderProp);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // --- Events Section ---
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Events", labelColor);
            EditorGUILayout.PropertyField(OnOpenMenuProp);
            EditorGUILayout.PropertyField(OnCloseMenuProp);
            EditorGUILayout.PropertyField(OnChangeNicknameProp);
            EditorGUILayout.PropertyField(OnChangeIconProp);
            EditorGUILayout.PropertyField(OnChangeFrameProp);
            EditorGUILayout.PropertyField(OnRedeemCouponProp);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            // --- Other Settings Section ---
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
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
    }
}
#endif
