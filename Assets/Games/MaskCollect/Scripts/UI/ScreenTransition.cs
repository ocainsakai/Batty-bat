using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace MaskCollect.UI
{
    /// <summary>
    /// Screen transition effects (fade, slide, etc.)
    /// </summary>
    public class ScreenTransition : MonoBehaviour
    {
        [Header("Transition Elements")]
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private Image slidePanel;
        [SerializeField] private RectTransform circleWipe;

        [Header("Settings")]
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private Color fadeColor = Color.black;

        private static ScreenTransition _instance;
        public static ScreenTransition Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeOverlays();
        }

        private void InitializeOverlays()
        {
            // Create fade overlay if not assigned
            if (fadeOverlay == null)
            {
                var canvas = GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 999;
                    gameObject.AddComponent<CanvasScaler>();
                    gameObject.AddComponent<GraphicRaycaster>();
                }

                var overlayGO = new GameObject("FadeOverlay");
                overlayGO.transform.SetParent(transform);
                
                var image = overlayGO.AddComponent<Image>();
                image.color = fadeColor;
                
                var rect = overlayGO.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;

                fadeOverlay = overlayGO.AddComponent<CanvasGroup>();
                fadeOverlay.alpha = 0f;
                fadeOverlay.blocksRaycasts = false;
            }
            else
            {
                fadeOverlay.alpha = 0f;
                fadeOverlay.blocksRaycasts = false;
            }
        }

        #region Fade Transitions

        /// <summary>
        /// Fade to black
        /// </summary>
        public async UniTask FadeOut(float duration = -1)
        {
            if (duration < 0) duration = defaultDuration;

            fadeOverlay.blocksRaycasts = true;
            await fadeOverlay.DOFade(1f, duration).SetUpdate(true).AsyncWaitForCompletion();
        }

        /// <summary>
        /// Fade from black
        /// </summary>
        public async UniTask FadeIn(float duration = -1)
        {
            if (duration < 0) duration = defaultDuration;

            await fadeOverlay.DOFade(0f, duration).SetUpdate(true).AsyncWaitForCompletion();
            fadeOverlay.blocksRaycasts = false;
        }

        /// <summary>
        /// Fade out, execute action, fade in
        /// </summary>
        public async UniTask FadeTransition(System.Action duringFade, float duration = -1)
        {
            if (duration < 0) duration = defaultDuration;

            await FadeOut(duration / 2);
            duringFade?.Invoke();
            await FadeIn(duration / 2);
        }

        /// <summary>
        /// Async fade transition
        /// </summary>
        public async UniTask FadeTransitionAsync(System.Func<UniTask> duringFade, float duration = -1)
        {
            if (duration < 0) duration = defaultDuration;

            await FadeOut(duration / 2);
            if (duringFade != null) await duringFade();
            await FadeIn(duration / 2);
        }

        #endregion

        #region Slide Transitions

        /// <summary>
        /// Slide panel from direction
        /// </summary>
        public async UniTask SlideIn(SlideDirection direction, float duration = -1)
        {
            if (slidePanel == null) return;
            if (duration < 0) duration = defaultDuration;

            slidePanel.gameObject.SetActive(true);
            var rect = slidePanel.rectTransform;

            Vector2 startPos = GetSlideStartPosition(direction);
            rect.anchoredPosition = startPos;

            await rect.DOAnchorPos(Vector2.zero, duration)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();
        }

        /// <summary>
        /// Slide panel out to direction
        /// </summary>
        public async UniTask SlideOut(SlideDirection direction, float duration = -1)
        {
            if (slidePanel == null) return;
            if (duration < 0) duration = defaultDuration;

            var rect = slidePanel.rectTransform;
            Vector2 endPos = GetSlideStartPosition(direction);

            await rect.DOAnchorPos(endPos, duration)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();

            slidePanel.gameObject.SetActive(false);
        }

        private Vector2 GetSlideStartPosition(SlideDirection direction)
        {
            return direction switch
            {
                SlideDirection.Left => new Vector2(-Screen.width, 0),
                SlideDirection.Right => new Vector2(Screen.width, 0),
                SlideDirection.Up => new Vector2(0, Screen.height),
                SlideDirection.Down => new Vector2(0, -Screen.height),
                _ => Vector2.zero
            };
        }

        #endregion

        #region Circle Wipe

        /// <summary>
        /// Circle wipe transition
        /// </summary>
        public async UniTask CircleWipeIn(Vector2 centerPoint, float duration = -1)
        {
            if (circleWipe == null) return;
            if (duration < 0) duration = defaultDuration;

            circleWipe.gameObject.SetActive(true);
            circleWipe.anchoredPosition = centerPoint;

            float maxSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) * 2;
            circleWipe.sizeDelta = Vector2.one * maxSize;

            await circleWipe.DOSizeDelta(Vector2.zero, duration)
                .SetEase(Ease.InQuad)
                .AsyncWaitForCompletion();
        }

        public async UniTask CircleWipeOut(Vector2 centerPoint, float duration = -1)
        {
            if (circleWipe == null) return;
            if (duration < 0) duration = defaultDuration;

            circleWipe.gameObject.SetActive(true);
            circleWipe.anchoredPosition = centerPoint;
            circleWipe.sizeDelta = Vector2.zero;

            float maxSize = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) * 2;

            await circleWipe.DOSizeDelta(Vector2.one * maxSize, duration)
                .SetEase(Ease.OutQuad)
                .AsyncWaitForCompletion();

            circleWipe.gameObject.SetActive(false);
        }

        #endregion

        public enum SlideDirection
        {
            Left,
            Right,
            Up,
            Down
        }
    }
}
