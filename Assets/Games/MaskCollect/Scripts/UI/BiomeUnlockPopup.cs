using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Popup displayed when a new biome is unlocked.
    /// </summary>
    public class BiomeUnlockPopup : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private Image biomeImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI biomeNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button goButton;
        [SerializeField] private Button laterButton;

        [Header("Effects")]
        [SerializeField] private ParticleSystem unlockParticles;
        [SerializeField] private Image[] rays;

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.5f;

        private BiomeData _unlockedBiome;
        private System.Action<BiomeData> _onGoClicked;
        private System.Action _onLaterClicked;

        private static BiomeUnlockPopup _instance;
        public static BiomeUnlockPopup Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            if (goButton != null)
            {
                goButton.onClick.AddListener(OnGoClicked);
            }

            if (laterButton != null)
            {
                laterButton.onClick.AddListener(OnLaterClicked);
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the unlock popup for a biome
        /// </summary>
        public void Show(BiomeData biome, System.Action<BiomeData> onGo = null, System.Action onLater = null)
        {
            _unlockedBiome = biome;
            _onGoClicked = onGo;
            _onLaterClicked = onLater;

            gameObject.SetActive(true);

            // Update UI
            if (biomeNameText != null)
            {
                biomeNameText.text = biome.BiomeName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = biome.Description;
            }

            if (biomeImage != null && biome.BackgroundSprite != null)
            {
                biomeImage.sprite = biome.BackgroundSprite;
            }

            if (titleText != null)
            {
                titleText.text = "Vùng Đất Mới Mở Khóa!";
            }

            PlayShowAnimation();
        }

        private void PlayShowAnimation()
        {
            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, animationDuration * 0.5f);
            }

            // Scale pop
            if (popupPanel != null)
            {
                popupPanel.transform.localScale = Vector3.zero;
                popupPanel.transform.DOScale(Vector3.one, animationDuration)
                    .SetEase(Ease.OutBack);
            }

            // Rotate rays
            if (rays != null)
            {
                foreach (var ray in rays)
                {
                    if (ray != null)
                    {
                        ray.transform.DORotate(new Vector3(0, 0, 360), 10f, RotateMode.FastBeyond360)
                            .SetLoops(-1, LoopType.Restart)
                            .SetEase(Ease.Linear);
                    }
                }
            }

            // Play particles
            if (unlockParticles != null)
            {
                unlockParticles.Play();
            }
        }

        private void PlayHideAnimation(System.Action onComplete)
        {
            // Scale down
            if (popupPanel != null)
            {
                popupPanel.transform.DOScale(Vector3.zero, animationDuration * 0.5f)
                    .SetEase(Ease.InBack);
            }

            // Fade out
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, animationDuration * 0.5f)
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        onComplete?.Invoke();
                    });
            }
            else
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            }

            // Stop ray rotation
            if (rays != null)
            {
                foreach (var ray in rays)
                {
                    if (ray != null)
                    {
                        DOTween.Kill(ray.transform);
                    }
                }
            }
        }

        private void OnGoClicked()
        {
            PlayHideAnimation(() => _onGoClicked?.Invoke(_unlockedBiome));
        }

        private void OnLaterClicked()
        {
            PlayHideAnimation(() => _onLaterClicked?.Invoke());
        }

        /// <summary>
        /// Static helper to show popup
        /// </summary>
        public static void ShowUnlock(BiomeData biome, System.Action<BiomeData> onGo = null, System.Action onLater = null)
        {
            if (Instance != null)
            {
                Instance.Show(biome, onGo, onLater);
            }
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
            DOTween.Kill(popupPanel);
        }
    }
}
