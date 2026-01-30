using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using MaskCollect.Data;
using MaskCollect.Gameplay;

namespace MaskCollect.UI
{
    /// <summary>
    /// Popup that displays when a new mask is collected.
    /// Shows celebratory animation and mask details.
    /// </summary>
    public class RewardPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Image maskImage;
        [SerializeField] private TextMeshProUGUI maskNameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image rarityGlow;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backgroundButton; // Click anywhere to close

        [Header("Effects")]
        [SerializeField] private ParticleSystem celebrationParticles;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation Settings")]
        [SerializeField] private float showDuration = 0.5f;
        [SerializeField] private float hideDuration = 0.3f;
        [SerializeField] private float autoHideDelay = 3f;
        [SerializeField] private bool autoHide = false;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonGlow = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color uncommonGlow = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color rareGlow = new Color(0.2f, 0.4f, 1f, 0.5f);
        [SerializeField] private Color legendaryGlow = new Color(1f, 0.8f, 0.2f, 0.5f);

        private bool _isShowing = false;

        private void Awake()
        {
            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (backgroundButton != null)
            {
                backgroundButton.onClick.AddListener(Hide);
            }
        }

        private void OnEnable()
        {
            if (RewardSystem.Instance != null)
            {
                RewardSystem.Instance.OnRewardGiven += HandleRewardGiven;
            }
        }

        private void OnDisable()
        {
            if (RewardSystem.Instance != null)
            {
                RewardSystem.Instance.OnRewardGiven -= HandleRewardGiven;
            }
        }

        private void HandleRewardGiven(MaskData mask, bool isNew)
        {
            if (isNew)
            {
                Show(mask);
            }
        }

        /// <summary>
        /// Show the popup with the given mask
        /// </summary>
        public void Show(MaskData mask)
        {
            if (mask == null || _isShowing) return;

            _isShowing = true;

            // Setup UI
            if (maskImage != null)
            {
                maskImage.sprite = mask.MaskSprite;
            }

            if (maskNameText != null)
            {
                maskNameText.text = mask.MaskName;
            }

            if (rarityText != null)
            {
                rarityText.text = mask.Rarity.ToString();
            }

            if (descriptionText != null)
            {
                descriptionText.text = mask.Description;
            }

            UpdateRarityGlow(mask.Rarity);

            // Show popup
            if (popupRoot != null)
            {
                popupRoot.SetActive(true);
            }

            PlayShowAnimation(mask).Forget();
        }

        private async UniTaskVoid PlayShowAnimation(MaskData mask)
        {
            // Reset state
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }

            if (maskImage != null)
            {
                maskImage.transform.localScale = Vector3.zero;
            }

            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, showDuration * 0.5f);
            }

            // Pop in mask
            if (maskImage != null)
            {
                maskImage.transform.DOScale(Vector3.one, showDuration)
                    .SetEase(Ease.OutBack);
            }

            // Play particles based on rarity
            if (celebrationParticles != null)
            {
                var main = celebrationParticles.main;
                main.startColor = GetRarityColor(mask.Rarity);
                celebrationParticles.Play();
            }

            // Auto hide if enabled
            if (autoHide)
            {
                await UniTask.WaitForSeconds(autoHideDelay);
                Hide();
            }
        }

        /// <summary>
        /// Hide the popup
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;

            PlayHideAnimation().Forget();
        }

        private async UniTaskVoid PlayHideAnimation()
        {
            // Fade out
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, hideDuration);
            }

            if (maskImage != null)
            {
                maskImage.transform.DOScale(Vector3.zero, hideDuration)
                    .SetEase(Ease.InBack);
            }

            await UniTask.WaitForSeconds(hideDuration);

            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }

            _isShowing = false;
        }

        private void UpdateRarityGlow(MaskRarity rarity)
        {
            if (rarityGlow == null) return;

            rarityGlow.color = GetRarityColor(rarity);
        }

        private Color GetRarityColor(MaskRarity rarity)
        {
            return rarity switch
            {
                MaskRarity.Common => commonGlow,
                MaskRarity.Uncommon => uncommonGlow,
                MaskRarity.Rare => rareGlow,
                MaskRarity.Legendary => legendaryGlow,
                _ => commonGlow
            };
        }
    }
}
