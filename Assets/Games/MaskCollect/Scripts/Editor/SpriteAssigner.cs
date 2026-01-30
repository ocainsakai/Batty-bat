using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace MaskCollect.Editor
{
    /// <summary>
    /// Editor tool to auto-assign sprites to UI prefabs from organized folders.
    /// </summary>
    public class SpriteAssigner : EditorWindow
    {
        private string spriteFolderPath = "Assets/Games/MaskCollect/Sprites";
        private Vector2 scrollPos;

        // Sprite references
        private Dictionary<string, Sprite> loadedSprites = new();
        private List<string> missingSprites = new();

        [MenuItem("Tools/MaskCollect/Sprite Assigner")]
        public static void ShowWindow()
        {
            GetWindow<SpriteAssigner>("Sprite Assigner");
        }

        private void OnGUI()
        {
            GUILayout.Label("MaskCollect Sprite Assigner", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            spriteFolderPath = EditorGUILayout.TextField("Sprites Folder", spriteFolderPath);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("🔍 Scan Sprites", GUILayout.Height(30)))
            {
                ScanSprites();
            }

            EditorGUILayout.Space(10);

            // Show found sprites
            if (loadedSprites.Count > 0)
            {
                EditorGUILayout.LabelField($"✅ Found Sprites: {loadedSprites.Count}", EditorStyles.boldLabel);
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(150));
                foreach (var kvp in loadedSprites)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(kvp.Key, kvp.Value, typeof(Sprite), false);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }

            // Show missing sprites
            if (missingSprites.Count > 0)
            {
                EditorGUILayout.Space(10);
                GUI.backgroundColor = Color.yellow;
                EditorGUILayout.LabelField($"⚠️ Missing Sprites: {missingSprites.Count}", EditorStyles.boldLabel);
                GUI.backgroundColor = Color.white;

                foreach (var missing in missingSprites)
                {
                    EditorGUILayout.LabelField($"  • {missing}", EditorStyles.miniLabel);
                }
            }

            EditorGUILayout.Space(20);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("✨ Auto-Assign to Prefabs", GUILayout.Height(40)))
            {
                AssignSpritesToPrefabs();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("📁 Create Folder Structure"))
            {
                CreateFolderStructure();
            }

            if (GUILayout.Button("📋 Copy Missing List to Clipboard"))
            {
                CopyMissingToClipboard();
            }
        }

        private void ScanSprites()
        {
            loadedSprites.Clear();
            missingSprites.Clear();

            // Expected sprites list
            var expectedSprites = GetExpectedSpritesList();

            // Scan folder
            if (Directory.Exists(spriteFolderPath))
            {
                string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolderPath });
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null)
                    {
                        string key = Path.GetFileNameWithoutExtension(path).ToLower();
                        loadedSprites[key] = sprite;
                    }
                }
            }

            // Check missing
            foreach (var expected in expectedSprites)
            {
                string key = expected.ToLower();
                if (!loadedSprites.ContainsKey(key))
                {
                    missingSprites.Add(expected);
                }
            }

            Debug.Log($"[SpriteAssigner] Found {loadedSprites.Count} sprites, missing {missingSprites.Count}");
        }

        private List<string> GetExpectedSpritesList()
        {
            return new List<string>
            {
                // ===== UI GENERAL =====
                "ui_logo",
                "ui_background_menu",
                "ui_background_loading",
                "ui_panel",
                "ui_panel_dark",
                "ui_panel_light",
                
                // ===== BUTTONS =====
                "btn_primary",
                "btn_secondary",
                "btn_close",
                "btn_back",
                "btn_pause",
                "btn_play",
                "btn_settings",
                "btn_home",
                
                // ===== ICONS =====
                "icon_mask",
                "icon_lock",
                "icon_unlock",
                "icon_star",
                "icon_star_empty",
                "icon_check",
                "icon_arrow_left",
                "icon_arrow_right",
                "icon_settings",
                "icon_sound_on",
                "icon_sound_off",
                "icon_music_on",
                "icon_music_off",
                
                // ===== JOYSTICK =====
                "joystick_background",
                "joystick_handle",
                
                // ===== BIOME ICONS =====
                "biome_meadow",
                "biome_forest",
                "biome_valley",
                "biome_wetlands",
                
                // ===== BIOME BACKGROUNDS =====
                "bg_meadow",
                "bg_forest",
                "bg_valley",
                "bg_wetlands",
                
                // ===== DECORATIONS =====
                "deco_grass_1",
                "deco_grass_2",
                "deco_flower_1",
                "deco_flower_2",
                "deco_tree_1",
                "deco_tree_2",
                "deco_rock_1",
                "deco_bush_1",
                
                // ===== ANIMALS =====
                "animal_dog",
                "animal_cat",
                "animal_rabbit",
                "animal_bird",
                "animal_fox",
                "animal_owl",
                "animal_redpanda",
                "animal_panda",
                "animal_lion",
                "animal_capybara",
                "animal_axolotl",
                
                // ===== MASKS (already have) =====
                "mask_dog",
                "mask_cat",
                "mask_rabbit",
                "mask_bird",
                "mask_fox",
                "mask_owl",
                "mask_redpanda",
                "mask_panda",
                "mask_lion",
                "mask_capybara",
                "mask_axolotl",
                
                // ===== MASK SILHOUETTES =====
                "mask_silhouette",
                
                // ===== VFX =====
                "particle_confetti",
                "particle_sparkle",
                "particle_star",
                
                // ===== POPUP =====
                "popup_rays",
                "popup_ribbon",
                "popup_frame_gold",
                "popup_frame_silver",
                
                // ===== RARITY BORDERS =====
                "border_common",
                "border_uncommon",
                "border_rare",
                "border_legendary",
                
                // ===== PLAYER =====
                "player_idle",
                "player_walk_1",
                "player_walk_2",
                
                // ===== WORLD MAP =====
                "worldmap_background",
                "worldmap_path",
                "worldmap_marker",
                "worldmap_marker_locked",
            };
        }

        private void AssignSpritesToPrefabs()
        {
            if (loadedSprites.Count == 0)
            {
                ScanSprites();
            }

            string prefabPath = "Assets/Games/MaskCollect/Prefabs/UI";
            if (!Directory.Exists(prefabPath))
            {
                EditorUtility.DisplayDialog("Error", "Prefab folder not found. Create prefabs first!", "OK");
                return;
            }

            int assigned = 0;

            // Find all prefabs
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabPath });
            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    bool modified = AssignSpritesToGameObject(instance);
                    
                    if (modified)
                    {
                        PrefabUtility.SaveAsPrefabAsset(instance, path);
                        assigned++;
                    }
                    
                    DestroyImmediate(instance);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Complete", $"Assigned sprites to {assigned} prefabs!", "OK");
        }

        private bool AssignSpritesToGameObject(GameObject obj)
        {
            bool modified = false;

            // Get all Image components
            Image[] images = obj.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                string name = image.gameObject.name.ToLower();
                Sprite sprite = FindSpriteForName(name);
                
                if (sprite != null && image.sprite == null)
                {
                    image.sprite = sprite;
                    modified = true;
                }
            }

            return modified;
        }

        private Sprite FindSpriteForName(string name)
        {
            // Direct match
            if (loadedSprites.TryGetValue(name, out Sprite sprite))
            {
                return sprite;
            }

            // Pattern matching
            var mappings = new Dictionary<string, string>
            {
                { "background", "ui_panel" },
                { "logo", "ui_logo" },
                { "pausebutton", "btn_pause" },
                { "playbutton", "btn_play" },
                { "backbutton", "btn_back" },
                { "homebutton", "btn_home" },
                { "settingsbutton", "btn_settings" },
                { "closebutton", "btn_close" },
                { "lockicon", "icon_lock" },
                { "maskimage", "icon_mask" },
                { "handle", "joystick_handle" },
                { "border", "border_common" },
            };

            foreach (var mapping in mappings)
            {
                if (name.Contains(mapping.Key))
                {
                    if (loadedSprites.TryGetValue(mapping.Value, out sprite))
                    {
                        return sprite;
                    }
                }
            }

            return null;
        }

        private void CreateFolderStructure()
        {
            string[] folders = new[]
            {
                $"{spriteFolderPath}/UI",
                $"{spriteFolderPath}/Buttons",
                $"{spriteFolderPath}/Icons",
                $"{spriteFolderPath}/Biomes",
                $"{spriteFolderPath}/Backgrounds",
                $"{spriteFolderPath}/Animals",
                $"{spriteFolderPath}/Masks",
                $"{spriteFolderPath}/Decorations",
                $"{spriteFolderPath}/VFX",
                $"{spriteFolderPath}/Player",
                $"{spriteFolderPath}/WorldMap",
            };

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", "Folder structure created!", "OK");
        }

        private void CopyMissingToClipboard()
        {
            if (missingSprites.Count == 0)
            {
                ScanSprites();
            }

            string text = "=== MISSING SPRITES FOR MASK COLLECT ===\n\n";
            
            // Group by category
            var categories = new Dictionary<string, List<string>>
            {
                { "🎨 UI General", new List<string>() },
                { "🔘 Buttons", new List<string>() },
                { "⭐ Icons", new List<string>() },
                { "🕹️ Joystick", new List<string>() },
                { "🌍 Biome Icons", new List<string>() },
                { "🖼️ Biome Backgrounds", new List<string>() },
                { "🌿 Decorations", new List<string>() },
                { "🐾 Animals", new List<string>() },
                { "🎭 Masks", new List<string>() },
                { "✨ VFX/Particles", new List<string>() },
                { "🎁 Popup Elements", new List<string>() },
                { "🏅 Rarity Borders", new List<string>() },
                { "🧑 Player", new List<string>() },
                { "🗺️ World Map", new List<string>() },
            };

            foreach (var missing in missingSprites)
            {
                if (missing.StartsWith("ui_")) categories["🎨 UI General"].Add(missing);
                else if (missing.StartsWith("btn_")) categories["🔘 Buttons"].Add(missing);
                else if (missing.StartsWith("icon_")) categories["⭐ Icons"].Add(missing);
                else if (missing.StartsWith("joystick_")) categories["🕹️ Joystick"].Add(missing);
                else if (missing.StartsWith("biome_")) categories["🌍 Biome Icons"].Add(missing);
                else if (missing.StartsWith("bg_")) categories["🖼️ Biome Backgrounds"].Add(missing);
                else if (missing.StartsWith("deco_")) categories["🌿 Decorations"].Add(missing);
                else if (missing.StartsWith("animal_")) categories["🐾 Animals"].Add(missing);
                else if (missing.StartsWith("mask_")) categories["🎭 Masks"].Add(missing);
                else if (missing.StartsWith("particle_")) categories["✨ VFX/Particles"].Add(missing);
                else if (missing.StartsWith("popup_")) categories["🎁 Popup Elements"].Add(missing);
                else if (missing.StartsWith("border_")) categories["🏅 Rarity Borders"].Add(missing);
                else if (missing.StartsWith("player_")) categories["🧑 Player"].Add(missing);
                else if (missing.StartsWith("worldmap_")) categories["🗺️ World Map"].Add(missing);
            }

            foreach (var cat in categories)
            {
                if (cat.Value.Count > 0)
                {
                    text += $"\n{cat.Key} ({cat.Value.Count})\n";
                    text += "─────────────────────────\n";
                    foreach (var item in cat.Value)
                    {
                        text += $"  □ {item}.png\n";
                    }
                }
            }

            text += "\n\n=== TỔNG CỘNG: " + missingSprites.Count + " sprites ===";

            GUIUtility.systemCopyBuffer = text;
            Debug.Log(text);
            EditorUtility.DisplayDialog("Copied!", "Missing sprites list copied to clipboard!", "OK");
        }
    }
}
