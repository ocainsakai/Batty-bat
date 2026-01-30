using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Main menu screen with Play, Collection, and Settings buttons.
    /// Features animated background with wandering animals.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private TextMeshProUGUI collectionCountText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject newGameConfirmPanel;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float buttonAnimDelay = 0.1f;

        [Header("Background")]
        [SerializeField] private GameObject[] backgroundAnimals;
        [SerializeField] private ParticleSystem ambientParticles;

        private GameFlowController _gameFlow;
        private InventoryManager _inventory;

        private void Awake()
        {
            SetupButtons();
        }

        private void OnEnable()
        {
            _gameFlow = GameFlowController.Instance;
            _inventory = InventoryManager.Instance;

            UpdateUI();
            PlayEnterAnimation();
        }

        private void SetupButtons()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (collectionButton != null)
            {
                collectionButton.onClick.AddListener(OnCollectionClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
            }
        }

        private void UpdateUI()
        {
            // Show Continue or Play based on save data
            bool hasSave = _gameFlow != null && _gameFlow.HasSaveData();

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(hasSave);
            }

            if (playButton != null)
            {
                // Change text based on save data
                var playText = playButton.GetComponentInChildren<TextMeshProUGUI>();
                if (playText != null)
                {
                    playText.text = hasSave ? "New Game" : "Play";
                }
            }

            // Update collection count
            if (collectionCountText != null && _inventory != null)
            {
                collectionCountText.text = $"{_inventory.CollectedCount}/{_inventory.TotalMaskCount}";
            }

            // Version text
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }
        }

        private void PlayEnterAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration);
            }

            // Animate title
            if (titleText != null)
            {
                titleText.transform.localScale = Vector3.zero;
                titleText.transform.DOScale(Vector3.one, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(0.2f);
            }

            // Animate buttons sequentially
            AnimateButtonsSequentially().Forget();

            // Start background animals
            if (backgroundAnimals != null)
            {
                foreach (var animal in backgroundAnimals)
                {
                    if (animal != null)
                    {
                        animal.SetActive(true);
                    }
                }
            }

            if (ambientParticles != null)
            {
                ambientParticles.Play();
            }
        }

        private async UniTaskVoid AnimateButtonsSequentially()
        {
            Button[] buttons = { playButton, continueButton, collectionButton, settingsButton };
            
            await UniTask.WaitForSeconds(0.3f);

            foreach (var button in buttons)
            {
                if (button != null && button.gameObject.activeSelf)
                {
                    button.transform.localScale = Vector3.zero;
                    button.transform.DOScale(Vector3.one, 0.3f)
                        .SetEase(Ease.OutBack);
                    
                    await UniTask.WaitForSeconds(buttonAnimDelay);
                }
            }
        }

        #region Button Handlers

        private void OnPlayClicked()
        {
            // If has save data, show confirmation
            if (_gameFlow != null && _gameFlow.HasSaveData())
            {
                ShowNewGameConfirmation();
            }
            else
            {
                StartNewGame();
            }
        }

        private void OnContinueClicked()
        {
            _gameFlow?.OnPlayButtonClicked();
        }

        private void ShowNewGameConfirmation()
        {
            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(true);
            }
        }

        public void OnConfirmNewGame()
        {
            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(false);
            }

            _gameFlow?.DeleteSaveData();
            StartNewGame();
        }

        public void OnCancelNewGame()
        {
            if (newGameConfirmPanel != null)
            {
                newGameConfirmPanel.SetActive(false);
            }
        }

        private void StartNewGame()
        {
            _gameFlow?.OnPlayButtonClicked();
        }

        private void OnCollectionClicked()
        {
            _gameFlow?.OnCollectionButtonClicked();
        }

        private void OnSettingsClicked()
        {
            _gameFlow?.OnSettingsButtonClicked();
        }

        private void OnCreditsClicked()
        {
            Debug.Log("[MainMenu] Credits clicked");
        }

        #endregion

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
        }
    }
}
