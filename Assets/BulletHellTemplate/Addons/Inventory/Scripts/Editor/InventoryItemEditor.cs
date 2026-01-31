#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom editor for the InventoryItem ScriptableObject.
    /// Organizes the inspector into sections for General Information, Item Stats, and Item Upgrades.
    /// Provides buttons to add and remove item upgrades and displays the item stats always visible.
    /// </summary>
    [CustomEditor(typeof(InventoryItem))]
    public class InventoryItemEditor : Editor
    {
        SerializedProperty titleProp;
        SerializedProperty titleTranslatedByLanguageProp;
        SerializedProperty descriptionProp;
        SerializedProperty descriptionTranslatedByLanguageProp;
        SerializedProperty itemIdProp;
        SerializedProperty categoryProp;
        SerializedProperty itemIconProp;
        SerializedProperty itemStatsProp;
        SerializedProperty itemUpgradesProp;
        SerializedProperty isUnlockedProp;
        SerializedProperty rarityProp;
        SerializedProperty slotProp;

        /// <summary>
        /// Initializes the serialized properties.
        /// </summary>
        private void OnEnable()
        {
            titleProp = serializedObject.FindProperty("title");
            titleTranslatedByLanguageProp = serializedObject.FindProperty("titleTranslatedByLanguage");
            descriptionProp = serializedObject.FindProperty("description");
            descriptionTranslatedByLanguageProp = serializedObject.FindProperty("descriptionTranslatedByLanguage");
            itemIdProp = serializedObject.FindProperty("itemId");
            categoryProp = serializedObject.FindProperty("category");
            itemIconProp = serializedObject.FindProperty("itemIcon");
            itemStatsProp = serializedObject.FindProperty("itemStats");
            itemUpgradesProp = serializedObject.FindProperty("itemUpgrades");
            isUnlockedProp = serializedObject.FindProperty("isUnlocked");
            rarityProp = serializedObject.FindProperty("rarity");
            slotProp = serializedObject.FindProperty("slot");
        }

        /// <summary>
        /// Draws the custom inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // General Information Section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("General Information", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(titleProp);
            EditorGUILayout.PropertyField(titleTranslatedByLanguageProp, new GUIContent("Title Translations"), true);
            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(descriptionTranslatedByLanguageProp, new GUIContent("Description Translations"), true);
            EditorGUILayout.PropertyField(itemIdProp);
            EditorGUILayout.PropertyField(categoryProp);
            EditorGUILayout.PropertyField(itemIconProp);
            EditorGUILayout.PropertyField(isUnlockedProp);
            EditorGUILayout.PropertyField(rarityProp);
            EditorGUILayout.PropertyField(slotProp);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Item Stats Section (always visible)
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Item Stats", EditorStyles.boldLabel);
            if (itemStatsProp != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(itemStatsProp, true);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Item Upgrades Section with add/remove buttons
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Item Upgrades", EditorStyles.boldLabel);
            for (int i = 0; i < itemUpgradesProp.arraySize; i++)
            {
                SerializedProperty upgradeProp = itemUpgradesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(upgradeProp, new GUIContent("Upgrade " + (i + 1)), true);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove Upgrade", GUILayout.Width(120)))
                {
                    itemUpgradesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            if (GUILayout.Button("Add New Upgrade"))
            {
                itemUpgradesProp.InsertArrayElementAtIndex(itemUpgradesProp.arraySize);
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
