using UnityEngine;
using UnityEditor;
using MaskCollect.Gameplay;

namespace MaskCollect.Editor
{
    /// <summary>
    /// Editor tool to create biome environment prefabs for gameplay.
    /// </summary>
    public class BiomePrefabCreator : EditorWindow
    {
        [MenuItem("Tools/Mask Collect/Create Biome Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<BiomePrefabCreator>("Biome Prefab Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Biome Prefab Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This will create biome environment prefabs for gameplay.\n\n" +
                "Each biome prefab contains:\n" +
                "• Background layers (parallax)\n" +
                "• Ground/Platform areas\n" +
                "• Decoration spawn points\n" +
                "• Animal spawn points\n" +
                "• Ambient effects container",
                MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("Create All Biome Prefabs", GUILayout.Height(40)))
            {
                CreateAllBiomePrefabs();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Create Biome Folder Structure", GUILayout.Height(30)))
            {
                CreateFolderStructure();
            }
        }

        private void CreateFolderStructure()
        {
            string basePath = "Assets/Games/MaskCollect/Prefabs";
            
            // Create Biomes folder
            if (!AssetDatabase.IsValidFolder($"{basePath}/Biomes"))
            {
                AssetDatabase.CreateFolder(basePath, "Biomes");
            }

            // Create subfolder for each biome
            string[] biomes = { "PeacefulMeadow", "MysticForest", "ZenValley", "WhimsicalWetlands" };
            foreach (var biome in biomes)
            {
                if (!AssetDatabase.IsValidFolder($"{basePath}/Biomes/{biome}"))
                {
                    AssetDatabase.CreateFolder($"{basePath}/Biomes", biome);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[BiomePrefabCreator] Folder structure created!");
            EditorUtility.DisplayDialog("Success", "Biome folder structure created!", "OK");
        }

        private void CreateAllBiomePrefabs()
        {
            CreateFolderStructure();

            CreateBiomePrefab("PeacefulMeadow", new Color(0.7f, 0.9f, 0.5f)); // Light green
            CreateBiomePrefab("MysticForest", new Color(0.2f, 0.4f, 0.3f));   // Dark green
            CreateBiomePrefab("ZenValley", new Color(0.6f, 0.7f, 0.8f));      // Soft blue
            CreateBiomePrefab("WhimsicalWetlands", new Color(0.4f, 0.6f, 0.7f)); // Teal

            AssetDatabase.Refresh();
            Debug.Log("[BiomePrefabCreator] All biome prefabs created!");
            EditorUtility.DisplayDialog("Success", "All biome prefabs created!", "OK");
        }

        private void CreateBiomePrefab(string biomeName, Color tintColor)
        {
            string prefabPath = $"Assets/Games/MaskCollect/Prefabs/Biomes/{biomeName}";
            string fullPath = $"{prefabPath}/{biomeName}Environment.prefab";

            // Check if already exists
            if (AssetDatabase.LoadAssetAtPath<GameObject>(fullPath) != null)
            {
                Debug.Log($"[BiomePrefabCreator] {biomeName} prefab already exists, skipping.");
                return;
            }

            // Create root GameObject
            GameObject biomeRoot = new GameObject($"{biomeName}Environment");

            // === Background Layer ===
            GameObject backgroundLayer = new GameObject("BackgroundLayer");
            backgroundLayer.transform.SetParent(biomeRoot.transform);
            
            // Far background
            GameObject farBg = CreateSpriteObject("FarBackground", backgroundLayer.transform, -10f);
            farBg.AddComponent<ParallaxLayer>().scrollSpeed = 0.1f;
            
            // Mid background
            GameObject midBg = CreateSpriteObject("MidBackground", backgroundLayer.transform, -5f);
            midBg.AddComponent<ParallaxLayer>().scrollSpeed = 0.3f;
            
            // Near background
            GameObject nearBg = CreateSpriteObject("NearBackground", backgroundLayer.transform, -2f);
            nearBg.AddComponent<ParallaxLayer>().scrollSpeed = 0.5f;

            // === Ground Layer ===
            GameObject groundLayer = new GameObject("GroundLayer");
            groundLayer.transform.SetParent(biomeRoot.transform);
            
            GameObject ground = CreateSpriteObject("Ground", groundLayer.transform, 0f);
            ground.transform.localPosition = new Vector3(0, -3f, 0);
            ground.transform.localScale = new Vector3(20f, 1f, 1f);

            // === Decoration Layer ===
            GameObject decorLayer = new GameObject("DecorationLayer");
            decorLayer.transform.SetParent(biomeRoot.transform);
            
            // Add decoration spawn points
            for (int i = 0; i < 5; i++)
            {
                GameObject decorPoint = new GameObject($"DecorationPoint_{i}");
                decorPoint.transform.SetParent(decorLayer.transform);
                decorPoint.transform.localPosition = new Vector3(-8f + i * 4f, -2.5f, 0);
            }

            // === Animal Spawn Layer ===
            GameObject animalLayer = new GameObject("AnimalSpawnLayer");
            animalLayer.transform.SetParent(biomeRoot.transform);
            var spawner = animalLayer.AddComponent<AnimalSpawnArea>();
            
            // Add spawn points
            for (int i = 0; i < 3; i++)
            {
                GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
                spawnPoint.transform.SetParent(animalLayer.transform);
                spawnPoint.transform.localPosition = new Vector3(-5f + i * 5f, 0f, 0);
            }

            // === Effects Layer ===
            GameObject effectsLayer = new GameObject("EffectsLayer");
            effectsLayer.transform.SetParent(biomeRoot.transform);
            
            // Ambient particles container
            GameObject ambientFx = new GameObject("AmbientParticles");
            ambientFx.transform.SetParent(effectsLayer.transform);

            // === Bounds ===
            GameObject bounds = new GameObject("Bounds");
            bounds.transform.SetParent(biomeRoot.transform);
            var boxCollider = bounds.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(20f, 12f);

            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(biomeRoot, fullPath);
            DestroyImmediate(biomeRoot);

            Debug.Log($"[BiomePrefabCreator] Created {biomeName} prefab at {fullPath}");
        }

        private GameObject CreateSpriteObject(string name, Transform parent, float zPos)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = new Vector3(0, 0, zPos);
            
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = (int)(-zPos * 10);
            
            return obj;
        }
    }
}
