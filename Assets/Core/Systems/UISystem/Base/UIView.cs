using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// Base class for all UI views (screens, popups, panels).
    /// Provides common functionality for showing, hiding, and animating UI.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        [Header("View Settings")]
        [SerializeField] protected string viewId;
        [SerializeField] protected bool showOnStart = false;
        [SerializeField] protected bool destroyOnHide = false;

        [Header("Animation")]
        [SerializeField] protected float showDuration = 0.3f;
        [SerializeField] protected float hideDuration = 0.2f;
        [SerializeField] protected AnimationType animationType = AnimationType.Fade;
        [SerializeField] protected Ease easeType = Ease.OutQuad;

        [Header("Events")]
        public UnityEvent OnShowStarted;
        public UnityEvent OnShowCompleted;
        public UnityEvent OnHideStarted;
        public UnityEvent OnHideCompleted;

        protected CanvasGroup _canvasGroup;
        protected RectTransform _rectTransform;
        protected bool _isVisible = false;
        protected bool _isAnimating = false;
        protected Sequence _currentAnimation;

        public string ViewId => string.IsNullOrEmpty(viewId) ? gameObject.name : viewId;
        public bool IsVisible => _isVisible;
        public bool IsAnimating => _isAnimating;

        public enum AnimationType
        {
            None,
            Fade,
            Scale,
            SlideFromLeft,
            SlideFromRight,
            SlideFromTop,
            SlideFromBottom,
            FadeAndScale
        }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Initialize hidden state
            if (!showOnStart)
            {
                SetVisibility(false, true);
            }
        }

        protected virtual async void Start()
        {
            if (showOnStart)
            {
                await Show();
            }
        }

        protected virtual void OnDestroy()
        {
            // Kill any running animations
            _currentAnimation?.Kill();
        }

        /// <summary>
        /// Show the view with animation
        /// </summary>
        public virtual async UniTask Show()
        {
            if (_isVisible || _isAnimating) return;

            _isAnimating = true;
            gameObject.SetActive(true);

            OnShowStarted?.Invoke();
            OnBeforeShow();

            await PlayShowAnimation();

            _isVisible = true;
            _isAnimating = false;
            SetInteractable(true);

            OnShowCompleted?.Invoke();
            OnAfterShow();
        }

        /// <summary>
        /// Hide the view with animation
        /// </summary>
        public virtual async UniTask Hide()
        {
            if (!_isVisible || _isAnimating) return;

            _isAnimating = true;
            SetInteractable(false);

            OnHideStarted?.Invoke();
            OnBeforeHide();

            await PlayHideAnimation();

            _isVisible = false;
            _isAnimating = false;

            if (destroyOnHide)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }

            OnHideCompleted?.Invoke();
            OnAfterHide();
        }

        /// <summary>
        /// Show immediately without animation
        /// </summary>
        public virtual void ShowImmediate()
        {
            _currentAnimation?.Kill();
            gameObject.SetActive(true);
            SetVisibility(true, true);
            _isVisible = true;
            OnAfterShow();
        }

        /// <summary>
        /// Hide immediately without animation
        /// </summary>
        public virtual void HideImmediate()
        {
            _currentAnimation?.Kill();
            SetVisibility(false, true);
            _isVisible = false;
            
            if (destroyOnHide)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
            
            OnAfterHide();
        }

        /// <summary>
        /// Toggle visibility
        /// </summary>
        public virtual async UniTask Toggle()
        {
            if (_isVisible)
                await Hide();
            else
                await Show();
        }

        protected virtual async UniTask PlayShowAnimation()
        {
            _currentAnimation?.Kill();
            
            switch (animationType)
            {
                case AnimationType.Fade:
                    await AnimateFade(0f, 1f, showDuration);
                    break;
                case AnimationType.Scale:
                    _canvasGroup.alpha = 1f;
                    await AnimateScale(Vector3.zero, Vector3.one, showDuration);
                    break;
                case AnimationType.SlideFromLeft:
                    _canvasGroup.alpha = 1f;
                    await AnimateSlide(new Vector2(-Screen.width, 0), Vector2.zero, showDuration);
                    break;
                case AnimationType.SlideFromRight:
                    _canvasGroup.alpha = 1f;
                    await AnimateSlide(new Vector2(Screen.width, 0), Vector2.zero, showDuration);
                    break;
                case AnimationType.SlideFromTop:
                    _canvasGroup.alpha = 1f;
                    await AnimateSlide(new Vector2(0, Screen.height), Vector2.zero, showDuration);
                    break;
                case AnimationType.SlideFromBottom:
                    _canvasGroup.alpha = 1f;
                    await AnimateSlide(new Vector2(0, -Screen.height), Vector2.zero, showDuration);
                    break;
                case AnimationType.FadeAndScale:
                    await AnimateFadeAndScale(0f, 1f, Vector3.one * 0.8f, Vector3.one, showDuration);
                    break;
                case AnimationType.None:
                default:
                    SetVisibility(true, true);
                    break;
            }
        }

        protected virtual async UniTask PlayHideAnimation()
        {
            _currentAnimation?.Kill();
            
            switch (animationType)
            {
                case AnimationType.Fade:
                    await AnimateFade(1f, 0f, hideDuration);
                    break;
                case AnimationType.Scale:
                    await AnimateScale(Vector3.one, Vector3.zero, hideDuration);
                    break;
                case AnimationType.SlideFromLeft:
                    await AnimateSlide(Vector2.zero, new Vector2(-Screen.width, 0), hideDuration);
                    break;
                case AnimationType.SlideFromRight:
                    await AnimateSlide(Vector2.zero, new Vector2(Screen.width, 0), hideDuration);
                    break;
                case AnimationType.SlideFromTop:
                    await AnimateSlide(Vector2.zero, new Vector2(0, Screen.height), hideDuration);
                    break;
                case AnimationType.SlideFromBottom:
                    await AnimateSlide(Vector2.zero, new Vector2(0, -Screen.height), hideDuration);
                    break;
                case AnimationType.FadeAndScale:
                    await AnimateFadeAndScale(1f, 0f, Vector3.one, Vector3.one * 0.8f, hideDuration);
                    break;
                case AnimationType.None:
                default:
                    SetVisibility(false, true);
                    break;
            }
        }

        protected async UniTask AnimateFade(float from, float to, float duration)
        {
            _canvasGroup.alpha = from;
            await _canvasGroup.DOFade(to, duration).SetEase(easeType).AsyncWaitForCompletion();
        }

        protected async UniTask AnimateScale(Vector3 from, Vector3 to, float duration)
        {
            _rectTransform.localScale = from;
            await _rectTransform.DOScale(to, duration).SetEase(easeType).AsyncWaitForCompletion();
        }

        protected async UniTask AnimateSlide(Vector2 from, Vector2 to, float duration)
        {
            _rectTransform.anchoredPosition = from;
            await _rectTransform.DOAnchorPos(to, duration).SetEase(easeType).AsyncWaitForCompletion();
        }

        protected async UniTask AnimateFadeAndScale(float fromAlpha, float toAlpha, Vector3 fromScale, Vector3 toScale, float duration)
        {
            _canvasGroup.alpha = fromAlpha;
            _rectTransform.localScale = fromScale;
            
            _currentAnimation = DOTween.Sequence();
            _currentAnimation.Join(_canvasGroup.DOFade(toAlpha, duration).SetEase(easeType));
            _currentAnimation.Join(_rectTransform.DOScale(toScale, duration).SetEase(easeType));
            
            await _currentAnimation.AsyncWaitForCompletion();
        }

        protected void SetVisibility(bool visible, bool immediate)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
            }
            
            if (immediate)
            {
                SetInteractable(visible);
            }
        }

        protected void SetInteractable(bool interactable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = interactable;
                _canvasGroup.blocksRaycasts = interactable;
            }
        }

        // Override these in derived classes for custom behavior
        protected virtual void OnBeforeShow() { }
        protected virtual void OnAfterShow() { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnAfterHide() { }
    }
}
