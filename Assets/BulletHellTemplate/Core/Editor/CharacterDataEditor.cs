#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="CharacterData"/>.
    /// Keeps the original logic (auto-fill EXP/Upgrade Cost, add/remove upgrades, etc.)
    /// while adding styling boxes, headers/subheaders and a dynamic \"Other Settings\" area
    /// that automatically renders any new serialized fields the devs add to CharacterData.
    /// </summary>
    [CustomEditor(typeof(CharacterData))]
    public class CharacterDataEditor : Editor
    {
        // ─────────────────────────────────────────────────────────────
        // Serialized Properties
        // ─────────────────────────────────────────────────────────────

        private SerializedProperty characterNameProp;
        private SerializedProperty characterNameTranslatedProp;
        private SerializedProperty characterDescriptionProp;
        private SerializedProperty characterDescriptionTranslatedProp;
        private SerializedProperty characterIdProp;
        private SerializedProperty characterTypeProp;
        private SerializedProperty iconProp;
        private SerializedProperty tierIconProp;
        private SerializedProperty characterClassTypeProp;
        private SerializedProperty characterClassTypeTranslatedProp;
        private SerializedProperty characterClassIconProp;
        private SerializedProperty characterRarityProp;
        private SerializedProperty characterModelProp;
        private SerializedProperty characterSkinsProp;
        private SerializedProperty autoAttackProp;
        private SerializedProperty skillsProp;
        private SerializedProperty itemSlotsProp;
        private SerializedProperty runeSlotsProp;
        private SerializedProperty isUnlockProp;
        private SerializedProperty baseStatsProp;
        private SerializedProperty maxLevelProp;
        private SerializedProperty percentageIncreaseByLevelProp;
        private SerializedProperty expCalculationMethodProp;
        private SerializedProperty initialExpProp;
        private SerializedProperty finalExpProp;
        private SerializedProperty expPerLevelProp;
        private SerializedProperty currencyIdProp;
        private SerializedProperty upgradeCostPerLevelProp;
        private SerializedProperty initialUpgradeCostProp;
        private SerializedProperty finalUpgradeCostProp;
        private SerializedProperty upgradeCostCalculationMethodProp;
        private SerializedProperty statUpgradesProp;

        // Styling + handled props
        private HashSet<string> _handledProps;
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void RegisterHandled(params SerializedProperty[] props)
        {
            foreach (var p in props)
            {
                if (p != null) _handledProps.Add(p.name);
            }
        }

        /// <summary>
        /// Initialize serialized properties.
        /// </summary>
        private void OnEnable()
        {
            _handledProps = new HashSet<string> { "m_Script" };

            characterNameProp = serializedObject.FindProperty("characterName");
            characterNameTranslatedProp = serializedObject.FindProperty("characterNameTranslated");
            characterDescriptionProp = serializedObject.FindProperty("characterDescription");
            characterDescriptionTranslatedProp = serializedObject.FindProperty("characterDescriptionTranslated");
            characterIdProp = serializedObject.FindProperty("characterId");
            characterTypeProp = serializedObject.FindProperty("characterType");
            iconProp = serializedObject.FindProperty("icon");
            tierIconProp = serializedObject.FindProperty("tierIcon");
            characterClassTypeProp = serializedObject.FindProperty("characterClassType");
            characterClassTypeTranslatedProp = serializedObject.FindProperty("characterClassTranslated");
            characterClassIconProp = serializedObject.FindProperty("characterClassIcon");
            characterRarityProp = serializedObject.FindProperty("characterRarity");
            characterModelProp = serializedObject.FindProperty("characterModel");
            characterSkinsProp = serializedObject.FindProperty("characterSkins");
            autoAttackProp = serializedObject.FindProperty("autoAttack");
            skillsProp = serializedObject.FindProperty("skills");
            itemSlotsProp = serializedObject.FindProperty("itemSlots");
            runeSlotsProp = serializedObject.FindProperty("runeSlots");
            isUnlockProp = serializedObject.FindProperty("isUnlock");
            baseStatsProp = serializedObject.FindProperty("baseStats");
            maxLevelProp = serializedObject.FindProperty("maxLevel");
            percentageIncreaseByLevelProp = serializedObject.FindProperty("statsPercentageIncreaseByLevel");

            expCalculationMethodProp = serializedObject.FindProperty("expCalculationMethod");
            initialExpProp = serializedObject.FindProperty("initialExp");
            finalExpProp = serializedObject.FindProperty("finalExp");
            expPerLevelProp = serializedObject.FindProperty("expPerLevel");

            currencyIdProp = serializedObject.FindProperty("currencyId");
            upgradeCostPerLevelProp = serializedObject.FindProperty("upgradeCostPerLevel");
            initialUpgradeCostProp = serializedObject.FindProperty("initialUpgradeCost");
            finalUpgradeCostProp = serializedObject.FindProperty("finalUpgradeCost");
            upgradeCostCalculationMethodProp = serializedObject.FindProperty("upgradeCostCalculationMethod");

            statUpgradesProp = serializedObject.FindProperty("statUpgrades");

            // Register all handled properties to be excluded from Other Settings
            RegisterHandled(
                characterNameProp, characterNameTranslatedProp, characterDescriptionProp,
                characterDescriptionTranslatedProp, characterIdProp, characterTypeProp, iconProp, tierIconProp,
                characterClassTypeProp, characterClassTypeTranslatedProp, characterClassIconProp, characterRarityProp,
                characterModelProp, characterSkinsProp, autoAttackProp, skillsProp, itemSlotsProp, runeSlotsProp,
                isUnlockProp, baseStatsProp, maxLevelProp, percentageIncreaseByLevelProp, expCalculationMethodProp,
                initialExpProp, finalExpProp, expPerLevelProp, currencyIdProp, upgradeCostPerLevelProp,
                initialUpgradeCostProp, finalUpgradeCostProp, upgradeCostCalculationMethodProp, statUpgradesProp
            );
        }

        /// <summary>
        /// Draws the custom inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Character Data", logoPath, headerColor);

            DrawGeneralInformationBox();
            DrawModelsBox();
            DrawSkillsAndItemsBox();
            DrawUnlockBox();
            DrawExperienceSettingsBox();
            DrawUpgradeCostSettingsBox();
            DrawBaseStatsBox();
            DrawUpgradesBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        // ─────────────────────────────────────────────────────────────
        // Sections
        // ─────────────────────────────────────────────────────────────

        private void DrawGeneralInformationBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("General Information", labelColor);

            EditorGUILayout.PropertyField(characterIdProp);
            EditorGUILayout.PropertyField(characterNameProp);
            EditorGUILayout.PropertyField(characterNameTranslatedProp, new GUIContent("Name Translations"), true);
            EditorGUILayout.PropertyField(characterDescriptionProp);
            EditorGUILayout.PropertyField(characterDescriptionTranslatedProp, new GUIContent("Description Translations"), true);
            EditorGUILayout.PropertyField(characterTypeProp);
            EditorGUILayout.PropertyField(iconProp);
            EditorGUILayout.PropertyField(tierIconProp);
            EditorGUILayout.PropertyField(characterClassTypeProp);
            EditorGUILayout.PropertyField(characterClassTypeTranslatedProp);
            EditorGUILayout.PropertyField(characterClassIconProp);
            EditorGUILayout.PropertyField(characterRarityProp);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawModelsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Models", labelColor);

            EditorGUILayout.PropertyField(characterModelProp);
            EditorGUILayout.PropertyField(characterSkinsProp, new GUIContent("Character Skins"), true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawSkillsAndItemsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Skills & Items", labelColor);

            EditorGUILayout.PropertyField(autoAttackProp);
            EditorGUILayout.PropertyField(skillsProp, true);
            EditorGUILayout.PropertyField(itemSlotsProp, true);
            EditorGUILayout.PropertyField(runeSlotsProp, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawUnlockBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Unlocked Status", labelColor);

            EditorGUILayout.PropertyField(isUnlockProp);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawExperienceSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Experience Settings", labelColor);

            EditorGUILayout.PropertyField(maxLevelProp);

            EditorUIUtils.DrawSubHeader("Autofill EXP Per Level", labelColor);
            EditorGUILayout.PropertyField(expCalculationMethodProp);
            EditorGUILayout.PropertyField(initialExpProp);
            EditorGUILayout.PropertyField(finalExpProp);

            if (GUILayout.Button("Auto Fill EXP"))
            {
                AutoFillExp(expPerLevelProp,
                    maxLevelProp.intValue,
                    initialExpProp.intValue,
                    finalExpProp.intValue,
                    (ExpCalculationMethod)expCalculationMethodProp.enumValueIndex);
            }

            EditorGUILayout.PropertyField(expPerLevelProp, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawUpgradeCostSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Upgrade Cost Settings", labelColor);

            EditorGUILayout.PropertyField(currencyIdProp);

            EditorUIUtils.DrawSubHeader("Autofill Upgrade Cost Per Level", labelColor);
            EditorGUILayout.PropertyField(initialUpgradeCostProp);
            EditorGUILayout.PropertyField(finalUpgradeCostProp);
            EditorGUILayout.PropertyField(upgradeCostCalculationMethodProp);

            if (GUILayout.Button("Auto Fill Upgrade Cost"))
            {
                AutoFillUpgradeCost(upgradeCostPerLevelProp,
                    maxLevelProp.intValue,
                    initialUpgradeCostProp.intValue,
                    finalUpgradeCostProp.intValue,
                    (ExpCalculationMethod)upgradeCostCalculationMethodProp.enumValueIndex);
            }

            EditorGUILayout.PropertyField(upgradeCostPerLevelProp, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawBaseStatsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Base Stats", labelColor);

            if (baseStatsProp != null)
            {
                EditorGUI.indentLevel++;
                // keep original idea of iterating, but PropertyField(true) would be fine too
                SerializedProperty iterator = baseStatsProp.Copy();
                SerializedProperty endProperty = baseStatsProp.GetEndProperty();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                    enterChildren = false;
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            EditorUIUtils.DrawSubHeader("Stats % Increase / Level", labelColor);
            EditorGUILayout.PropertyField(percentageIncreaseByLevelProp);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawUpgradesBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Upgrades", labelColor);

            for (int i = 0; i < statUpgradesProp.arraySize; i++)
            {
                SerializedProperty element = statUpgradesProp.GetArrayElementAtIndex(i);
                SerializedProperty upgradeNameProp = element.FindPropertyRelative("upgradeName");
                string upgradeLabel = string.IsNullOrEmpty(upgradeNameProp.stringValue)
                    ? $"Upgrade {i + 1}"
                    : upgradeNameProp.stringValue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(element, new GUIContent(upgradeLabel), true);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Upgrade", GUILayout.Width(140)))
                {
                    statUpgradesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add New Upgrade"))
            {
                statUpgradesProp.InsertArrayElementAtIndex(statUpgradesProp.arraySize);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Automatically draws any new serialized fields that weren't explicitly handled.
        /// </summary>
        private void DrawOtherSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Other Settings", labelColor);

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true); // skip script

            while (prop.NextVisible(false))
            {
                if (_handledProps.Contains(prop.name))
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // ─────────────────────────────────────────────────────────────
        // Utility Methods (kept from your original logic)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Auto fills the EXP array based on the selected calculation method.
        /// </summary>
        private void AutoFillExp(SerializedProperty expArrayProp, int maxLevel, int initialExp, int finalExp, ExpCalculationMethod method)
        {
            if (maxLevel < 1)
                return;

            expArrayProp.arraySize = maxLevel;
            for (int i = 0; i < maxLevel; i++)
            {
                int value = 0;
                float t = (maxLevel > 1) ? (float)i / (maxLevel - 1) : 0f;
                switch (method)
                {
                    case ExpCalculationMethod.Linear:
                        value = Mathf.RoundToInt(Mathf.Lerp(initialExp, finalExp, t));
                        break;
                    case ExpCalculationMethod.Exponential:
                        if (initialExp <= 0) initialExp = 1;
                        value = Mathf.RoundToInt(initialExp * Mathf.Pow((float)finalExp / initialExp, t));
                        break;
                    case ExpCalculationMethod.Custom:
                    default:
                        value = Mathf.RoundToInt(Mathf.Lerp(initialExp, finalExp, t));
                        break;
                }
                expArrayProp.GetArrayElementAtIndex(i).intValue = value;
            }
        }

        /// <summary>
        /// Auto fills the upgrade cost array based on the selected calculation method.
        /// </summary>
        private void AutoFillUpgradeCost(SerializedProperty costArrayProp, int maxLevel, int initialCost, int finalCost, ExpCalculationMethod method)
        {
            if (maxLevel < 1)
                return;

            costArrayProp.arraySize = maxLevel;
            for (int i = 0; i < maxLevel; i++)
            {
                int value = 0;
                float t = (maxLevel > 1) ? (float)i / (maxLevel - 1) : 0f;
                switch (method)
                {
                    case ExpCalculationMethod.Linear:
                        value = Mathf.RoundToInt(Mathf.Lerp(initialCost, finalCost, t));
                        break;
                    case ExpCalculationMethod.Exponential:
                        if (initialCost <= 0) initialCost = 1;
                        value = Mathf.RoundToInt(initialCost * Mathf.Pow((float)finalCost / initialCost, t));
                        break;
                    case ExpCalculationMethod.Custom:
                    default:
                        value = Mathf.RoundToInt(Mathf.Lerp(initialCost, finalCost, t));
                        break;
                }
                costArrayProp.GetArrayElementAtIndex(i).intValue = value;
            }
        }
    }
}
#endif
