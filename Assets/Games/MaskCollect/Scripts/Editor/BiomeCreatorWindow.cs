using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using MaskCollect.Data;

namespace MaskCollect.Editor
{
    /// <summary>
    /// Editor window to quickly create BiomeData assets and assign animals.
    /// </summary>
    public class BiomeCreatorWindow : EditorWindow
    {
        private const string OUTPUT_PATH = "Assets/Games/MaskCollect/Data/Biomes";
        private const string DATABASE_PATH = "Assets/Games/MaskCollect/Data/BiomeDatabase.asset";

        private List<BiomeEntry> _biomeEntries = new();
        private Vector2 _scrollPosition;
        private MaskDatabase _maskDatabase;

        private class BiomeEntry
        {
            public BiomeType Type;
            public string Name;
            public string Description;
            public BiomeDifficulty Difficulty;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public string[] AnimalTypes;
            public bool IsUnlockedByDefault;
            public int MasksRequiredToUnlock;
            public bool Create = true;
        }

        [MenuItem("Tools/MaskCollect/Biome Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BiomeCreatorWindow>("Biome Creator");
            window.minSize = new Vector2(650, 600);
            window.InitializeBiomes();
        }

        private void OnEnable()
        {
            InitializeBiomes();
            LoadMaskDatabase();
        }

        private void LoadMaskDatabase()
        {
            _maskDatabase = AssetDatabase.LoadAssetAtPath<MaskDatabase>("Assets/Games/MaskCollect/Data/MaskDatabase.asset");
        }

        private void InitializeBiomes()
        {
            _biomeEntries = new List<BiomeEntry>
            {
                new BiomeEntry
                {
                    Type = BiomeType.PeacefulMeadow,
                    Name = "Đồng Cỏ Thân Thiện",
                    Description = "Khu vực mở đầu game, nơi hội tụ các loài gần gũi nhất. Không gian mở, dễ di chuyển.",
                    Difficulty = BiomeDifficulty.Easy,
                    PrimaryColor = new Color(0.6f, 0.9f, 0.4f), // Xanh lá non
                    SecondaryColor = new Color(1f, 0.9f, 0.4f), // Vàng nắng
                    AnimalTypes = new[] { "dog", "cat", "rabbit", "bird" },
                    IsUnlockedByDefault = true,
                    MasksRequiredToUnlock = 0,
                    Create = true
                },
                new BiomeEntry
                {
                    Type = BiomeType.MysticForest,
                    Name = "Rừng Rậm Bí Ẩn",
                    Description = "Khu vực yêu cầu người chơi phải khám phá sâu hơn để tìm thấy các loài nhút nhát.",
                    Difficulty = BiomeDifficulty.Medium,
                    PrimaryColor = new Color(0.2f, 0.5f, 0.2f), // Xanh lục đậm
                    SecondaryColor = new Color(0.5f, 0.35f, 0.2f), // Nâu gỗ
                    AnimalTypes = new[] { "fox", "owl", "redpanda" },
                    IsUnlockedByDefault = false,
                    MasksRequiredToUnlock = 2,
                    Create = true
                },
                new BiomeEntry
                {
                    Type = BiomeType.ZenValley,
                    Name = "Thung Lũng Phương Đông",
                    Description = "Khu vực đặc biệt dành cho các loài mang tính biểu tượng Á Đông. Có rừng tre, cổng đá, hồ nước tĩnh lặng.",
                    Difficulty = BiomeDifficulty.Hard,
                    PrimaryColor = new Color(0.4f, 0.7f, 0.4f), // Xanh tre trúc
                    SecondaryColor = new Color(0.8f, 0.2f, 0.2f), // Đỏ trang trí
                    AnimalTypes = new[] { "panda", "lion" },
                    IsUnlockedByDefault = false,
                    MasksRequiredToUnlock = 5,
                    Create = true
                },
                new BiomeEntry
                {
                    Type = BiomeType.WhimsicalWetlands,
                    Name = "Đầm Lầy Màu Nhiệm",
                    Description = "Khu vực có các loài động vật gắn liền với môi trường nước. Có các hồ nước, hoa sen lớn làm bục nhảy.",
                    Difficulty = BiomeDifficulty.Secret,
                    PrimaryColor = new Color(0.2f, 0.7f, 0.7f), // Xanh ngọc bích
                    SecondaryColor = new Color(0.7f, 0.4f, 0.8f), // Tím sen
                    AnimalTypes = new[] { "capybara", "axolotl" },
                    IsUnlockedByDefault = false,
                    MasksRequiredToUnlock = 8,
                    Create = true
                }
            };
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Biome Creator Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Tạo nhanh 4 Biomes cho Mask Collect:\n" +
                "• Peaceful Meadow (Easy) - Dog, Cat, Rabbit, Bird\n" +
                "• Mystic Forest (Medium) - Fox, Owl, Red Panda\n" +
                "• Zen Valley (Hard) - Panda, Lion\n" +
                "• Whimsical Wetlands (Secret) - Capybara, Axolotl",
                MessageType.Info);

            EditorGUILayout.Space(10);

            // Mask Database reference
            _maskDatabase = (MaskDatabase)EditorGUILayout.ObjectField(
                "Mask Database", _maskDatabase, typeof(MaskDatabase), false);

            EditorGUILayout.Space(5);

            // Biome list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            for (int i = 0; i < _biomeEntries.Count; i++)
            {
                DrawBiomeEntry(_biomeEntries[i]);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            // Action buttons
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Create Biome Assets", GUILayout.Height(35)))
            {
                CreateBiomeAssets();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);

            GUI.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            if (GUILayout.Button("Create All + Database + Update Masks", GUILayout.Height(35)))
            {
                CreateBiomeAssets();
                CreateOrUpdateDatabase();
                UpdateMaskBiomeAssignments();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawBiomeEntry(BiomeEntry entry)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            entry.Create = EditorGUILayout.Toggle(entry.Create, GUILayout.Width(20));
            
            // Color indicator
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(30, 20), entry.PrimaryColor);
            
            EditorGUILayout.LabelField(entry.Name, EditorStyles.boldLabel);
            entry.Difficulty = (BiomeDifficulty)EditorGUILayout.EnumPopup(entry.Difficulty, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Animals:", GUILayout.Width(60));
            EditorGUILayout.LabelField(string.Join(", ", entry.AnimalTypes), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Unlock:", GUILayout.Width(50));
            if (entry.IsUnlockedByDefault)
            {
                EditorGUILayout.LabelField("Default (Starting Area)", EditorStyles.miniLabel);
            }
            else
            {
                entry.MasksRequiredToUnlock = EditorGUILayout.IntField(entry.MasksRequiredToUnlock, GUILayout.Width(40));
                EditorGUILayout.LabelField("masks required", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            entry.PrimaryColor = EditorGUILayout.ColorField("Primary", entry.PrimaryColor, GUILayout.Width(200));
            entry.SecondaryColor = EditorGUILayout.ColorField("Secondary", entry.SecondaryColor, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void CreateBiomeAssets()
        {
            // Ensure output directory exists
            if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
            {
                CreateFolderPath(OUTPUT_PATH);
            }

            int created = 0;

            foreach (var entry in _biomeEntries)
            {
                if (!entry.Create) continue;

                string assetPath = $"{OUTPUT_PATH}/{entry.Type}.asset";

                // Create BiomeData asset
                BiomeData biomeData = ScriptableObject.CreateInstance<BiomeData>();

                SerializedObject so = new SerializedObject(biomeData);
                so.FindProperty("biomeId").stringValue = entry.Type.ToString().ToLower();
                so.FindProperty("biomeName").stringValue = entry.Name;
                so.FindProperty("description").stringValue = entry.Description;
                so.FindProperty("biomeType").enumValueIndex = (int)entry.Type;
                so.FindProperty("difficulty").enumValueIndex = (int)entry.Difficulty;
                so.FindProperty("primaryColor").colorValue = entry.PrimaryColor;
                so.FindProperty("secondaryColor").colorValue = entry.SecondaryColor;
                so.FindProperty("isUnlockedByDefault").boolValue = entry.IsUnlockedByDefault;
                so.FindProperty("masksRequiredToUnlock").intValue = entry.MasksRequiredToUnlock;

                // Set animal types
                var animalTypesProp = so.FindProperty("animalTypes");
                animalTypesProp.ClearArray();
                for (int i = 0; i < entry.AnimalTypes.Length; i++)
                {
                    animalTypesProp.InsertArrayElementAtIndex(i);
                    animalTypesProp.GetArrayElementAtIndex(i).stringValue = entry.AnimalTypes[i];
                }

                // Find and assign available masks
                if (_maskDatabase != null)
                {
                    var masksProp = so.FindProperty("availableMasks");
                    masksProp.ClearArray();
                    int maskIndex = 0;

                    foreach (var mask in _maskDatabase.AllMasks)
                    {
                        foreach (var animalType in entry.AnimalTypes)
                        {
                            if (mask.AssociatedAnimal.ToLower().Contains(animalType.ToLower()))
                            {
                                masksProp.InsertArrayElementAtIndex(maskIndex);
                                masksProp.GetArrayElementAtIndex(maskIndex).objectReferenceValue = mask;
                                maskIndex++;
                                break;
                            }
                        }
                    }
                }

                so.ApplyModifiedPropertiesWithoutUndo();

                AssetDatabase.CreateAsset(biomeData, assetPath);
                created++;

                Debug.Log($"[BiomeCreator] Created: {entry.Name}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Biome Creator",
                $"Created {created} biome assets.",
                "OK");
        }

        private void CreateOrUpdateDatabase()
        {
            string dataFolder = Path.GetDirectoryName(DATABASE_PATH);
            if (!AssetDatabase.IsValidFolder(dataFolder))
            {
                CreateFolderPath(dataFolder);
            }

            BiomeDatabase database = AssetDatabase.LoadAssetAtPath<BiomeDatabase>(DATABASE_PATH);

            if (database == null)
            {
                database = ScriptableObject.CreateInstance<BiomeDatabase>();
                AssetDatabase.CreateAsset(database, DATABASE_PATH);
            }

            string[] guids = AssetDatabase.FindAssets("t:BiomeData", new[] { OUTPUT_PATH });

            SerializedObject so = new SerializedObject(database);
            SerializedProperty allBiomesProp = so.FindProperty("allBiomes");
            allBiomesProp.ClearArray();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BiomeData biome = AssetDatabase.LoadAssetAtPath<BiomeData>(path);

                if (biome != null)
                {
                    allBiomesProp.InsertArrayElementAtIndex(allBiomesProp.arraySize);
                    allBiomesProp.GetArrayElementAtIndex(allBiomesProp.arraySize - 1).objectReferenceValue = biome;
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            Debug.Log($"[BiomeCreator] Database updated with {guids.Length} biomes");

            EditorGUIUtility.PingObject(database);
            Selection.activeObject = database;
        }

        private void UpdateMaskBiomeAssignments()
        {
            if (_maskDatabase == null)
            {
                Debug.LogWarning("[BiomeCreator] No MaskDatabase assigned!");
                return;
            }

            int updated = 0;

            foreach (var mask in _maskDatabase.AllMasks)
            {
                BiomeType targetBiome = GetBiomeForAnimal(mask.AssociatedAnimal);
                
                SerializedObject so = new SerializedObject(mask);
                so.FindProperty("homeBiome").enumValueIndex = (int)targetBiome;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(mask);
                updated++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[BiomeCreator] Updated {updated} masks with biome assignments");
        }

        private BiomeType GetBiomeForAnimal(string animalName)
        {
            string lower = animalName.ToLower();

            // Meadow animals
            if (lower.Contains("dog") || lower.Contains("cat") || 
                lower.Contains("rabbit") || lower.Contains("bird"))
            {
                return BiomeType.PeacefulMeadow;
            }

            // Forest animals
            if (lower.Contains("fox") || lower.Contains("owl") || 
                lower.Contains("redpanda") || lower.Contains("red panda"))
            {
                return BiomeType.MysticForest;
            }

            // Zen Valley animals
            if (lower.Contains("panda") || lower.Contains("lion"))
            {
                // Special case: redpanda goes to forest
                if (lower.Contains("red")) return BiomeType.MysticForest;
                return BiomeType.ZenValley;
            }

            // Wetlands animals
            if (lower.Contains("capybara") || lower.Contains("axolotl"))
            {
                return BiomeType.WhimsicalWetlands;
            }

            return BiomeType.PeacefulMeadow; // Default
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
