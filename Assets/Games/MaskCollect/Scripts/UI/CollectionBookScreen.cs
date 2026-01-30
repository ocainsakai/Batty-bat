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
    /// Collection book screen that displays all masks in a grid format.
    /// Creates a "collect them all" feeling with locked slots for undiscovered masks.
    /// </summary>
    public class CollectionBookScreen : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskDatabase maskDatabase;
        [SerializeField] private InventoryManager inventory;
        [SerializeField] private BiomeManager biomeManager;

        [Header("UI Elements")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private GameObject maskSlotPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeDetailButton;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Progress Display")]
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI percentageText;

        [Header("Filter Tabs")]
        [SerializeField] private Button allTab;
        [SerializeField] private Button ownedTab;
        [SerializeField] private Button missingTab;
        [SerializeField] private Button[] biomeTabs;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private Image detailMaskImage;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescText;
        [SerializeField] private TextMeshProUGUI detailRarityText;
        [SerializeField] private TextMeshProUGUI detailAnimalText;
        [SerializeField] private TextMeshProUGUI detailBiomeText;
        [SerializeField] private Image detailRarityIcon;

        [Header("Visual Settings")]
        [SerializeField] private Color lockedSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color[] rarityColors = new Color[]
        {
            new Color(0.7f, 0.7f, 0.7f),    // Common - Gray
            new Color(0.3f, 0.8f, 0.3f),    // Uncommon - Green
            new Color(0.3f, 0.5f, 1f),      // Rare - Blue
            new Color(1f, 0.6f, 0.2f),      // Epic - Orange
            new Color(1f, 0.8f, 0.2f)       // Legendary - Gold
        };

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float slotAnimDelay = 0.02f;

        private List<CollectionSlotUI> _slots = new();
        private MaskData _selectedMask;
        private FilterMode _currentFilter = FilterMode.All;

        private enum FilterMode
        {
            All,
            Owned,
            Missing,
            Biome
        }

        private BiomeType _currentBiomeFilter;
        private GameFlowController _gameFlow;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            _gameFlow = GameFlowController.Instance;
            
            if (inventory == null)
            {
                inventory = InventoryManager.Instance;
            }

            InitializeCollection();
            UpdateProgress();
            PlayEnterAnimation();

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        private void SetupButtons()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (closeDetailButton != null)
            {
                closeDetailButton.onClick.AddListener(CloseDetailPanel);
            }

            if (allTab != null)
            {
                allTab.onClick.AddListener(() => SetFilter(FilterMode.All));
            }

            if (ownedTab != null)
            {
                ownedTab.onClick.AddListener(() => SetFilter(FilterMode.Owned));
            }

            if (missingTab != null)
            {
                missingTab.onClick.AddListener(() => SetFilter(FilterMode.Missing));
            }
        }

        private void InitializeCollection()
        {
            // Clear existing slots
            foreach (var slot in _slots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _slots.Clear();

            if (maskDatabase == null || gridContainer == null || maskSlotPrefab == null)
            {
                return;
            }

            // Create slots for all masks
            foreach (var mask in maskDatabase.AllMasks)
            {
                if (mask == null) continue;

                var slotObj = Instantiate(maskSlotPrefab, gridContainer);
                var slot = slotObj.GetComponent<CollectionSlotUI>();

                if (slot != null)
                {
                    bool isOwned = inventory != null && inventory.HasMask(mask.MaskId);
                    slot.Initialize(mask, isOwned, GetRarityColor(mask.Rarity), lockedSlotColor);
                    slot.OnSlotClicked += OnSlotClicked;
                    _slots.Add(slot);
                }
            }
        }

        private void UpdateProgress()
        {
            if (inventory == null) return;

            int collected = inventory.CollectedCount;
            int total = inventory.TotalMaskCount;
            float progress = total > 0 ? (float)collected / total : 0f;

            if (progressText != null)
            {
                progressText.text = $"{collected}/{total} Mặt nạ";
            }

            if (progressSlider != null)
            {
                progressSlider.DOValue(progress, 0.5f);
            }

            if (percentageText != null)
            {
                percentageText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        private void PlayEnterAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration);
            }

            // Animate slots appearing
            AnimateSlots().Forget();
        }

        private async UniTaskVoid AnimateSlots()
        {
            await UniTask.WaitForSeconds(0.2f);

            foreach (var slot in _slots)
            {
                if (slot != null && slot.gameObject.activeSelf)
                {
                    slot.transform.localScale = Vector3.zero;
                    slot.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                    await UniTask.WaitForSeconds(slotAnimDelay);
                }
            }
        }

        private void SetFilter(FilterMode mode)
        {
            _currentFilter = mode;
            ApplyFilter();
        }

        public void SetBiomeFilter(BiomeType biomeType)
        {
            _currentFilter = FilterMode.Biome;
            _currentBiomeFilter = biomeType;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            foreach (var slot in _slots)
            {
                if (slot == null) continue;

                bool show = _currentFilter switch
                {
                    FilterMode.All => true,
                    FilterMode.Owned => slot.IsOwned,
                    FilterMode.Missing => !slot.IsOwned,
                    FilterMode.Biome => slot.MaskData.HomeBiome == _currentBiomeFilter,
                    _ => true
                };

                slot.gameObject.SetActive(show);
            }
        }

        private void OnSlotClicked(MaskData mask, bool isOwned)
        {
            _selectedMask = mask;
            ShowDetailPanel(mask, isOwned);
        }

        private void ShowDetailPanel(MaskData mask, bool isOwned)
        {
            if (detailPanel == null) return;

            detailPanel.SetActive(true);
            detailPanel.transform.localScale = Vector3.one * 0.8f;
            detailPanel.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

            if (detailMaskImage != null)
            {
                detailMaskImage.sprite = isOwned ? mask.MaskSprite : mask.SilhouetteSprite;
                detailMaskImage.color = isOwned ? Color.white : lockedSlotColor;
            }

            if (detailNameText != null)
            {
                detailNameText.text = isOwned ? mask.MaskName : "???";
            }

            if (detailDescText != null)
            {
                detailDescText.text = isOwned ? mask.Description : "Hãy tìm kiếm mặt nạ này!";
            }

            if (detailRarityText != null)
            {
                detailRarityText.text = GetRarityName(mask.Rarity);
                detailRarityText.color = GetRarityColor(mask.Rarity);
            }

            if (detailAnimalText != null)
            {
                detailAnimalText.text = isOwned 
                    ? $"Từ: {FormatAnimalName(mask.AssociatedAnimal)}"
                    : "Từ: ???";
            }

            if (detailBiomeText != null)
            {
                detailBiomeText.text = isOwned
                    ? $"Vùng: {GetBiomeName(mask.HomeBiome)}"
                    : "Vùng: ???";
            }

            if (detailRarityIcon != null)
            {
                detailRarityIcon.color = GetRarityColor(mask.Rarity);
            }
        }

        private void CloseDetailPanel()
        {
            if (detailPanel == null) return;

            detailPanel.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => detailPanel.SetActive(false));

            _selectedMask = null;
        }

        private Color GetRarityColor(MaskRarity rarity)
        {
            return rarity switch
            {
                MaskRarity.Common => rarityColors[0],
                MaskRarity.Uncommon => rarityColors[1],
                MaskRarity.Rare => rarityColors[2],
                MaskRarity.Legendary => rarityColors.Length > 3 ? rarityColors[3] : rarityColors[2],
                _ => Color.white
            };
        }

        private string GetRarityName(MaskRarity rarity)
        {
            return rarity switch
            {
                MaskRarity.Common => "Thường",
                MaskRarity.Uncommon => "Không phổ biến",
                MaskRarity.Rare => "Hiếm",
                MaskRarity.Legendary => "Huyền thoại",
                _ => "Thường"
            };
        }

        private string FormatAnimalName(string animalType)
        {
            return animalType switch
            {
                "dog" => "Chó Shiba",
                "cat" => "Mèo Calico",
                "rabbit" => "Thỏ",
                "bird" => "Chim",
                "fox" => "Cáo",
                "owl" => "Cú Mèo",
                "redpanda" => "Red Panda",
                "panda" => "Gấu Trúc",
                "lion" => "Sư Tử",
                "capybara" => "Capybara",
                "axolotl" => "Axolotl",
                _ => animalType
            };
        }

        private string GetBiomeName(BiomeType biomeType)
        {
            return biomeType switch
            {
                BiomeType.PeacefulMeadow => "Thảo Nguyên Bình Yên",
                BiomeType.MysticForest => "Rừng Bí Ẩn",
                BiomeType.ZenValley => "Thung Lũng Thiền",
                BiomeType.WhimsicalWetlands => "Đầm Lầy Diệu Kỳ",
                _ => biomeType.ToString()
            };
        }

        private void OnBackClicked()
        {
            _gameFlow?.OnBackButtonClicked();
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }
    }
}
