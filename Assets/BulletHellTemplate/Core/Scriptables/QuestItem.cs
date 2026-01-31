using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BulletHellTemplate
{
    /// <summary>
    /// ScriptableObject representing a quest item with its rewards and requirements.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestItem", menuName = "Quest/Quest Item", order = 51)]
    public class QuestItem : ScriptableObject
    {
        [Tooltip("Unique identifier for the quest")]
        public int questId;

        [Tooltip("Quest icon")]
        public Sprite icon;

        [Tooltip("Quest title")]
        public string title;

        [Tooltip("Localized quest titles")]
        public NameTranslatedByLanguage[] titleTranslated;

        [Tooltip("Quest description")]
        public string description;

        [Tooltip("Localized quest descriptions")]
        public DescriptionTranslatedByLanguage[] descriptionTranslated;

        [Tooltip("Currency type rewarded")]
        public string currencyReward = "GO";

        [Tooltip("Amount of currency rewarded")]
        public int currencyAmount;

        [Tooltip("Battle pass experience points")]
        public int battlePassExp;

        [Tooltip("Account experience points")]
        public int accountExp;

        [Tooltip("Experience points for selected character")]
        public int selectedCharacterExp;

        [Tooltip("Type of reward for the quest")]
        public QuestReward questReward;

        [Tooltip("Data for character reward")]
        public CharacterData characterData;

        [Tooltip("Icon item reward")]
        public IconItem iconItem;

        [Tooltip("Frame item reward")]
        public FrameItem frameItem;

        [Tooltip("Inventory item reward")]
        public InventoryItem inventoryItem;

        [Tooltip("Single quest requirement")]
        public QuestRequirement requirement = new QuestRequirement();

        [Tooltip("Type of quest")]
        public QuestType questType;
    }

    /// <summary>
    /// Enumeration of quest requirement types.
    /// </summary>
    public enum QuestRequirementType
    {
        KillMonster,
        CompleteMap,
        LevelUpAccount,
        KillMonsterWithSpecificCharacter,
        CompleteMapWithSpecificCharacter,
        LevelUpCharacter
    }

    /// <summary>
    /// Class representing a quest requirement.
    /// </summary>
    [System.Serializable]
    public class QuestRequirement
    {
        [Tooltip("Type of quest requirement")]
        public QuestRequirementType requirementType;

        [Tooltip("Number required for the quest (renamed according to the requirement type)")]
        public int targetAmount;

        [Header("Required for map-related quests")]
        [Tooltip("Map required for completion")]
        public MapInfoData targetMap;

        [Header("Required for character-specific quests")]
        [Tooltip("Specific character required")]
        public CharacterData targetCharacter;
    }

    /// <summary>
    /// Enumeration of quest rewards.
    /// </summary>
    [System.Serializable]
    public enum QuestReward
    {
        None,
        CharacterReward,
        IconReward,
        FrameReward,
        ItemReward
    }

    /// <summary>
    /// Enumeration of quest types.
    /// </summary>
    [System.Serializable]
    public enum QuestType
    {
        Normal,
        Daily,
        Repeat
    }

#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for QuestItem that organizes properties into boxes and conditionally displays fields.
    /// </summary>
    [CustomEditor(typeof(QuestItem))]
    public class QuestItemEditor : Editor
    {
        /// <summary>
        /// Overrides the default Inspector GUI to provide custom layout and conditional property drawing.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ======================
            // BASIC PROPERTIES BOX
            // ======================
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questId"), new GUIContent("Quest ID", "Unique identifier for the quest"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), new GUIContent("Icon", "Quest icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("title"), new GUIContent("Title", "Quest title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("titleTranslated"), new GUIContent("Title Translated", "Localized quest titles"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"), new GUIContent("Description", "Quest description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("descriptionTranslated"), new GUIContent("Description Translated", "Localized quest descriptions"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currencyReward"), new GUIContent("Currency Reward", "Currency type rewarded"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currencyAmount"), new GUIContent("Currency Amount", "Amount of currency rewarded"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("battlePassExp"), new GUIContent("Battle Pass Exp", "Battle pass experience points"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accountExp"), new GUIContent("Account Exp", "Account experience points"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedCharacterExp"), new GUIContent("Selected Character Exp", "Experience points for selected character"));
            EditorGUILayout.EndVertical();

            // ===============
            // QUEST REWARD BOX
            // ===============
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questReward"), new GUIContent("Quest Reward", "Type of reward for the quest"));

            // Show the relevant reward field(s) based on the selected QuestReward
            var questRewardProp = serializedObject.FindProperty("questReward");
            var questRewardValue = (QuestReward)questRewardProp.enumValueIndex;

            switch (questRewardValue)
            {
                case QuestReward.CharacterReward:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("characterData"), new GUIContent("Character Data", "Data for character reward"));
                    break;

                case QuestReward.IconReward:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("iconItem"), new GUIContent("Icon Item", "Icon item reward"));
                    break;

                case QuestReward.FrameReward:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("frameItem"), new GUIContent("Frame Item", "Frame item reward"));
                    break;

                case QuestReward.ItemReward:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inventoryItem"), new GUIContent("Inventory Item", "Inventory item reward"));
                    break;

                case QuestReward.None:
                default:
                    // Do not display any item-based fields
                    break;
            }
            EditorGUILayout.EndVertical();

            // ======================
            // QUEST REQUIREMENT BOX
            // ======================
            EditorGUILayout.BeginVertical("box");
            SerializedProperty requirementProp = serializedObject.FindProperty("requirement");
            // Draw the requirementType first
            EditorGUILayout.PropertyField(requirementProp.FindPropertyRelative("requirementType"), new GUIContent("Requirement Type", "Type of quest requirement"));

            // Based on requirementType, rename and display the relevant fields
            var reqTypeIndex = requirementProp.FindPropertyRelative("requirementType").enumValueIndex;
            var requirementTypeEnum = (QuestRequirementType)reqTypeIndex;

            // We'll store references for easier reading
            SerializedProperty targetAmountProp = requirementProp.FindPropertyRelative("targetAmount");
            SerializedProperty targetMapProp = requirementProp.FindPropertyRelative("targetMap");
            SerializedProperty targetCharacterProp = requirementProp.FindPropertyRelative("targetCharacter");

            // Use a switch to rename targetAmount and show/hide fields
            switch (requirementTypeEnum)
            {
                case QuestRequirementType.KillMonster:
                    EditorGUILayout.PropertyField(
                        targetAmountProp,
                        new GUIContent("Monsters Amount", "Number of monsters to kill")
                    );
                    // No map or character needed
                    break;

                case QuestRequirementType.CompleteMap:
                    EditorGUILayout.PropertyField(
                        targetAmountProp,
                        new GUIContent("Times to Complete", "Number of times the map must be completed")
                    );
                    EditorGUILayout.PropertyField(
                        targetMapProp,
                        new GUIContent("Map Required", "Map required for completion")
                    );
                    // No character needed
                    break;

                case QuestRequirementType.LevelUpAccount:
                    EditorGUILayout.PropertyField(
                        targetAmountProp,
                        new GUIContent("Level Required", "Account level required")
                    );
                    // No map or character needed
                    break;

                case QuestRequirementType.KillMonsterWithSpecificCharacter:
                    EditorGUILayout.PropertyField(
                        targetAmountProp,
                        new GUIContent("Monsters Amount", "Number of monsters to kill")
                    );
                    EditorGUILayout.PropertyField(
                        targetCharacterProp,
                        new GUIContent("Required Character", "Specific character required for killing monsters")
                    );
                    // No map needed
                    break;

                case QuestRequirementType.CompleteMapWithSpecificCharacter:
                    EditorGUILayout.PropertyField(
                        targetAmountProp,
                        new GUIContent("Times to Complete", "Number of times the map must be completed")
                    );
                    EditorGUILayout.PropertyField(
                        targetMapProp,
                        new GUIContent("Map Required", "Map required for completion")
                    );
                    EditorGUILayout.PropertyField(
                        targetCharacterProp,
                        new GUIContent("Required Character", "Specific character required for map completion")
                    );
                    break;

                case QuestRequirementType.LevelUpCharacter:
                    EditorGUILayout.PropertyField(
                        targetAmountProp,
                        new GUIContent("Character Level Required", "Character level required")
                    );
                    EditorGUILayout.PropertyField(
                        targetCharacterProp,
                        new GUIContent("Required Character", "Specific character to level up")
                    );
                    // No map needed
                    break;
            }
            EditorGUILayout.EndVertical();

            // =============
            // QUEST TYPE BOX
            // =============
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("questType"), new GUIContent("Quest Type", "Type of quest"));
            EditorGUILayout.EndVertical();

            // Apply any modified properties to the serialized object
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
