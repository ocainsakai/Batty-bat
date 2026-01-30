using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace MaskCollect.UI
{
    /// <summary>
    /// Tutorial controller that guides new players through the basics.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject handPointer;
        [SerializeField] private Image highlightOverlay;

        [Header("Tutorial Steps")]
        [SerializeField] private TutorialStep[] steps;

        [Header("Animation")]
        [SerializeField] private float textTypingSpeed = 0.03f;
        [SerializeField] private float stepTransitionTime = 0.3f;

        private int _currentStepIndex = 0;
        private bool _isTyping = false;
        private string _fullText;

        [System.Serializable]
        public class TutorialStep
        {
            public string instruction;
            public Sprite characterSprite;
            public Vector2 highlightPosition;
            public Vector2 highlightSize;
            public bool showHandPointer;
            public Vector2 handPointerPosition;
            public bool waitForAction; // Wait for player to complete action
            public string actionId; // Identifier for the action to wait for
        }

        private void Awake()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        private void OnEnable()
        {
            _currentStepIndex = 0;
            ShowStep(_currentStepIndex);
            PlayEnterAnimation();
        }

        private void PlayEnterAnimation()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.5f);
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.transform.localScale = Vector3.one * 0.8f;
                tutorialPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            }
        }

        private void ShowStep(int index)
        {
            if (steps == null || index >= steps.Length)
            {
                CompleteTutorial();
                return;
            }

            var step = steps[index];

            // Update instruction text with typing effect
            if (instructionText != null)
            {
                TypeText(step.instruction).Forget();
            }

            // Update character image
            if (characterImage != null && step.characterSprite != null)
            {
                characterImage.sprite = step.characterSprite;
                characterImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 1);
            }

            // Show/position hand pointer
            if (handPointer != null)
            {
                handPointer.SetActive(step.showHandPointer);
                if (step.showHandPointer)
                {
                    handPointer.GetComponent<RectTransform>().anchoredPosition = step.handPointerPosition;
                    AnimateHandPointer();
                }
            }

            // Show/position highlight
            if (highlightOverlay != null && step.highlightSize != Vector2.zero)
            {
                var rect = highlightOverlay.GetComponent<RectTransform>();
                rect.anchoredPosition = step.highlightPosition;
                rect.sizeDelta = step.highlightSize;
                highlightOverlay.gameObject.SetActive(true);
            }
            else if (highlightOverlay != null)
            {
                highlightOverlay.gameObject.SetActive(false);
            }

            // Update button visibility
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(!step.waitForAction);
            }
        }

        private async UniTaskVoid TypeText(string text)
        {
            _isTyping = true;
            _fullText = text;
            instructionText.text = "";

            foreach (char c in text)
            {
                if (!_isTyping) break;

                instructionText.text += c;
                await UniTask.WaitForSeconds(textTypingSpeed);
            }

            instructionText.text = text;
            _isTyping = false;
        }

        private void SkipTyping()
        {
            _isTyping = false;
            if (instructionText != null)
            {
                instructionText.text = _fullText;
            }
        }

        private void AnimateHandPointer()
        {
            if (handPointer == null) return;

            // Bobbing animation
            handPointer.transform.DOLocalMoveY(
                handPointer.transform.localPosition.y + 10f, 
                0.5f
            ).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnNextClicked()
        {
            if (_isTyping)
            {
                SkipTyping();
                return;
            }

            NextStep();
        }

        private void NextStep()
        {
            _currentStepIndex++;

            if (_currentStepIndex >= steps.Length)
            {
                CompleteTutorial();
            }
            else
            {
                // Transition animation
                TransitionToNextStep().Forget();
            }
        }

        private async UniTaskVoid TransitionToNextStep()
        {
            if (tutorialPanel != null)
            {
                await tutorialPanel.transform
                    .DOScale(Vector3.one * 0.9f, stepTransitionTime / 2)
                    .AsyncWaitForCompletion();

                ShowStep(_currentStepIndex);

                await tutorialPanel.transform
                    .DOScale(Vector3.one, stepTransitionTime / 2)
                    .SetEase(Ease.OutBack)
                    .AsyncWaitForCompletion();
            }
            else
            {
                ShowStep(_currentStepIndex);
            }
        }

        /// <summary>
        /// Call this when player completes an action that the tutorial was waiting for
        /// </summary>
        public void OnActionCompleted(string actionId)
        {
            if (steps == null || _currentStepIndex >= steps.Length) return;

            var currentStep = steps[_currentStepIndex];
            if (currentStep.waitForAction && currentStep.actionId == actionId)
            {
                NextStep();
            }
        }

        private void OnSkipClicked()
        {
            CompleteTutorial();
        }

        private void CompleteTutorial()
        {
            // Fade out animation
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, 0.3f)
                    .OnComplete(() =>
                    {
                        GameFlowController.Instance?.CompleteTutorial();
                        gameObject.SetActive(false);
                    });
            }
            else
            {
                GameFlowController.Instance?.CompleteTutorial();
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            DOTween.Kill(gameObject);
            DOTween.Kill(handPointer);
        }
    }
}
