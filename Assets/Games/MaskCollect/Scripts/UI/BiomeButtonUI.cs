using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Individual biome button on the world map.
    /// </summary>
    public class BiomeButtonUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button button;
        [SerializeField] private Image biomeIcon;
        [SerializeField] private Image borderImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private Slider progressBar;

        [Header("Colors")]
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color unlockedColor = Color.white;

        private BiomeData _biomeData;
        private bool _isUnlocked;

        public event Action<BiomeData> OnClicked;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        public void Initialize(BiomeData biome, bool isUnlocked, float unlockProgress)
        {
            _biomeData = biome;
            _isUnlocked = isUnlocked;

            // Set icon
            if (biomeIcon != null)
            {
                if (biome.MinimapIcon != null)
                {
                    biomeIcon.sprite = biome.MinimapIcon;
                }
                biomeIcon.color = isUnlocked ? unlockedColor : lockedColor;
            }

            // Set border color based on biome
            if (borderImage != null)
            {
                borderImage.color = biome.PrimaryColor;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = biome.BiomeName;
                nameText.color = isUnlocked ? Color.white : Color.gray;
            }

            // Show/hide lock
            if (lockIcon != null)
            {
                lockIcon.SetActive(!isUnlocked);
            }

            // Hide new badge by default
            if (newBadge != null)
            {
                newBadge.SetActive(false);
            }

            // Show progress if locked
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(!isUnlocked && unlockProgress > 0);
                progressBar.value = unlockProgress;
            }
        }

        private void HandleClick()
        {
            // Play click animation
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1);

            OnClicked?.Invoke(_biomeData);
        }

        /// <summary>
        /// Play unlock animation
        /// </summary>
        public void PlayUnlockAnimation()
        {
            _isUnlocked = true;

            // Hide lock with animation
            if (lockIcon != null)
            {
                lockIcon.transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => lockIcon.SetActive(false));
            }

            // Brighten icon
            if (biomeIcon != null)
            {
                biomeIcon.DOColor(unlockedColor, 0.5f);
            }

            // Pop animation
            transform.DOPunchScale(Vector3.one * 0.2f, 0.5f, 2);

            // Show new badge
            if (newBadge != null)
            {
                newBadge.SetActive(true);
                newBadge.transform.localScale = Vector3.zero;
                newBadge.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }

            // Hide progress bar
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            OnClicked = null;
        }
    }
}
