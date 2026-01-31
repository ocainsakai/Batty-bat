#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="GameInstance"/>.
    /// Groups related settings into clear, fold-out sections
    /// and exposes every serialized field defined in the component.
    /// </summary>
    [CustomEditor(typeof(GameInstance))]
    public class GameInstanceEditor : Editor
    {
        /* ---------- Serialized Properties ---------- */
        //--- Game data
        SerializedProperty currencyDataProp;
        SerializedProperty characterDataProp;
        SerializedProperty iconItemsProp;
        SerializedProperty frameItemsProp;
        SerializedProperty inventoryItemsProp;
        SerializedProperty characterTypesProp;
        SerializedProperty mapInfoDataProp;
        SerializedProperty questDataProp;
        SerializedProperty couponDataProp;
        SerializedProperty shopDataProp;
        SerializedProperty newPlayerRewardProp;
        SerializedProperty dailyRewardProp;
        SerializedProperty pvpModesProp;

        SerializedProperty battlePassDataProp;
        SerializedProperty mainMenuSceneProp;

        //--- Auth settings
        SerializedProperty minPasswordLengthProp;
        SerializedProperty maxPasswordLengthProp;
        SerializedProperty requireUppercaseProp;
        SerializedProperty requireLowercaseProp;
        SerializedProperty requireNumbersProp;
        SerializedProperty requireSpecialProp;

        //--- Core references
        SerializedProperty characterEntityProp;
        SerializedProperty mobileGameplayProp;
        SerializedProperty desktopGameplayProp;
        SerializedProperty platformTypeProp;
        SerializedProperty worldTypeProp;

        //--- Progression blocks
        SerializedProperty accountLevelsProp;
        SerializedProperty characterMasteryProp;
        SerializedProperty masteryLevelsProp;

        //--- Element settings
        SerializedProperty advantageDamageIncreaseProp;
        SerializedProperty weaknessDamageReductionProp;

        //--- Battle-Pass settings
        SerializedProperty battlePassEXPProp;
        SerializedProperty battlePassDurationDaysProp;
        SerializedProperty goldCurrencyProp;
        SerializedProperty maxLevelPassProp;
        SerializedProperty baseExpPassProp;
        SerializedProperty seasonLengthProp;
        SerializedProperty incPerLevelPassProp;
        SerializedProperty passCurrencyIDProp;
        SerializedProperty battlePassPriceProp;

        //--- Name-change settings
        SerializedProperty changeNameTickProp;
        SerializedProperty minNameLengthProp;
        SerializedProperty maxNameLengthProp;
        SerializedProperty ticketsToChangeProp;
        SerializedProperty needTicketProp;

        /* ---------- Foldout States ---------- */
        bool showAccountExp = false;
        bool showMasteryExp = false;
        bool showMasteryLevelsSection = false;
        readonly List<bool> masteryLevelsFoldouts = new();

        Color titleColor = new Color(0.12f, 0.45f, 0.80f);
        Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        void OnEnable()
        {
            /* --- Assign properties --- */
            currencyDataProp = Find("currencyData");
            characterDataProp = Find("characterData");
            iconItemsProp = Find("iconItems");
            frameItemsProp = Find("frameItems");
            inventoryItemsProp = Find("inventoryItems");
            characterTypesProp = Find("characterTypes");
            mapInfoDataProp = Find("mapInfoData");
            questDataProp = Find("questData");
            couponDataProp = Find("couponData");
            shopDataProp = Find("shopData");
            battlePassDataProp = Find("battlePassData");
            newPlayerRewardProp = Find("newPlayerRewardItems");
            dailyRewardProp = Find("dailyRewardItems");
            pvpModesProp = Find("pvpModes");

            minPasswordLengthProp = Find("minPasswordLength");
            maxPasswordLengthProp = Find("maxPasswordLength");
            requireUppercaseProp = Find("requireUppercase");
            requireLowercaseProp = Find("requireLowercase");
            requireNumbersProp = Find("requireNumbers");
            requireSpecialProp = Find("requireSpecial");

            characterEntityProp = Find("characterEntity");
            mobileGameplayProp = Find("mobileGameplay");
            desktopGameplayProp = Find("desktopGameplay");
            platformTypeProp = Find("platformType");
            worldTypeProp = Find("worldType");

            accountLevelsProp = Find("accountLevels");
            characterMasteryProp = Find("characterMastery");
            masteryLevelsProp = Find("masteryLevels");

            mainMenuSceneProp = Find("mainMenuScene");
            goldCurrencyProp = Find("goldCurrency");
            battlePassEXPProp = Find("BattlePassEXP");
            battlePassDurationDaysProp = Find("battlePassDurationDays");
            advantageDamageIncreaseProp = Find("advantageDamageIncrease");
            weaknessDamageReductionProp = Find("weaknessDamageReduction");
                      
            maxLevelPassProp = Find("maxLevelPass");
            baseExpPassProp = Find("baseExpPass");
            seasonLengthProp = Find("SeasonLengthInDays");
            incPerLevelPassProp = Find("incPerLevelPass");
            passCurrencyIDProp = Find("battlePassCurrencyID");
            battlePassPriceProp = Find("battlePassPrice");

            changeNameTickProp = Find("changeNameTick");
            minNameLengthProp = Find("minNameLength");
            maxNameLengthProp = Find("maxNameLength");
            ticketsToChangeProp = Find("ticketsToChange");
            needTicketProp = Find("needTicket");

            InitMasteryLevelFoldouts();
        }

        SerializedProperty Find(string name) => serializedObject.FindProperty(name);

        void InitMasteryLevelFoldouts()
        {
            masteryLevelsFoldouts.Clear();
            for (int i = 0; i < masteryLevelsProp.arraySize; i++)
                masteryLevelsFoldouts.Add(true);
        }

        /* ---------- Inspector GUI ---------- */
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            const string pathTitle = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";

            EditorUIUtils.DrawTitleHeader("Game Instance", pathTitle, titleColor);
            DrawGameDataSection();
            DrawAuthSettingsSection();
            DrawNameChangeSection();
            DrawBattlePassSection();
            DrawAccountLevelsSection();
            DrawElementSettingsSection();
            DrawCharacterMasterySection();
            DrawMasteryLevelsSection();
            DrawPrefabReferencesSection();

            serializedObject.ApplyModifiedProperties();
        }             

        /* ---------- Helper Draw Methods ---------- */  

        void DrawGameDataSection()
        {            
            EditorGUILayout.BeginVertical("box");
            Space();
            EditorUIUtils.DrawSubHeader("Game Data (Scriptables)", labelColor);
            PropertyField(currencyDataProp);
            Space();
            PropertyField(characterDataProp);
            Space();
            PropertyField(iconItemsProp);
            Space();
            PropertyField(frameItemsProp);
            Space();
            PropertyField(inventoryItemsProp);
            Space();
            PropertyField(characterTypesProp);
            Space();
            PropertyField(mapInfoDataProp);
            Space();
            PropertyField(questDataProp);
            Space();
            PropertyField(couponDataProp);
            Space();
            PropertyField(shopDataProp);
            Space();
            PropertyField(battlePassDataProp);
            Space();
            PropertyField(newPlayerRewardProp);
            Space();
            PropertyField(dailyRewardProp);
            Space();
            PropertyField(pvpModesProp);
            EditorGUILayout.EndVertical();
            Space();
        }
        void DrawAuthSettingsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Authentication Settings", labelColor);
            PropertyField(minPasswordLengthProp, new GUIContent("Min Password Length"));
            PropertyField(maxPasswordLengthProp, new GUIContent("Max Password Length"));
            PropertyField(requireUppercaseProp, new GUIContent("Require Uppercase"));
            PropertyField(requireLowercaseProp, new GUIContent("Require Lowercase"));
            PropertyField(requireNumbersProp, new GUIContent("Require Numbers"));
            PropertyField(requireSpecialProp, new GUIContent("Require Special Characters"));

            EditorGUILayout.EndVertical();
            Space();
        }

        void DrawAccountLevelsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Account Levels", labelColor);
            var max = accountLevelsProp.FindPropertyRelative("accountMaxLevel");
            var initial = accountLevelsProp.FindPropertyRelative("initialExp");
            var final = accountLevelsProp.FindPropertyRelative("finalExp");
            var method = accountLevelsProp.FindPropertyRelative("accountExpCalculationMethod");
            var array = accountLevelsProp.FindPropertyRelative("accountExpPerLevel");

            PropertyField(max, new GUIContent("Max Level"));
            PropertyField(initial);
            PropertyField(final);
            PropertyField(method, new GUIContent("Progression Method"));

            showAccountExp = Fold("Account EXP Per Level", showAccountExp);
            if (showAccountExp) EditableIntArray(array, "Level ");

            if (Button("Auto-Fill Account EXP"))
            {
                ((GameInstance)target).AutoFillAccountExp();
                serializedObject.Update();
            }
            EditorGUILayout.EndVertical();
            Space();
        }

        void DrawCharacterMasterySection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Character Mastery", labelColor);
            var max = characterMasteryProp.FindPropertyRelative("maxMasteryLevel");
            var initial = characterMasteryProp.FindPropertyRelative("initialExp");
            var final = characterMasteryProp.FindPropertyRelative("finalExp");
            var method = characterMasteryProp.FindPropertyRelative("characterExpCalculationMethod");
            var array = characterMasteryProp.FindPropertyRelative("masteryExpPerLevel");

            PropertyField(max, new GUIContent("Max Level"));
            PropertyField(initial);
            PropertyField(final);
            PropertyField(method, new GUIContent("Progression Method"));

            showMasteryExp = Fold("Mastery EXP Per Level", showMasteryExp);
            if (showMasteryExp) EditableIntArray(array, "Level ");

            if (Button("Auto-Fill Mastery EXP"))
            {
                ((GameInstance)target).AutoFillMasteryExp();
                serializedObject.Update();
            }
            EditorGUILayout.EndVertical();
            Space();
        }
        void DrawMasteryLevelsSection()
        {
            EditorUIUtils.DrawSubHeader("Mastery Levels", labelColor);

            showMasteryLevelsSection = EditorGUILayout.Foldout(
                showMasteryLevelsSection,
                " Show Mastery Levels",
                true,
                EditorStyles.foldoutHeader);

            if (!showMasteryLevelsSection)
            {
                EditorGUILayout.Space();
                return;
            }

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < masteryLevelsProp.arraySize; i++)
            {
                var levelProp = masteryLevelsProp.GetArrayElementAtIndex(i);

                masteryLevelsFoldouts[i] =
                    EditorGUILayout.Foldout(masteryLevelsFoldouts[i], $"Level {i + 1}", true);
                if (masteryLevelsFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    PropertyField(levelProp.FindPropertyRelative("masteryName"));
                    PropertyField(levelProp.FindPropertyRelative("masteryNameTranslated"));
                    PropertyField(levelProp.FindPropertyRelative("masteryIcon"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space(4);
            }

            if (GUILayout.Button("Add New Mastery Level"))
            {
                masteryLevelsProp.InsertArrayElementAtIndex(masteryLevelsProp.arraySize);
                InitMasteryLevelFoldouts();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        void DrawMasteryLevelsSectionOld()
        {
            EditorUIUtils.DrawSubHeader("Mastery Levels", labelColor);
            if (!Button(showMasteryLevelsSection ? "Hide Mastery Levels" : "Show Mastery Levels"))
                return;

            showMasteryLevelsSection = !showMasteryLevelsSection;
            if (!showMasteryLevelsSection) return;

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < masteryLevelsProp.arraySize; i++)
            {
                var levelProp = masteryLevelsProp.GetArrayElementAtIndex(i);

                if (i >= masteryLevelsFoldouts.Count)
                    masteryLevelsFoldouts.Add(true);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                masteryLevelsFoldouts[i] =
                    EditorGUILayout.Foldout(masteryLevelsFoldouts[i], $"Level {i + 1}", true);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    masteryLevelsProp.DeleteArrayElementAtIndex(i);
                    InitMasteryLevelFoldouts();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                if (masteryLevelsFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    PropertyField(levelProp.FindPropertyRelative("masteryName"));
                    PropertyField(levelProp.FindPropertyRelative("masteryNameTranslated"));
                    PropertyField(levelProp.FindPropertyRelative("masteryIcon"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
                Space();
            }
            if (Button("Add New Mastery Level"))
            {
                masteryLevelsProp.InsertArrayElementAtIndex(masteryLevelsProp.arraySize);
                InitMasteryLevelFoldouts();
            }
            EditorGUILayout.EndVertical();
            Space();
        }

        void DrawBattlePassSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Battle Pass Settings", labelColor);       
            PropertyField(maxLevelPassProp, new GUIContent("Max Level"));
            PropertyField(baseExpPassProp, new GUIContent("Base EXP"));
            PropertyField(seasonLengthProp, new GUIContent("Season Length (Days)"));
            PropertyField(incPerLevelPassProp, new GUIContent("EXP Increase / Level"));
            PropertyField(passCurrencyIDProp, new GUIContent("Pass Currency ID"));
            PropertyField(battlePassPriceProp, new GUIContent("Pass Price"));
            EditorGUILayout.EndVertical();
            Space();
        }

        void DrawNameChangeSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Name Change Settings", labelColor);
            PropertyField(changeNameTickProp, new GUIContent("Ticket Currency ID"));
            PropertyField(minNameLengthProp, new GUIContent("Min Length"));
            PropertyField(maxNameLengthProp, new GUIContent("Max Length"));
            PropertyField(ticketsToChangeProp, new GUIContent("Tickets Required"));
            PropertyField(needTicketProp, new GUIContent("Need Ticket?"));
            EditorGUILayout.EndVertical();
            Space();
        }

        void DrawElementSettingsSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Gameplay Settings", labelColor);

            PropertyField(mainMenuSceneProp, new GUIContent("Scene to return after Battle"));
            PropertyField(goldCurrencyProp, new GUIContent("Currency Reward in Battle"));
            PropertyField(battlePassEXPProp, new GUIContent("BattlePass EXP Reward in Battle"));
            PropertyField(battlePassDurationDaysProp, new GUIContent("BattlePass duration in days"));         
            EditorGUILayout.Space();

            EditorUIUtils.DrawSubHeader("Elemental Settings", labelColor);
            PropertyField(advantageDamageIncreaseProp, new GUIContent("Advantage Damage Increase"));
            PropertyField(weaknessDamageReductionProp, new GUIContent("Weakness Damage Reduction"));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }


        void DrawPrefabReferencesSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Prefab & Platform References", labelColor);
            PropertyField(characterEntityProp);
            PropertyField(platformTypeProp);
            PropertyField(worldTypeProp);
            PropertyField(mobileGameplayProp);
            PropertyField(desktopGameplayProp);
            EditorGUILayout.EndVertical();
        }

        /* ---------- Small Utility Wrappers ---------- */
        void PropertyField(SerializedProperty prop) =>
            EditorGUILayout.PropertyField(prop, true);                   

        void PropertyField(SerializedProperty prop, GUIContent label) =>
            EditorGUILayout.PropertyField(prop, label);
        void PropertyField(SerializedProperty prop, GUIContent label, params GUILayoutOption[] options) =>
            EditorGUILayout.PropertyField(prop, label, options);       
        void Label(string text) => EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        void Space() => EditorGUILayout.Space();
        void Space(int space) => EditorGUILayout.Space(space);
        bool Button(string txt) => GUILayout.Button(txt);
        bool Fold(string txt, bool state) => EditorGUILayout.Foldout(state, txt, true);

        void EditableIntArray(SerializedProperty arrayProp, string labelPrefix)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                PropertyField(arrayProp.GetArrayElementAtIndex(i), new GUIContent($"{labelPrefix}{i + 1}"), GUILayout.MaxWidth(220));
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    arrayProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Element")) arrayProp.InsertArrayElementAtIndex(arrayProp.arraySize);
            EditorGUI.indentLevel--;
        }
    }
}
#endif
