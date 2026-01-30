using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace MaskCollect.Editor
{
    /// <summary>
    /// Editor tool to quickly create UI prefabs for MaskCollect game.
    /// </summary>
    public class UIPrefabCreator : EditorWindow
    {
        private string prefabSavePath = "Assets/Games/MaskCollect/Prefabs/UI";

        [MenuItem("Tools/MaskCollect/Create UI Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<UIPrefabCreator>("UI Prefab Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("MaskCollect UI Prefab Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            prefabSavePath = EditorGUILayout.TextField("Save Path", prefabSavePath);
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Screen Prefabs", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Loading Screen")) CreateLoadingScreen();
            if (GUILayout.Button("Create Main Menu Screen")) CreateMainMenuScreen();
            if (GUILayout.Button("Create World Map Screen")) CreateWorldMapScreen();
            if (GUILayout.Button("Create Gameplay HUD")) CreateGameplayHUD();
            if (GUILayout.Button("Create Pause Screen")) CreatePauseScreen();
            if (GUILayout.Button("Create Collection Book")) CreateCollectionBook();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Component Prefabs", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Mask Slot")) CreateMaskSlot();
            if (GUILayout.Button("Create Biome Button")) CreateBiomeButton();
            if (GUILayout.Button("Create Floating Joystick")) CreateFloatingJoystick();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Popup Prefabs", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Reward Popup")) CreateRewardPopup();
            if (GUILayout.Button("Create Biome Unlock Popup")) CreateBiomeUnlockPopup();

            EditorGUILayout.Space(20);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🎉 Create ALL Prefabs", GUILayout.Height(40)))
            {
                CreateAllPrefabs();
            }
            GUI.backgroundColor = Color.white;
        }

        private void EnsurePath()
        {
            if (!AssetDatabase.IsValidFolder(prefabSavePath))
            {
                System.IO.Directory.CreateDirectory(prefabSavePath);
                AssetDatabase.Refresh();
            }
        }

        #region Screen Creators

        private void CreateLoadingScreen()
        {
            EnsurePath();

            var root = CreateUIRoot("LoadingScreen");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.LoadingScreen>();

            // Background
            var bg = CreateImage(root.transform, "Background", Color.black);
            StretchToFill(bg.GetComponent<RectTransform>());

            // Logo
            var logo = CreateImage(root.transform, "Logo", Color.white);
            var logoRect = logo.GetComponent<RectTransform>();
            logoRect.anchoredPosition = new Vector2(0, 100);
            logoRect.sizeDelta = new Vector2(300, 300);

            // Progress Bar Background
            var progressBg = CreateImage(root.transform, "ProgressBarBg", new Color(0.2f, 0.2f, 0.2f));
            var progressBgRect = progressBg.GetComponent<RectTransform>();
            progressBgRect.anchoredPosition = new Vector2(0, -100);
            progressBgRect.sizeDelta = new Vector2(400, 20);

            // Progress Bar Fill (as Slider)
            var slider = new GameObject("ProgressBar");
            slider.transform.SetParent(root.transform);
            var sliderComp = slider.AddComponent<Slider>();
            var sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.anchoredPosition = new Vector2(0, -100);
            sliderRect.sizeDelta = new Vector2(400, 20);

            // Quote Text
            var quote = CreateText(root.transform, "QuoteText", "\"Loading...\" - Animal");
            var quoteRect = quote.GetComponent<RectTransform>();
            quoteRect.anchoredPosition = new Vector2(0, -200);
            quoteRect.sizeDelta = new Vector2(600, 60);

            SavePrefab(root, "LoadingScreen");
        }

        private void CreateMainMenuScreen()
        {
            EnsurePath();

            var root = CreateUIRoot("MainMenuScreen");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.MainMenuScreen>();

            // Background
            var bg = CreateImage(root.transform, "Background", new Color(0.2f, 0.3f, 0.4f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Title
            var title = CreateText(root.transform, "Title", "MASK COLLECT");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 200);
            titleRect.sizeDelta = new Vector2(500, 100);
            title.GetComponent<TextMeshProUGUI>().fontSize = 72;

            // Buttons Container
            var buttonsContainer = new GameObject("Buttons");
            buttonsContainer.transform.SetParent(root.transform);
            var containerRect = buttonsContainer.AddComponent<RectTransform>();
            containerRect.anchoredPosition = Vector2.zero;
            var layout = buttonsContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;

            // Play Button
            CreateButton(buttonsContainer.transform, "PlayButton", "Bắt Đầu");
            CreateButton(buttonsContainer.transform, "ContinueButton", "Tiếp Tục");
            CreateButton(buttonsContainer.transform, "CollectionButton", "Bộ Sưu Tập");
            CreateButton(buttonsContainer.transform, "SettingsButton", "Cài Đặt");

            SavePrefab(root, "MainMenuScreen");
        }

        private void CreateWorldMapScreen()
        {
            EnsurePath();

            var root = CreateUIRoot("WorldMapScreen");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.WorldMapScreen>();

            // Background
            var bg = CreateImage(root.transform, "Background", new Color(0.3f, 0.5f, 0.3f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Title
            var title = CreateText(root.transform, "Title", "Bản Đồ Thế Giới");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -80);

            // Biomes Container
            var biomesContainer = new GameObject("BiomesContainer");
            biomesContainer.transform.SetParent(root.transform);
            var containerRect = biomesContainer.AddComponent<RectTransform>();
            StretchToFill(containerRect);
            containerRect.offsetMin = new Vector2(50, 150);
            containerRect.offsetMax = new Vector2(-50, -150);
            var grid = biomesContainer.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(200, 200);
            grid.spacing = new Vector2(30, 30);
            grid.childAlignment = TextAnchor.MiddleCenter;

            // Back Button
            var backBtn = CreateButton(root.transform, "BackButton", "← Quay Lại");
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 1);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.anchoredPosition = new Vector2(100, -50);

            // Info Panel (hidden by default)
            var infoPanel = CreatePanel(root.transform, "InfoPanel", new Color(0, 0, 0, 0.9f));
            infoPanel.SetActive(false);

            SavePrefab(root, "WorldMapScreen");
        }

        private void CreateGameplayHUD()
        {
            EnsurePath();

            var root = CreateUIRoot("GameplayHUD");
            root.AddComponent<UI.GameplayHUD>();

            // Top Bar
            var topBar = CreateImage(root.transform, "TopBar", new Color(0, 0, 0, 0.5f));
            var topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.anchoredPosition = Vector2.zero;
            topRect.sizeDelta = new Vector2(0, 80);

            // Pause Button
            var pauseBtn = CreateButton(topBar.transform, "PauseButton", "II");
            var pauseRect = pauseBtn.GetComponent<RectTransform>();
            pauseRect.anchorMin = new Vector2(0, 0.5f);
            pauseRect.anchorMax = new Vector2(0, 0.5f);
            pauseRect.anchoredPosition = new Vector2(50, 0);
            pauseRect.sizeDelta = new Vector2(60, 60);

            // Mask Counter
            var counter = CreateText(topBar.transform, "MaskCountText", "0/11");
            var counterRect = counter.GetComponent<RectTransform>();
            counterRect.anchorMin = new Vector2(1, 0.5f);
            counterRect.anchorMax = new Vector2(1, 0.5f);
            counterRect.anchoredPosition = new Vector2(-100, 0);

            // Biome Name
            var biomeName = CreateText(topBar.transform, "BiomeNameText", "Peaceful Meadow");
            var biomeRect = biomeName.GetComponent<RectTransform>();
            biomeRect.anchorMin = new Vector2(0.5f, 0.5f);
            biomeRect.anchorMax = new Vector2(0.5f, 0.5f);
            biomeRect.anchoredPosition = Vector2.zero;

            // Notification Banner
            var notification = CreateImage(root.transform, "NotificationBanner", new Color(0.2f, 0.6f, 0.2f, 0.9f));
            var notifRect = notification.GetComponent<RectTransform>();
            notifRect.anchorMin = new Vector2(0.5f, 1);
            notifRect.anchorMax = new Vector2(0.5f, 1);
            notifRect.anchoredPosition = new Vector2(0, -100);
            notifRect.sizeDelta = new Vector2(400, 50);
            notification.SetActive(false);

            SavePrefab(root, "GameplayHUD");
        }

        private void CreatePauseScreen()
        {
            EnsurePath();

            var root = CreateUIRoot("PauseScreen");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.PauseScreen>();

            // Dim Background
            var bg = CreateImage(root.transform, "Background", new Color(0, 0, 0, 0.7f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Panel
            var panel = CreatePanel(root.transform, "Panel", new Color(0.2f, 0.2f, 0.3f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 500);

            // Title
            var title = CreateText(panel.transform, "Title", "TẠM DỪNG");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 180);

            // Buttons
            var buttonsContainer = new GameObject("Buttons");
            buttonsContainer.transform.SetParent(panel.transform);
            var containerRect = buttonsContainer.AddComponent<RectTransform>();
            containerRect.anchoredPosition = new Vector2(0, -20);
            var layout = buttonsContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleCenter;

            CreateButton(buttonsContainer.transform, "ResumeButton", "Tiếp Tục");
            CreateButton(buttonsContainer.transform, "SettingsButton", "Cài Đặt");
            CreateButton(buttonsContainer.transform, "HomeButton", "Về Menu");
            CreateButton(buttonsContainer.transform, "QuitButton", "Thoát Game");

            SavePrefab(root, "PauseScreen");
        }

        private void CreateCollectionBook()
        {
            EnsurePath();

            var root = CreateUIRoot("CollectionBookScreen");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.CollectionBookScreen>();

            // Background
            var bg = CreateImage(root.transform, "Background", new Color(0.15f, 0.1f, 0.2f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Title
            var title = CreateText(root.transform, "Title", "BỘ SƯU TẬP");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1);
            titleRect.anchorMax = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -60);

            // Progress
            var progress = CreateText(root.transform, "ProgressText", "0/11 Mặt nạ");
            var progressRect = progress.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.5f, 1);
            progressRect.anchorMax = new Vector2(0.5f, 1);
            progressRect.anchoredPosition = new Vector2(0, -100);

            // Grid Container with Scroll
            var scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(root.transform);
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollRectTransform = scrollView.GetComponent<RectTransform>();
            StretchToFill(scrollRectTransform);
            scrollRectTransform.offsetMin = new Vector2(30, 100);
            scrollRectTransform.offsetMax = new Vector2(-30, -130);

            var viewport = CreateImage(scrollView.transform, "Viewport", Color.clear);
            viewport.AddComponent<Mask>();
            StretchToFill(viewport.GetComponent<RectTransform>());

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 150);
            grid.spacing = new Vector2(15, 15);
            grid.childAlignment = TextAnchor.UpperCenter;
            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;
            scrollRect.viewport = viewport.GetComponent<RectTransform>();

            // Back Button
            var backBtn = CreateButton(root.transform, "BackButton", "← Quay Lại");
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 1);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.anchoredPosition = new Vector2(100, -50);

            SavePrefab(root, "CollectionBookScreen");
        }

        #endregion

        #region Component Creators

        private void CreateMaskSlot()
        {
            EnsurePath();

            var root = new GameObject("MaskSlot");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(120, 150);
            root.AddComponent<UI.CollectionSlotUI>();

            // Background/Border
            var border = CreateImage(root.transform, "Border", new Color(0.5f, 0.5f, 0.5f));
            StretchToFill(border.GetComponent<RectTransform>());

            // Mask Image
            var maskImg = CreateImage(root.transform, "MaskImage", Color.white);
            var maskRect = maskImg.GetComponent<RectTransform>();
            maskRect.anchoredPosition = new Vector2(0, 10);
            maskRect.sizeDelta = new Vector2(80, 80);

            // Lock Icon
            var lockIcon = CreateImage(root.transform, "LockIcon", Color.gray);
            var lockRect = lockIcon.GetComponent<RectTransform>();
            lockRect.sizeDelta = new Vector2(40, 40);

            // Name Text
            var nameText = CreateText(root.transform, "NameText", "???");
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0);
            nameRect.anchoredPosition = new Vector2(0, 20);
            nameRect.sizeDelta = new Vector2(0, 30);

            // Button
            root.AddComponent<Button>();

            SavePrefab(root, "MaskSlot");
        }

        private void CreateBiomeButton()
        {
            EnsurePath();

            var root = new GameObject("BiomeButton");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(200, 200);
            root.AddComponent<UI.BiomeButtonUI>();
            root.AddComponent<Button>();

            // Background
            var bg = CreateImage(root.transform, "Background", new Color(0.3f, 0.4f, 0.3f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Border
            var border = CreateImage(root.transform, "Border", Color.white);
            StretchToFill(border.GetComponent<RectTransform>());
            border.GetComponent<Image>().type = Image.Type.Sliced;

            // Icon
            var icon = CreateImage(root.transform, "BiomeIcon", Color.white);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(0, 20);
            iconRect.sizeDelta = new Vector2(100, 100);

            // Name
            var nameText = CreateText(root.transform, "NameText", "Biome Name");
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0);
            nameRect.anchoredPosition = new Vector2(0, 30);
            nameRect.sizeDelta = new Vector2(0, 40);

            // Lock Icon
            var lockIcon = CreateImage(root.transform, "LockIcon", new Color(0.2f, 0.2f, 0.2f, 0.8f));
            var lockRect = lockIcon.GetComponent<RectTransform>();
            lockRect.sizeDelta = new Vector2(50, 50);

            SavePrefab(root, "BiomeButton");
        }

        private void CreateFloatingJoystick()
        {
            EnsurePath();

            var root = new GameObject("FloatingJoystick");
            var rootRect = root.AddComponent<RectTransform>();
            StretchToFill(rootRect);
            root.AddComponent<CanvasGroup>();
            root.AddComponent<Gameplay.FloatingJoystick>();

            // Background (outer circle)
            var bg = CreateImage(root.transform, "Background", new Color(1, 1, 1, 0.3f));
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(150, 150);

            // Handle (inner circle)
            var handle = CreateImage(bg.transform, "Handle", new Color(1, 1, 1, 0.8f));
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(60, 60);

            SavePrefab(root, "FloatingJoystick");
        }

        #endregion

        #region Popup Creators

        private void CreateRewardPopup()
        {
            EnsurePath();

            var root = CreateUIRoot("RewardPopup");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.RewardPopup>();

            // Dim Background
            var bg = CreateImage(root.transform, "Background", new Color(0, 0, 0, 0.7f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Panel
            var panel = CreatePanel(root.transform, "Panel", new Color(0.9f, 0.85f, 0.7f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(400, 500);

            // Title
            var title = CreateText(panel.transform, "Title", "MẶT NẠ MỚI!");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 180);
            title.GetComponent<TextMeshProUGUI>().color = Color.black;

            // Mask Image
            var maskImg = CreateImage(panel.transform, "MaskImage", Color.white);
            var maskRect = maskImg.GetComponent<RectTransform>();
            maskRect.sizeDelta = new Vector2(200, 200);

            // Mask Name
            var maskName = CreateText(panel.transform, "MaskName", "Mask Name");
            var nameRect = maskName.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -130);
            maskName.GetComponent<TextMeshProUGUI>().color = Color.black;

            // Continue Button
            var continueBtn = CreateButton(panel.transform, "ContinueButton", "Tuyệt vời!");
            var btnRect = continueBtn.GetComponent<RectTransform>();
            btnRect.anchoredPosition = new Vector2(0, -200);

            SavePrefab(root, "RewardPopup");
        }

        private void CreateBiomeUnlockPopup()
        {
            EnsurePath();

            var root = CreateUIRoot("BiomeUnlockPopup");
            root.AddComponent<CanvasGroup>();
            root.AddComponent<UI.BiomeUnlockPopup>();

            // Dim Background
            var bg = CreateImage(root.transform, "Background", new Color(0, 0, 0, 0.8f));
            StretchToFill(bg.GetComponent<RectTransform>());

            // Panel
            var panel = CreatePanel(root.transform, "Panel", new Color(0.2f, 0.4f, 0.3f));
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(450, 550);

            // Title
            var title = CreateText(panel.transform, "Title", "VÙNG ĐẤT MỚI!");
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 220);

            // Biome Image
            var biomeImg = CreateImage(panel.transform, "BiomeImage", Color.white);
            var biomeRect = biomeImg.GetComponent<RectTransform>();
            biomeRect.anchoredPosition = new Vector2(0, 50);
            biomeRect.sizeDelta = new Vector2(300, 150);

            // Biome Name
            var biomeName = CreateText(panel.transform, "BiomeName", "Mystic Forest");
            var nameRect = biomeName.GetComponent<RectTransform>();
            nameRect.anchoredPosition = new Vector2(0, -60);

            // Description
            var desc = CreateText(panel.transform, "Description", "Một vùng đất mới chờ bạn khám phá!");
            var descRect = desc.GetComponent<RectTransform>();
            descRect.anchoredPosition = new Vector2(0, -110);
            descRect.sizeDelta = new Vector2(380, 60);

            // Buttons
            var goBtn = CreateButton(panel.transform, "GoButton", "Khám Phá Ngay!");
            var goRect = goBtn.GetComponent<RectTransform>();
            goRect.anchoredPosition = new Vector2(0, -180);

            var laterBtn = CreateButton(panel.transform, "LaterButton", "Để Sau");
            var laterRect = laterBtn.GetComponent<RectTransform>();
            laterRect.anchoredPosition = new Vector2(0, -240);

            SavePrefab(root, "BiomeUnlockPopup");
        }

        #endregion

        #region Helpers

        private GameObject CreateUIRoot(string name)
        {
            var root = new GameObject(name);
            var rect = root.AddComponent<RectTransform>();
            StretchToFill(rect);
            return root;
        }

        private GameObject CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private GameObject CreateText(Transform parent, string name, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(300, 50);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            return go;
        }

        private GameObject CreateButton(Transform parent, string name, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(250, 60);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.3f, 0.5f, 0.7f);
            go.AddComponent<Button>();

            var textGo = CreateText(go.transform, "Text", text);
            StretchToFill(textGo.GetComponent<RectTransform>());

            return go;
        }

        private GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var go = CreateImage(parent, name, color);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 400);
            return go;
        }

        private void StretchToFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void SavePrefab(GameObject obj, string name)
        {
            string path = $"{prefabSavePath}/{name}.prefab";
            
            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);
            Debug.Log($"✅ Created prefab: {path}");
        }

        private void CreateAllPrefabs()
        {
            CreateLoadingScreen();
            CreateMainMenuScreen();
            CreateWorldMapScreen();
            CreateGameplayHUD();
            CreatePauseScreen();
            CreateCollectionBook();
            CreateMaskSlot();
            CreateBiomeButton();
            CreateFloatingJoystick();
            CreateRewardPopup();
            CreateBiomeUnlockPopup();

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Hoàn tất!", "Đã tạo tất cả UI Prefabs!", "OK");
        }

        #endregion
    }
}
