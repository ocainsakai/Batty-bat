using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Gameplay HUD that shows during active play.
    /// Includes pause button, collection counter, and biome indicator.
    /// </summary>
    public class GameplayHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventory;
        [SerializeField] private BiomeManager biomeManager;

        [Header("Top Bar")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private TextMeshProUGUI biomeNameText;
        [SerializeField] private Image biomeIcon;

        [Header("Collection Counter")]
        [SerializeField] private TextMeshProUGUI maskCountText;
        [SerializeField] private Image maskIcon;
        [SerializeField] private Button collectionButton;

        [Header("Quick Actions")]
        [SerializeField] private Button mapButton;

        [Header("Notification")]
        [SerializeField] private GameObject notificationBanner;
        [SerializeField] private TextMeshProUGUI notificationText;

        [Header("Mobile Controls")]
        [SerializeField] private GameObject joystickContainer;

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

            if (biomeManager == null)
            {
                biomeManager = BiomeManager.Instance;
            }

            // Subscribe to events
            if (inventory != null)
            {
                inventory.OnMaskCollected += HandleMaskCollected;
            }

            if (biomeManager != null)
            {
                biomeManager.OnBiomeEntered += HandleBiomeEntered;
            }

            UpdateUI();

            // Show mobile controls if on mobile
            if (joystickContainer != null)
            {
                joystickContainer.SetActive(Application.isMobilePlatform);
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnMaskCollected -= HandleMaskCollected;
            }

            if (biomeManager != null)
            {
                biomeManager.OnBiomeEntered -= HandleBiomeEntered;
            }
        }

        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (homeButton != null)
            {
                homeButton.onClick.AddListener(OnHomeClicked);
            }

            if (collectionButton != null)
            {
                collectionButton.onClick.AddListener(OnCollectionClicked);
            }

            if (mapButton != null)
            {
                mapButton.onClick.AddListener(OnMapClicked);
            }
        }

        private void UpdateUI()
        {
            UpdateMaskCount();
            UpdateBiomeInfo();
        }

        private void UpdateMaskCount()
        {
            if (maskCountText != null && inventory != null)
            {
                maskCountText.text = $"{inventory.CollectedCount}/{inventory.TotalMaskCount}";
            }
        }

        private void UpdateBiomeInfo()
        {
            if (biomeManager == null || biomeManager.CurrentBiome == null) return;

            var biome = biomeManager.CurrentBiome;

            if (biomeNameText != null)
            {
                biomeNameText.text = biome.BiomeName;
            }

            if (biomeIcon != null && biome.MinimapIcon != null)
            {
                biomeIcon.sprite = biome.MinimapIcon;
                biomeIcon.color = biome.PrimaryColor;
            }
        }

        private void HandleMaskCollected(MaskData mask)
        {
            UpdateMaskCount();

            // Animate mask icon
            if (maskIcon != null)
            {
                maskIcon.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 2);
            }

            // Show notification
            ShowNotification($"Received: {mask.MaskName}!");
        }

        private void HandleBiomeEntered(BiomeData biome)
        {
            UpdateBiomeInfo();
            ShowNotification($"Welcome to {biome.BiomeName}!");
        }

        /// <summary>
        /// Show a temporary notification banner
        /// </summary>
        public void ShowNotification(string message, float duration = 2f)
        {
            if (notificationBanner == null || notificationText == null) return;

            notificationText.text = message;
            notificationBanner.SetActive(true);

            // Animate in
            notificationBanner.transform.localScale = new Vector3(1f, 0f, 1f);
            notificationBanner.transform.DOScaleY(1f, 0.2f).SetEase(Ease.OutBack);

            // Auto hide
            DOTween.Sequence()
                .AppendInterval(duration)
                .Append(notificationBanner.transform.DOScaleY(0f, 0.2f))
                .OnComplete(() => notificationBanner.SetActive(false));
        }

        #region Button Handlers

        private void OnPauseClicked()
        {
            _gameFlow?.OnPauseButtonClicked();
        }

        private void OnHomeClicked()
        {
            _gameFlow?.OnHomeButtonClicked();
        }

        private void OnCollectionClicked()
        {
            _gameFlow?.OnCollectionButtonClicked();
        }

        private void OnMapClicked()
        {
            _gameFlow?.TransitionToState(GameState.WorldMap);
        }

        #endregion
    }
}
