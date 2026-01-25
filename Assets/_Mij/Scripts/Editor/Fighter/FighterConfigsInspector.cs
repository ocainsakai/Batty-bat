using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(FighterConfigs))]
public class FighterConfigsInspector : Editor
{
    private TextAsset _sourceFile;
    private DefaultAsset _spriteFolder;
    private const string PREFS_KEY = "Mij_FighterConfigs_SourceFilePath";
    private const string PREFS_SPRITE_FOLDER_KEY = "Mij_FighterConfigs_SpriteFolderPath";

    private void OnEnable()
    {
        string path = EditorPrefs.GetString(PREFS_KEY, "");
        if (!string.IsNullOrEmpty(path))
        {
            _sourceFile = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }
        
        string spriteFolderPath = EditorPrefs.GetString(PREFS_SPRITE_FOLDER_KEY, "");
        if (!string.IsNullOrEmpty(spriteFolderPath))
        {
            _spriteFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(spriteFolderPath);
        }
    }

    public override void OnInspectorGUI()
    {
        FighterConfigs configs = (FighterConfigs)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configuration Source (Editor Only)", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        _sourceFile = (TextAsset)EditorGUILayout.ObjectField("Source File", _sourceFile, typeof(TextAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            if (_sourceFile != null)
            {
                EditorPrefs.SetString(PREFS_KEY, AssetDatabase.GetAssetPath(_sourceFile));
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Ping", GUILayout.Width(50)))
        {
            if (_sourceFile != null)
            {
                EditorGUIUtility.PingObject(_sourceFile);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Import/Export Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Import (Auto)"))
        {
            AutoImport(configs);
        }
        if (GUILayout.Button("Export (Auto)"))
        {
            AutoExport(configs);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Manual Import CSV"))
        {
            ImportCSV(configs);
        }
        if (GUILayout.Button("Manual Export CSV"))
        {
            ExportCSV(configs);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Manual Import JSON"))
        {
            ImportJSON(configs);
        }
        if (GUILayout.Button("Manual Export JSON"))
        {
            ExportJSON(configs);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sprite Matching", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        _spriteFolder = (DefaultAsset)EditorGUILayout.ObjectField("Sprite Folder", _spriteFolder, typeof(DefaultAsset), false);
        if (EditorGUI.EndChangeCheck())
        {
            if (_spriteFolder != null)
            {
                string folderPath = AssetDatabase.GetAssetPath(_spriteFolder);
                if (AssetDatabase.IsValidFolder(folderPath))
                {
                    EditorPrefs.SetString(PREFS_SPRITE_FOLDER_KEY, folderPath);
                }
                else
                {
                    Debug.LogWarning("Selected asset is not a folder!");
                    _spriteFolder = null;
                }
            }
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Ping Folder", GUILayout.Width(100)))
        {
            if (_spriteFolder != null)
            {
                EditorGUIUtility.PingObject(_spriteFolder);
            }
        }
        if (GUILayout.Button("Auto-Match Sprites"))
        {
            AutoMatchSprites(configs);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }

    private void AutoMatchSprites(FighterConfigs configs)
    {
        if (_spriteFolder == null)
        {
            Debug.LogError("No sprite folder assigned!");
            return;
        }

        string folderPath = AssetDatabase.GetAssetPath(_spriteFolder);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogError("Invalid folder path!");
            return;
        }

        if (configs.fighterDefinitions == null || configs.fighterDefinitions.Length == 0)
        {
            Debug.LogWarning("No fighter configs to match!");
            return;
        }

        int matchedCount = 0;
        foreach (var config in configs.fighterDefinitions)
        {
            if (config.fighterDefinitions == null) continue;
            
            string fighterName = config.fighterDefinitions.name;
            
            // Try to find a sprite with matching name
            string[] guids = AssetDatabase.FindAssets($"t:Sprite {fighterName}", new[] { folderPath });
            
            if (guids.Length == 0)
            {
                // Try finding by texture name instead
                guids = AssetDatabase.FindAssets($"t:Texture2D {fighterName}", new[] { folderPath });
            }
            
            if (guids.Length > 0)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                
                if (sprite == null)
                {
                    // Load as texture and get sprite from it
                    var allSprites = AssetDatabase.LoadAllAssetsAtPath(spritePath);
                    foreach (var obj in allSprites)
                    {
                        if (obj is Sprite s)
                        {
                            sprite = s;
                            break;
                        }
                    }
                }
                
                if (sprite != null)
                {
                    config.sprite = sprite;
                    matchedCount++;
                    Debug.Log($"Matched sprite for {fighterName}: {spritePath}");
                }
            }
            else
            {
                Debug.LogWarning($"No sprite found for: {fighterName}");
            }
        }

        EditorUtility.SetDirty(configs);
        AssetDatabase.SaveAssets();
        Debug.Log($"Auto-matched {matchedCount}/{configs.fighterDefinitions.Length} sprites.");
    }

    private void AutoImport(FighterConfigs configs)
    {
        if (_sourceFile == null)
        {
            Debug.LogError("No source file assigned!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(_sourceFile);
        if (path.EndsWith(".csv"))
        {
            ProcessCSV(configs, File.ReadAllLines(path));
        }
        else if (path.EndsWith(".json"))
        {
            ProcessJSON(configs, File.ReadAllText(path));
        }
        else
        {
            Debug.LogError("Unsupported file format! Use CSV or JSON.");
        }
    }

    private void AutoExport(FighterConfigs configs)
    {
        if (_sourceFile == null)
        {
            Debug.LogError("No source file assigned for Auto Export!");
            return;
        }

        string path = AssetDatabase.GetAssetPath(_sourceFile);
        if (path.EndsWith(".csv"))
        {
            WriteCSV(configs, path);
        }
        else if (path.EndsWith(".json"))
        {
            WriteJSON(configs, path);
        }
    }

    private void ImportCSV(FighterConfigs configs)
    {
        string path = EditorUtility.OpenFilePanel("Import CSV", "", "csv");
        if (string.IsNullOrEmpty(path)) return;
        ProcessCSV(configs, File.ReadAllLines(path));
    }

    private void ProcessCSV(FighterConfigs configs, string[] lines)
    {
        if (lines.Length <= 1) return;

        List<FighterConfig> importedConfigs = new List<FighterConfig>();
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length < 10) continue;

            string name = values[0].Trim();
            FighterClass fClass = (FighterClass)Enum.Parse(typeof(FighterClass), values[1].Trim());
            FighterAttribute fAttr = (FighterAttribute)Enum.Parse(typeof(FighterAttribute), values[2].Trim());

            FighterConfig config = new FighterConfig();
            config.fighterDefinitions = GetOrCreateFighterDefinition(name, fClass, fAttr);
            config.AttackRange = float.Parse(values[3]);
            config.SightRange = float.Parse(values[4]);
            config.PatrolRadius = float.Parse(values[5]);
            config.MoveSpeed = float.Parse(values[6]);
            config.AttackSpeed = float.Parse(values[7]);
            config.MaxHealth = int.Parse(values[8]);
            config.Damage = int.Parse(values[9]);

            importedConfigs.Add(config);
        }

        configs.fighterDefinitions = importedConfigs.ToArray();
        EditorUtility.SetDirty(configs);
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported {importedConfigs.Count} configs from CSV.");
    }

    private void ExportCSV(FighterConfigs configs)
    {
        string path = EditorUtility.SaveFilePanel("Export CSV", "", "FighterConfigs", "csv");
        if (string.IsNullOrEmpty(path)) return;
        WriteCSV(configs, path);
    }

    private void WriteCSV(FighterConfigs configs, string path)
    {
        List<string> lines = new List<string>();
        lines.Add("FighterName,Class,Attribute,AttackRange,SightRange,PatrolRadius,MoveSpeed,AttackSpeed,MaxHealth,Damage");

        if (configs.fighterDefinitions != null)
        {
            foreach (var config in configs.fighterDefinitions)
            {
                if (config.fighterDefinitions == null) continue;
                string name = config.fighterDefinitions.name;
                string fClass = config.fighterDefinitions.fighterClass.ToString();
                string fAttr = config.fighterDefinitions.fighterAttribute.ToString();
                
                lines.Add($"{name},{fClass},{fAttr},{config.AttackRange},{config.SightRange},{config.PatrolRadius},{config.MoveSpeed},{config.AttackSpeed},{config.MaxHealth},{config.Damage}");
            }
        }

        File.WriteAllLines(path, lines);
        Debug.Log($"Exported {lines.Count - 1} configs to CSV at {path}.");
    }

    private void ImportJSON(FighterConfigs configs)
    {
        string path = EditorUtility.OpenFilePanel("Import JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;
        ProcessJSON(configs, File.ReadAllText(path));
    }

    private void ProcessJSON(FighterConfigs configs, string json)
    {
        FighterConfigsData data = JsonUtility.FromJson<FighterConfigsData>(json);
        if (data == null || data.items == null) return;

        List<FighterConfig> importedConfigs = new List<FighterConfig>();
        foreach (var item in data.items)
        {
            FighterConfig config = new FighterConfig();
            config.fighterDefinitions = GetOrCreateFighterDefinition(item.FighterName, item.Class, item.Attribute);
            config.AttackRange = item.AttackRange;
            config.SightRange = item.SightRange;
            config.PatrolRadius = item.PatrolRadius;
            config.MoveSpeed = item.MoveSpeed;
            config.AttackSpeed = item.AttackSpeed;
            config.MaxHealth = item.MaxHealth;
            config.Damage = item.Damage;
            importedConfigs.Add(config);
        }

        configs.fighterDefinitions = importedConfigs.ToArray();
        EditorUtility.SetDirty(configs);
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported {importedConfigs.Count} configs from JSON.");
    }

    private void ExportJSON(FighterConfigs configs)
    {
        string path = EditorUtility.SaveFilePanel("Export JSON", "", "FighterConfigs", "json");
        if (string.IsNullOrEmpty(path)) return;
        WriteJSON(configs, path);
    }

    private void WriteJSON(FighterConfigs configs, string path)
    {
        FighterConfigsData data = new FighterConfigsData();
        data.items = new List<FighterConfigItem>();

        if (configs.fighterDefinitions != null)
        {
            foreach (var config in configs.fighterDefinitions)
            {
                if (config.fighterDefinitions == null) continue;
                data.items.Add(new FighterConfigItem
                {
                    FighterName = config.fighterDefinitions.name,
                    Class = config.fighterDefinitions.fighterClass,
                    Attribute = config.fighterDefinitions.fighterAttribute,
                    AttackRange = config.AttackRange,
                    SightRange = config.SightRange,
                    PatrolRadius = config.PatrolRadius,
                    MoveSpeed = config.MoveSpeed,
                    AttackSpeed = config.AttackSpeed,
                    MaxHealth = config.MaxHealth,
                    Damage = config.Damage
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Exported {data.items.Count} configs to JSON at {path}.");
    }

    private FighterDefinition GetOrCreateFighterDefinition(string name, FighterClass fighterClass, FighterAttribute attribute)
    {
        string folderPath = "Assets/_Mij/Data/Fighters";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/_Mij/Data", "Fighters");
        }

        string assetPath = $"{folderPath}/{name}.asset";
        FighterDefinition definition = AssetDatabase.LoadAssetAtPath<FighterDefinition>(assetPath);

        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<FighterDefinition>();
            definition.fighterClass = fighterClass;
            definition.fighterAttribute = attribute;
            AssetDatabase.CreateAsset(definition, assetPath);
            Debug.Log($"Created new FighterDefinition asset: {assetPath}");
        }
        else
        {
            definition.fighterClass = fighterClass;
            definition.fighterAttribute = attribute;
            EditorUtility.SetDirty(definition);
        }

        return definition;
    }

    [Serializable]
    private class FighterConfigsData
    {
        public List<FighterConfigItem> items;
    }

    [Serializable]
    private class FighterConfigItem
    {
        public string FighterName;
        public FighterClass Class;
        public FighterAttribute Attribute;
        public float AttackRange;
        public float SightRange;
        public float PatrolRadius;
        public float MoveSpeed;
        public float AttackSpeed;
        public int MaxHealth;
        public int Damage;
    }
}
