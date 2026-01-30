using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Heads-Up Display showing collection progress and quick actions.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventory;

        [Header("Collection Counter")]
        [SerializeField] private TextMeshProUGUI collectionCountText;
        [SerializeField] private Slider collectionProgressBar;
        [SerializeField] private Image progressFillImage;

        [Header("Buttons")]
        [SerializeField] private Button collectionBookButton;
        [SerializeField] private Button settingsButton;

        [Header("Collection Book")]
        [SerializeField] private CollectionView collectionView;

        [Header("Visual Settings")]
        [SerializeField] private Color progressColorStart = Color.red;
        [SerializeField] private Color progressColorEnd = Color.green;

        [Header("Animation")]
        [SerializeField] private float countUpdateDuration = 0.5f;

        private int _displayedCount = 0;

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnMaskCollected += HandleMaskCollected;
                inventory.OnInventoryLoaded += RefreshDisplay;
            }

            if (collectionBookButton != null)
            {
                collectionBookButton.onClick.AddListener(OpenCollectionBook);
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnMaskCollected -= HandleMaskCollected;
                inventory.OnInventoryLoaded -= RefreshDisplay;
            }

            if (collectionBookButton != null)
            {
                collectionBookButton.onClick.RemoveListener(OpenCollectionBook);
            }
        }

        private void Start()
        {
            if (inventory == null)
            {
                inventory = InventoryManager.Instance;
            }

            RefreshDisplay();
        }

        private void HandleMaskCollected(MaskData mask)
        {
            // Animate count increase
            AnimateCountUpdate(inventory.CollectedCount);

            // Pulse the counter
            PlayCounterPulse();
        }

        private void RefreshDisplay()
        {
            if (inventory == null) return;

            UpdateCountDisplay(inventory.CollectedCount);
            UpdateProgressBar(inventory.GetCollectionProgress());
        }

        private void UpdateCountDisplay(int count)
        {
            _displayedCount = count;
            
            if (collectionCountText != null)
            {
                collectionCountText.text = $"{count}/{inventory.TotalMaskCount}";
            }
        }

        private void UpdateProgressBar(float progress)
        {
            if (collectionProgressBar != null)
            {
                collectionProgressBar.value = progress;
            }

            if (progressFillImage != null)
            {
                progressFillImage.color = Color.Lerp(progressColorStart, progressColorEnd, progress);
            }
        }

        private void AnimateCountUpdate(int targetCount)
        {
            DOTween.Kill(gameObject);
            DOTween.To(() => _displayedCount, x =>
                {
                    _displayedCount = x;
                    if (collectionCountText != null)
                    {
                        collectionCountText.text = $"{_displayedCount}/{inventory.TotalMaskCount}";
                    }
                }, targetCount, countUpdateDuration)
                .SetTarget(gameObject)
                .OnComplete(() =>
                {
                    _displayedCount = targetCount;
                    UpdateProgressBar(inventory.GetCollectionProgress());
                });
        }

        private void PlayCounterPulse()
        {
            if (collectionCountText == null) return;

            var originalScale = collectionCountText.transform.localScale;
            DOTween.Kill(collectionCountText.gameObject);
            
            collectionCountText.transform
                .DOScale(originalScale * 1.3f, 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    collectionCountText.transform
                        .DOScale(originalScale, 0.15f)
                        .SetEase(Ease.InQuad);
                });
        }

        private void OpenCollectionBook()
        {
            if (collectionView != null)
            {
                collectionView.Toggle();
            }
        }

        public void OpenSettings()
        {
            // TODO: Implement settings panel
            Debug.Log("[HUD] Settings clicked");
        }
    }
}
