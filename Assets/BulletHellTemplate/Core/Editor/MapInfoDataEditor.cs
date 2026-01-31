#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="MapInfoData"/>.
    /// Displays map info settings in grouped boxes and automatically renders any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(MapInfoData))]
    public class MapInfoDataEditor : Editor
    {
        // ---------- Serialized Properties ----------
        private SerializedProperty sceneProp;
        private SerializedProperty mapIdProp;
        private SerializedProperty isUnlockedProp;
        private SerializedProperty mapNameProp;
        private SerializedProperty mapNameTranslatedProp;
        private SerializedProperty mapDescriptionProp;
        private SerializedProperty mapDescriptionTranslatedProp;
        private SerializedProperty mapPreviewImageProp;
        private SerializedProperty mapMinimapImageProp;
        private SerializedProperty difficultyRatingProp;

        private SerializedProperty isRewardOnCompleteFirstTimeProp;
        private SerializedProperty winMapRewardsProp;
        private SerializedProperty rewardTypeProp;
        private SerializedProperty iconItemProp;
        private SerializedProperty frameItemProp;
        private SerializedProperty characterDataProp;
        private SerializedProperty inventoryItemProp;

        private SerializedProperty isNeedCurrencyProp;
        private SerializedProperty currencyProp;
        private SerializedProperty amountProp;
        private SerializedProperty canIgnoreMapProp;

        private SerializedProperty isEventMapProp;
        private SerializedProperty eventIdNameProp;

        // Handled property names to exclude from "Other Settings"
        private HashSet<string> handledProps;

        // Styling
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            sceneProp = serializedObject.FindProperty("scene");
            mapIdProp = serializedObject.FindProperty("mapId");
            isUnlockedProp = serializedObject.FindProperty("isUnlocked");
            mapNameProp = serializedObject.FindProperty("mapName");
            mapNameTranslatedProp = serializedObject.FindProperty("mapNameTranslated");
            mapDescriptionProp = serializedObject.FindProperty("mapDescription");
            mapDescriptionTranslatedProp = serializedObject.FindProperty("mapDescriptionTranslated");
            mapPreviewImageProp = serializedObject.FindProperty("mapPreviewImage");
            mapMinimapImageProp = serializedObject.FindProperty("mapMinimapImage");
            difficultyRatingProp = serializedObject.FindProperty("difficultyRating");

            isRewardOnCompleteFirstTimeProp = serializedObject.FindProperty("isRewardOnCompleteFirstTime");
            winMapRewardsProp = serializedObject.FindProperty("WinMapRewards");
            rewardTypeProp = serializedObject.FindProperty("rewardType");
            iconItemProp = serializedObject.FindProperty("iconItem");
            frameItemProp = serializedObject.FindProperty("frameItem");
            characterDataProp = serializedObject.FindProperty("characterData");
            inventoryItemProp = serializedObject.FindProperty("inventoryItem");

            isNeedCurrencyProp = serializedObject.FindProperty("isNeedCurrency");
            currencyProp = serializedObject.FindProperty("currency");
            amountProp = serializedObject.FindProperty("amount");
            canIgnoreMapProp = serializedObject.FindProperty("canIgnoreMap");

            isEventMapProp = serializedObject.FindProperty("isEventMap");
            eventIdNameProp = serializedObject.FindProperty("eventIdName");

            handledProps = new HashSet<string>
            {
                "m_Script",
                sceneProp.name,
                mapIdProp.name,
                isUnlockedProp.name,
                mapNameProp.name,
                mapNameTranslatedProp.name,
                mapDescriptionProp.name,
                mapDescriptionTranslatedProp.name,
                mapPreviewImageProp.name,
                mapMinimapImageProp.name,
                difficultyRatingProp.name,
                isRewardOnCompleteFirstTimeProp.name,
                winMapRewardsProp.name,
                rewardTypeProp.name,
                iconItemProp.name,
                frameItemProp.name,
                characterDataProp.name,
                inventoryItemProp.name,
                isNeedCurrencyProp.name,
                currencyProp.name,
                amountProp.name,
                canIgnoreMapProp.name,
                isEventMapProp.name,
                eventIdNameProp.name
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Map Info Data", logoPath, headerColor);

            DrawBasicSettingsBox();
            DrawCompletionRewardsBox();
            DrawAccessRequirementsBox();
            DrawEventConfigurationBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBasicSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Basic Map Settings", labelColor);
            EditorGUILayout.PropertyField(sceneProp, new GUIContent("Scene", "The scene associated with the map."));
            EditorGUILayout.PropertyField(mapIdProp, new GUIContent("Map ID", "The unique identifier for the map."));
            EditorGUILayout.PropertyField(isUnlockedProp, new GUIContent("Unlocked", "Indicates whether the map is unlocked."));
            EditorGUILayout.PropertyField(mapNameProp, new GUIContent("Map Name", "The display name of the map."));
            EditorGUILayout.PropertyField(mapNameTranslatedProp, new GUIContent("Map Name Translated", "Translated names of the map."));
            EditorGUILayout.PropertyField(mapDescriptionProp, new GUIContent("Description", "A brief description of the map."));
            EditorGUILayout.PropertyField(mapDescriptionTranslatedProp, new GUIContent("Description Translated", "Translated descriptions of the map."));
            EditorGUILayout.PropertyField(mapPreviewImageProp, new GUIContent("Preview Image", "The preview image for the map."));
            EditorGUILayout.PropertyField(mapMinimapImageProp, new GUIContent("Minimap Image", "The minimap display image."));
            EditorGUILayout.PropertyField(difficultyRatingProp, new GUIContent("Difficulty Rating", "Difficulty rating from 1 to 5."));
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawCompletionRewardsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Completion Rewards", labelColor);
            EditorGUILayout.PropertyField(isRewardOnCompleteFirstTimeProp, new GUIContent("Reward On First Complete", "Enable reward for first completion."));
            if (isRewardOnCompleteFirstTimeProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(winMapRewardsProp, new GUIContent("Currency Rewards", "List of currency rewards."), true);
                EditorGUILayout.PropertyField(rewardTypeProp, new GUIContent("Special Reward Type", "Type of special reward."));
                switch ((MapRewardType)rewardTypeProp.enumValueIndex)
                {
                    case MapRewardType.Icon:
                        EditorGUILayout.PropertyField(iconItemProp, new GUIContent("Icon Item", "Icon reward item."));
                        break;
                    case MapRewardType.Frame:
                        EditorGUILayout.PropertyField(frameItemProp, new GUIContent("Frame Item", "Frame reward item."));
                        break;
                    case MapRewardType.Character:
                        EditorGUILayout.PropertyField(characterDataProp, new GUIContent("Character Data", "Character data reward."));
                        break;
                    case MapRewardType.InventoryItem:
                        EditorGUILayout.PropertyField(inventoryItemProp, new GUIContent("Inventory Item", "Inventory item reward."));
                        break;
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawAccessRequirementsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Access Requirements", labelColor);
            EditorGUILayout.PropertyField(isNeedCurrencyProp, new GUIContent("Require Currency", "Requires specific currency to access this map."));
            if (isNeedCurrencyProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(currencyProp, new GUIContent("Currency Type", "Currency required to enter the map."));
                EditorGUILayout.PropertyField(amountProp, new GUIContent("Amount Required", "Amount of currency required."));
                EditorGUILayout.PropertyField(canIgnoreMapProp, new GUIContent("Can Ignore Requirement", "Allow skipping requirement when previous maps completed."));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawEventConfigurationBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Event Configuration", labelColor);
            EditorGUILayout.PropertyField(isEventMapProp, new GUIContent("Event Map", "Enable time-limited event map."));
            if (isEventMapProp.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(eventIdNameProp, new GUIContent("Event ID", "Identifier for the event in Firebase."));
                EditorGUI.indentLevel--;
            }
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
