using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using MaskCollect.Data;

namespace MaskCollect.Editor
{
    /// <summary>
    /// Editor window to quickly create MaskData assets from sprites.
    /// </summary>
    public class MaskCreatorWindow : EditorWindow
    {
        private const string SPRITES_PATH = "Assets/Games/MaskCollect/mask collect";
        private const string OUTPUT_PATH = "Assets/Games/MaskCollect/Data/Masks";
        private const string DATABASE_PATH = "Assets/Games/MaskCollect/Data/MaskDatabase.asset";

        private List<MaskEntry> _maskEntries = new();
        private Vector2 _scrollPosition;
        private bool _autoAssignRarity = true;

        private class MaskEntry
        {
            public Sprite Sprite;
            public string MaskId;
            public string MaskName;
            public string Description;
            public MaskRarity Rarity;
            public string AssociatedAnimal;
            public bool Create = true;
        }

        [MenuItem("Tools/MaskCollect/Mask Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<MaskCreatorWindow>("Mask Creator");
            window.minSize = new Vector2(600, 500);
            window.LoadSprites();
        }

        private void OnEnable()
        {
            LoadSprites();
        }

        private void LoadSprites()
        {
            _maskEntries.Clear();

            // Find all sprites in the mask collect folder
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { SPRITES_PATH });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string animalName = ExtractAnimalName(fileName);
                    
                    _maskEntries.Add(new MaskEntry
                    {
                        Sprite = sprite,
                        MaskId = fileName.ToLower().Replace(" ", "_"),
                        MaskName = FormatDisplayName(animalName) + " Mask",
                        Description = $"A beautiful mask inspired by the {animalName}.",
                        Rarity = AssignDefaultRarity(animalName),
                        AssociatedAnimal = animalName,
                        Create = true
                    });
                }
            }

