using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// Base class for popup dialogs.
    /// Popups overlay on top of screens and can have background dimming.
    /// </summary>
    public class UIPopup : UIView
    {
        [Header("Popup Settings")]
        [SerializeField] protected bool showBackground = true;
        [SerializeField] protected bool closeOnBackgroundClick = true;
        [SerializeField] protected bool closeOnBackButton = true;
        [SerializeField] protected Color backgroundColor = new Color(0, 0, 0, 0.5f);

        [Header("Popup References")]
        [SerializeField] protected Image backgroundImage;
        [SerializeField] protected RectTransform contentContainer;

        private Tween _backgroundTween;

        protected override void Awake()
        {
            base.Awake();

            // Create background if needed
            if (showBackground && backgroundImage == null)
            {
                CreateBackground();
            }

            // Setup background click
            if (backgroundImage != null && closeOnBackgroundClick)
            {
                var button = backgroundImage.gameObject.GetComponent<Button>();
                if (button == null)
                {
                    button = backgroundImage.gameObject.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                }
                button.onClick.AddListener(() => Hide().Forget());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _backgroundTween?.Kill();
        }

        private void CreateBackground()
        {
            // Create background GameObject
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(transform);
            bgGo.transform.SetAsFirstSibling();

            // Setup RectTransform to fill parent
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Setup Image
            backgroundImage = bgGo.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            backgroundImage.raycastTarget = true;
        }

        public override async UniTask Show()
        {
            // Register with UIManager
            if (UIManager.HasInstance)
            {
                UIManager.Instance.RegisterPopup(this);
            }

            // Animate background
            if (backgroundImage != null && showBackground)
            {
                _backgroundTween?.Kill();
                backgroundImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0);
                _backgroundTween = backgroundImage.DOFade(backgroundColor.a, showDuration).SetEase(easeType);
            }

            await base.Show();
        }

        public override async UniTask Hide()
        {
            // Animate background
            if (backgroundImage != null && showBackground)
            {
                _backgroundTween?.Kill();
                _backgroundTween = backgroundImage.DOFade(0f, hideDuration).SetEase(easeType);
            }

            await base.Hide();

            // Unregister from UIManager
            if (UIManager.HasInstance)
            {
                UIManager.Instance.UnregisterPopup(this);
            }
        }

        /// <summary>
        /// Handle back button
        /// </summary>
        public virtual void OnBackPressed()
        {
            if (closeOnBackButton)
            {
                Hide().Forget();
            }
        }

        /// <summary>
        /// Close this popup
        /// </summary>
        public void Close()
        {
            Hide().Forget();
        }
    }
}
