using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// World map screen for biome selection.
    /// Shows all biomes with lock/unlock status.
    /// </summary>
    public class WorldMapScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BiomeDatabase biomeDatabase;
        [SerializeField] private BiomeManager biomeManager;

        [Header("UI Elements")]
        [SerializeField] private Transform biomesContainer;
        [SerializeField] private GameObject biomeButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Biome Info Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private Image biomeImage;
        [SerializeField] private TextMeshProUGUI biomeNameText;
        [SerializeField] private TextMeshProUGUI biomeDescText;
        [SerializeField] private TextMeshProUGUI biomeAnimalsText;
        [SerializeField] private Button enterBiomeButton;
        [SerializeField] private Slider unlockProgressBar;
        [SerializeField] private TextMeshProUGUI unlockRequirementText;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.5f;

        private List<BiomeButtonUI> _biomeButtons = new();
        private BiomeData _selectedBiome;
        private GameFlowController _gameFlow;

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (enterBiomeButton != null)
            {
                enterBiomeButton.onClick.AddListener(OnEnterBiomeClicked);
            }
        }

        private void OnEnable()
        {
            _gameFlow = GameFlowController.Instance;

            if (biomeManager == null)
            {
                biomeManager = BiomeManager.Instance;
            }

            InitializeBiomeButtons();
            UpdateProgress();
            PlayEnterAnimation();

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }

        private void InitializeBiomeButtons()
        {
            // Clear existing buttons
            foreach (var button in _biomeButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            _biomeButtons.Clear();

            if (biomeDatabase == null || biomesContainer == null || biomeButtonPrefab == null)
            {
                return;
            }

            // Create buttons for each biome
            foreach (var biome in biomeDatabase.AllBiomes)
            {
                if (biome == null) continue;

                var buttonObj = Instantiate(biomeButtonPrefab, biomesContainer);
                var biomeButton = buttonObj.GetComponent<BiomeButtonUI>();

                if (biomeButton != null)
                {
                    bool isUnlocked = biomeManager != null && biomeManager.IsBiomeUnlocked(biome.Type);
                    float progress = biomeManager != null ? biomeManager.GetUnlockProgress(biome) : 0f;

                    biomeButton.Initialize(biome, isUnlocked, progress);
                    biomeButton.OnClicked += OnBiomeButtonClicked;
                    _biomeButtons.Add(biomeButton);
                }
            }
        }

        private void UpdateProgress()
        {
            if (progressText == null || biomeManager == null) return;

            int unlocked = biomeManager.UnlockedBiomes.Count;
            int total = biomeDatabase != null ? biomeDatabase.TotalBiomeCount : 0;

            progressText.text = $"Areas Unlocked: {unlocked}/{total}";
        }

        private void PlayEnterAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration);
            }

            // Animate biome buttons
            AnimateBiomeButtons().Forget();
        }

        private async UniTaskVoid AnimateBiomeButtons()
        {
            await UniTask.WaitForSeconds(0.2f);

            foreach (var button in _biomeButtons)
            {
                if (button != null)
                {
                    button.transform.localScale = Vector3.zero;
                    button.transform.DOScale(Vector3.one, 0.3f)
                        .SetEase(Ease.OutBack);
                    
                    await UniTask.WaitForSeconds(0.1f);
                }
            }
        }

        private void OnBiomeButtonClicked(BiomeData biome)
        {
            _selectedBiome = biome;
            ShowBiomeInfo(biome);
        }

        private void ShowBiomeInfo(BiomeData biome)
        {
            if (infoPanel == null || biome == null) return;

            infoPanel.SetActive(true);

            // Animate panel in
            infoPanel.transform.localScale = Vector3.one * 0.8f;
            infoPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

            // Update info
            if (biomeNameText != null)
            {
                biomeNameText.text = biome.BiomeName;
            }

            if (biomeDescText != null)
            {
                biomeDescText.text = biome.Description;
            }

            if (biomeAnimalsText != null && biome.AnimalTypes != null)
            {
                biomeAnimalsText.text = "Animals: " + string.Join(", ", biome.AnimalTypes);
            }

            if (biomeImage != null && biome.BackgroundSprite != null)
            {
                biomeImage.sprite = biome.BackgroundSprite;
            }

            // Check unlock status
            bool isUnlocked = biomeManager != null && biomeManager.IsBiomeUnlocked(biome.Type);

            if (enterBiomeButton != null)
            {
                enterBiomeButton.interactable = isUnlocked;
                var buttonText = enterBiomeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isUnlocked ? "Explore" : "Locked";
                }
            }

            // Show unlock progress if locked
            if (unlockProgressBar != null)
            {
                unlockProgressBar.gameObject.SetActive(!isUnlocked);
                if (!isUnlocked && biomeManager != null)
                {
                    unlockProgressBar.value = biomeManager.GetUnlockProgress(biome);
                }
            }

            if (unlockRequirementText != null)
            {
                unlockRequirementText.gameObject.SetActive(!isUnlocked);
                if (!isUnlocked)
                {
                    unlockRequirementText.text = $"Collect {biome.MasksRequiredToUnlock} masks to unlock";
                }
            }
        }

        private void OnEnterBiomeClicked()
        {
            if (_selectedBiome == null || _gameFlow == null) return;

            bool isUnlocked = biomeManager != null && biomeManager.IsBiomeUnlocked(_selectedBiome.Type);
            if (!isUnlocked) return;

            _gameFlow.OnBiomeSelected(_selectedBiome.Type);
        }

        private void OnBackClicked()
        {
            _gameFlow?.OnBackButtonClicked();
        }

        public void CloseInfoPanel()
        {
            if (infoPanel != null)
            {
                infoPanel.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => infoPanel.SetActive(false));
            }
            _selectedBiome = null;
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }
    }
}
