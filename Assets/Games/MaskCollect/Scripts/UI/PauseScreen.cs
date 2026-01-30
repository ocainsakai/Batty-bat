using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace MaskCollect.UI
{
    /// <summary>
    /// Pause screen with resume, settings, and quit options.
    /// </summary>
    public class PauseScreen : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button quitButton;

        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject pausePanel;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.2f;

        private GameFlowController _gameFlow;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            _gameFlow = GameFlowController.Instance;
            PlayEnterAnimation();
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (homeButton != null)
            {
                homeButton.onClick.AddListener(OnHomeClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void PlayEnterAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
            }

            if (pausePanel != null)
            {
                pausePanel.transform.localScale = Vector3.one * 0.8f;
                pausePanel.transform.DOScale(Vector3.one, fadeInDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }

        private void OnResumeClicked()
        {
            _gameFlow?.OnResumeButtonClicked();
        }

        private void OnSettingsClicked()
        {
            _gameFlow?.OnSettingsButtonClicked();
        }

        private void OnHomeClicked()
        {
            _gameFlow?.OnHomeButtonClicked();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }
    }
}