            Debug.Log($"[MaskCreator] Found {_maskEntries.Count} sprites");
        }

        private string ExtractAnimalName(string fileName)
        {
            // Remove "_mask" suffix if present
            string name = fileName.Replace("_mask", "").Replace("_Mask", "");
            return name;
        }

        private string FormatDisplayName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // Handle special cases
            name = name.Replace("redpanda", "Red Panda");
            
            // Capitalize first letter
            if (name.Length > 0)
            {
                name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
            }

            return name;
        }

        private MaskRarity AssignDefaultRarity(string animalName)
        {
            // Assign rarity based on animal type
            string lowerName = animalName.ToLower();
            
            return lowerName switch
            {
                "lion" => MaskRarity.Legendary,
                "fox" => MaskRarity.Rare,
                "owl" => MaskRarity.Rare,
                "redpanda" or "red panda" => MaskRarity.Rare,
                "panda" => MaskRarity.Uncommon,
                "axolotl" => MaskRarity.Uncommon,
                "capybara" => MaskRarity.Uncommon,
                _ => MaskRarity.Common
            };
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Mask Creator Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This tool creates MaskData ScriptableObjects from sprites.\n" +
                "Sprites are loaded from: " + SPRITES_PATH,
                MessageType.Info);

            EditorGUILayout.Space(5);

            // Options
            EditorGUILayout.BeginHorizontal();
            _autoAssignRarity = EditorGUILayout.Toggle("Auto-assign Rarity", _autoAssignRarity);
            
            if (GUILayout.Button("Refresh Sprites", GUILayout.Width(120)))
            {
                LoadSprites();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Mask entries list
            EditorGUILayout.LabelField($"Found {_maskEntries.Count} masks:", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            for (int i = 0; i < _maskEntries.Count; i++)
            {
                DrawMaskEntry(_maskEntries[i], i);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Select All", GUILayout.Height(30)))
            {
                foreach (var entry in _maskEntries) entry.Create = true;
            }
            
            if (GUILayout.Button("Deselect All", GUILayout.Height(30)))
            {
                foreach (var entry in _maskEntries) entry.Create = false;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Create Mask Assets", GUILayout.Height(40)))
            {
                CreateMaskAssets();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            if (GUILayout.Button("Create Mask Assets + Database", GUILayout.Height(40)))
            {
                CreateMaskAssets();
                CreateOrUpdateDatabase();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawMaskEntry(MaskEntry entry, int index)
        {
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            
            // Checkbox
            entry.Create = EditorGUILayout.Toggle(entry.Create, GUILayout.Width(20));
            
            // Sprite preview
            if (entry.Sprite != null)
            {
                Rect spriteRect = GUILayoutUtility.GetRect(50, 50, GUILayout.Width(50));
                GUI.DrawTexture(spriteRect, entry.Sprite.texture, ScaleMode.ScaleToFit);
            }

            EditorGUILayout.BeginVertical();
            
            // Row 1: ID and Name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID:", GUILayout.Width(25));
            entry.MaskId = EditorGUILayout.TextField(entry.MaskId, GUILayout.Width(120));
            EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
            entry.MaskName = EditorGUILayout.TextField(entry.MaskName);
            EditorGUILayout.EndHorizontal();

            // Row 2: Rarity and Animal
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rarity:", GUILayout.Width(45));
            entry.Rarity = (MaskRarity)EditorGUILayout.EnumPopup(entry.Rarity, GUILayout.Width(100));
            EditorGUILayout.LabelField("Animal:", GUILayout.Width(45));
            entry.AssociatedAnimal = EditorGUILayout.TextField(entry.AssociatedAnimal);
            EditorGUILayout.EndHorizontal();

            // Row 3: Description
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Desc:", GUILayout.Width(35));
            entry.Description = EditorGUILayout.TextField(entry.Description);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void CreateMaskAssets()
        {
            // Ensure output directory exists
            if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
            {
                CreateFolderPath(OUTPUT_PATH);
            }

            int created = 0;
            int skipped = 0;

            foreach (var entry in _maskEntries)
            {
                if (!entry.Create) continue;

                string assetPath = $"{OUTPUT_PATH}/{entry.MaskId}.asset";

                // Check if asset already exists
                if (AssetDatabase.LoadAssetAtPath<MaskData>(assetPath) != null)
                {
                    if (!EditorUtility.DisplayDialog("Asset Exists",
                        $"Asset '{entry.MaskId}' already exists. Overwrite?",
                        "Overwrite", "Skip"))
                    {
                        skipped++;
                        continue;
                    }
                }

                // Create MaskData asset
                MaskData maskData = ScriptableObject.CreateInstance<MaskData>();
                
                // Use SerializedObject to set private fields
                SerializedObject so = new SerializedObject(maskData);
                so.FindProperty("maskId").stringValue = entry.MaskId;
                so.FindProperty("maskName").stringValue = entry.MaskName;
                so.FindProperty("description").stringValue = entry.Description;
                so.FindProperty("maskSprite").objectReferenceValue = entry.Sprite;
                so.FindProperty("rarity").enumValueIndex = (int)entry.Rarity;
                so.FindProperty("associatedAnimal").stringValue = entry.AssociatedAnimal;
                so.ApplyModifiedPropertiesWithoutUndo();

                // Save asset
                AssetDatabase.CreateAsset(maskData, assetPath);
                created++;

                Debug.Log($"[MaskCreator] Created: {entry.MaskName} ({entry.Rarity})");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Mask Creator",
                $"Created {created} mask assets.\nSkipped {skipped} existing assets.",
                "OK");
        }

        private void CreateOrUpdateDatabase()
        {
            // Ensure Data folder exists
            string dataFolder = Path.GetDirectoryName(DATABASE_PATH);
            if (!AssetDatabase.IsValidFolder(dataFolder))
            {
                CreateFolderPath(dataFolder);
            }

            // Load or create database
            MaskDatabase database = AssetDatabase.LoadAssetAtPath<MaskDatabase>(DATABASE_PATH);
            
            if (database == null)
            {
                database = ScriptableObject.CreateInstance<MaskDatabase>();
                AssetDatabase.CreateAsset(database, DATABASE_PATH);
            }

            // Find all MaskData assets
            string[] guids = AssetDatabase.FindAssets("t:MaskData", new[] { OUTPUT_PATH });
            
            SerializedObject so = new SerializedObject(database);
            SerializedProperty allMasksProp = so.FindProperty("allMasks");
            allMasksProp.ClearArray();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MaskData mask = AssetDatabase.LoadAssetAtPath<MaskData>(path);
                
                if (mask != null)
                {
                    allMasksProp.InsertArrayElementAtIndex(allMasksProp.arraySize);
                    allMasksProp.GetArrayElementAtIndex(allMasksProp.arraySize - 1).objectReferenceValue = mask;
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log($"[MaskCreator] Database updated with {guids.Length} masks");
            EditorUtility.DisplayDialog("Database Created",
                $"MaskDatabase created/updated with {guids.Length} masks.\n\nPath: {DATABASE_PATH}",
                "OK");

            // Ping the database in Project window
            EditorGUIUtility.PingObject(database);
            Selection.activeObject = database;
        }

        private void CreateFolderPath(string path)
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];

            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }
}
