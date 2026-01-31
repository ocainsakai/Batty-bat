#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Custom inspector for <see cref="ShopItem"/>.
    /// Displays grouped fields and automatically renders any new serialized fields in the "Other Settings" box.
    /// </summary>
    [CustomEditor(typeof(ShopItem))]
    public class ShopItemEditor : Editor
    {
        // ---------- Serialized Properties ----------
        private SerializedProperty itemTitleTranslatedProp;
        private SerializedProperty itemDescriptionTranslatedProp;
        private SerializedProperty currencyRewardsProp;
        private SerializedProperty iconsProp;
        private SerializedProperty framesProp;
        private SerializedProperty characterDataProp;
        private SerializedProperty inventoryItemsProp;

        // ---------- Handled Property Names ----------
        private HashSet<string> handledProps;

        // ---------- Styling ----------
        private readonly Color headerColor = new Color(0.12f, 0.45f, 0.80f);
        private readonly Color labelColor = new Color(0.255f, 0.412f, 0.882f);

        private void OnEnable()
        {
            itemTitleTranslatedProp = serializedObject.FindProperty("itemTitleTranslated");
            itemDescriptionTranslatedProp = serializedObject.FindProperty("itemDescriptionTranslated");
            currencyRewardsProp = serializedObject.FindProperty("currencyRewards");
            iconsProp = serializedObject.FindProperty("icons");
            framesProp = serializedObject.FindProperty("frames");
            characterDataProp = serializedObject.FindProperty("characterData");
            inventoryItemsProp = serializedObject.FindProperty("inventoryItems");

            handledProps = new HashSet<string>
            {
                "m_Script",
                "itemId",
                "itemTitle",
                itemTitleTranslatedProp.name,
                "itemDescription",
                itemDescriptionTranslatedProp.name,
                "price",
                "currency",
                "itemIcon",
                "category",
                "isCurrencyPackage",
                currencyRewardsProp.name,
                iconsProp.name,
                framesProp.name,
                characterDataProp.name,
                inventoryItemsProp.name
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            const string logoPath = "Assets/BulletHellTemplate/Core/Editor/EditorLogo/bizachi.png";
            EditorUIUtils.DrawTitleHeader("Shop Item", logoPath, headerColor);

            DrawMainItemBox();
            DrawPackageSettingsBox();
            DrawOtherSettingsBox();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMainItemBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Main Item Information", labelColor);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemId"),
                new GUIContent("Item ID", "Unique identifier for the shop item."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemTitle"),
                new GUIContent("Item Title", "Display title of the shop item."));
            EditorGUILayout.PropertyField(itemTitleTranslatedProp,
                new GUIContent("Translated Titles", "Translated titles of the item."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemDescription"),
                new GUIContent("Item Description", "Description of the shop item."));
            EditorGUILayout.PropertyField(itemDescriptionTranslatedProp,
                new GUIContent("Translated Descriptions", "Translated descriptions of the item."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("price"),
                new GUIContent("Price", "Price of the item."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currency"),
                new GUIContent("Currency", "Currency type for the price."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemIcon"),
                new GUIContent("Item Icon", "Icon sprite for the shop item."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("category"),
                new GUIContent("Category", "Category of the shop item."));

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawPackageSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Package Settings", labelColor);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isCurrencyPackage"),
                new GUIContent("Is Currency Package?", "Determines if this item is a currency package."));

            if (serializedObject.FindProperty("isCurrencyPackage").boolValue)
            {
                EditorGUILayout.HelpBox(
                    "This item will act as a currency reward. Other items such as icons, frames, and characters will not accompany this purchase. It will remain available after purchase.",
                    MessageType.Info
                );
                EditorGUILayout.PropertyField(currencyRewardsProp,
                    new GUIContent("Currency Rewards", "List of currency rewards."), true);
            }
            else
            {
                EditorGUILayout.PropertyField(iconsProp,
                    new GUIContent("Icons", "List of icons unlocked upon purchase."), true);
                EditorGUILayout.PropertyField(framesProp,
                    new GUIContent("Frames", "List of frames unlocked upon purchase."), true);
                EditorGUILayout.PropertyField(characterDataProp,
                    new GUIContent("Characters", "List of characters unlocked upon purchase."), true);
                EditorGUILayout.PropertyField(inventoryItemsProp,
                    new GUIContent("Inventory Items", "List of inventory items unlocked upon purchase."), true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawOtherSettingsBox()
        {
            EditorGUILayout.BeginVertical("box");
            EditorUIUtils.DrawSubHeader("Other Settings", labelColor);

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true); // skip script field
            while (prop.NextVisible(false))
            {
                if (handledProps.Contains(prop.name)) continue;
                EditorGUILayout.PropertyField(prop, true);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
    }
}
#endif
