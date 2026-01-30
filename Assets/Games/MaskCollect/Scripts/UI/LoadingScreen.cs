using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace MaskCollect.UI
{
    /// <summary>
    /// Loading screen with progress bar and cute animal quotes.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image logoImage;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI quoteText;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float quoteChangeInterval = 3f;

        [Header("Quotes")]
        [SerializeField] private string[] animalQuotes = new[]
        {
            "\"Mỗi người bạn nhỏ đều có một câu chuyện...\" - Chú Thỏ",
            "\"Hãy kiên nhẫn, phép màu sẽ đến!\" - Cáo con",
            "\"Giúp đỡ ai đó khiến ta thấy hạnh phúc.\" - Capybara",
            "\"Bạn bè là kho báu quý giá nhất.\" - Chó Shiba",
            "\"Mặt nạ không chỉ là vật phẩm, đó là ký ức.\" - Cú Mèo",
            "\"Khám phá thế giới, khám phá chính mình!\" - Red Panda",
            "\"Đôi khi điều kỳ diệu ở ngay bên cạnh ta.\" - Mèo Calico",
            "\"Nước có thể chảy, nước có thể đập...\" - Axolotl",
            "\"Sự bình yên đến từ trái tim thanh thản.\" - Gấu Trúc",
            "\"Dũng cảm không có nghĩa là không sợ hãi.\" - Sư Tử"
        };

        private int _currentQuoteIndex = 0;
        private bool _isActive = false;

        private void OnEnable()
        {
            _isActive = true;
            PlayEnterAnimation();
            StartQuoteRotation().Forget();
        }

        private void OnDisable()
        {
            _isActive = false;
            DOTween.Kill(gameObject);
        }

        private void PlayEnterAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration);
            }

            if (logoImage != null)
            {
                logoImage.transform.localScale = Vector3.one * 0.5f;
                logoImage.transform.DOScale(Vector3.one, fadeInDuration)
                    .SetEase(Ease.OutBack);
            }

            // Start with first quote
            if (quoteText != null && animalQuotes.Length > 0)
            {
                quoteText.text = animalQuotes[0];
            }
        }

        public async UniTask PlayExitAnimation()
        {
            if (canvasGroup != null)
            {
                await canvasGroup.DOFade(0f, fadeOutDuration).AsyncWaitForCompletion();
            }
        }

        /// <summary>
        /// Update loading progress (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressBar != null)
            {
                progressBar.DOValue(progress, 0.2f);
            }

            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// Set loading tip text
        /// </summary>
        public void SetTip(string tip)
        {
            if (tipText != null)
            {
                tipText.text = tip;
            }
        }

        private async UniTaskVoid StartQuoteRotation()
        {
            while (_isActive)
            {
                await UniTask.WaitForSeconds(quoteChangeInterval);

                if (!_isActive || quoteText == null) break;

                // Fade out current quote
                await quoteText.DOFade(0f, 0.3f).AsyncWaitForCompletion();

                // Change quote
                _currentQuoteIndex = (_currentQuoteIndex + 1) % animalQuotes.Length;
                quoteText.text = animalQuotes[_currentQuoteIndex];

                // Fade in new quote
                await quoteText.DOFade(1f, 0.3f).AsyncWaitForCompletion();
            }
        }
    }
}
