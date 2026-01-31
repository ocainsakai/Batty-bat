using DentedPixel;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// EnhancedUpgradeIndicatorAnimation provides various animation options for upgrade indicators using LeanTween.
    /// It supports movement along a configurable axis, fade effects, and loop options.
    /// Attach this script to a GameObject with a RectTransform component.
    /// </summary>
    public class EnhancedUpgradeIndicatorAnimation : MonoBehaviour
    {
        /// <summary>
        /// Enum for selecting the axis along which the UI element moves.
        /// </summary>
        public enum MovementAxis
        {
            None,
            Horizontal,
            Vertical,
            Both
        }

        #region Indicator Settings

        /// <summary>
        /// The RectTransform of the UI element to animate.
        /// </summary>
        [Tooltip("The RectTransform of the UI element to animate.")]
        public RectTransform indicatorRect;

        /// <summary>
        /// If true, the animation will start automatically on Start.
        /// </summary>
        [Tooltip("If true, the animation will start automatically on Start.")]
        public bool animateOnStart = true;

        #endregion

        #region Movement Settings

        /// <summary>
        /// Select the movement axis for the animation.
        /// </summary>
        [Header("Movement Settings")]
        [Tooltip("Select the movement axis for the animation.")]
        public MovementAxis movementAxis = MovementAxis.Vertical;

        /// <summary>
        /// The distance in pixels the UI element will move.
        /// </summary>
        [Tooltip("The distance in pixels the UI element will move.")]
        public float moveDistance = 20f;

        /// <summary>
        /// Duration of the movement animation in seconds.
        /// </summary>
        [Tooltip("Duration of the movement animation in seconds.")]
        public float animationDuration = 0.5f;

        /// <summary>
        /// Easing type for the movement tween.
        /// </summary>
        [Tooltip("Easing type for the movement tween.")]
        public LeanTweenType movementEase = LeanTweenType.easeInOutSine;

        /// <summary>
        /// If true, the animation will loop indefinitely.
        /// </summary>
        [Tooltip("If true, the animation will loop indefinitely.")]
        public bool loopAnimation = true;

        /// <summary>
        /// If true, the animation will use a ping-pong loop (back and forth).
        /// </summary>
        [Tooltip("If true, the animation will use a ping-pong loop (back and forth).")]
        public bool usePingPongLoop = true;

        #endregion

        #region Fade Settings

        /// <summary>
        /// If true, a fade effect will be applied concurrently with the movement animation.
        /// </summary>
        [Header("Fade Settings")]
        [Tooltip("If true, a fade effect will be applied concurrently with the movement animation.")]
        public bool applyFade = false;

        /// <summary>
        /// Starting alpha for the fade effect.
        /// </summary>
        [Tooltip("Starting alpha for the fade effect.")]
        public float fadeFromAlpha = 0f;

        /// <summary>
        /// Target alpha for the fade effect.
        /// </summary>
        [Tooltip("Target alpha for the fade effect.")]
        public float fadeToAlpha = 1f;

        #endregion

        private Vector2 originalAnchoredPosition;
        private CanvasGroup canvasGroup;

        /// <summary>
        /// Initializes the UI element and prepares fade if needed.
        /// </summary>
        private void OnEnable()
        {
            if (indicatorRect == null)
            {
                indicatorRect = GetComponent<RectTransform>();
            }
            originalAnchoredPosition = indicatorRect.anchoredPosition;

            if (applyFade)
            {
                canvasGroup = indicatorRect.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = indicatorRect.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = fadeFromAlpha;
            }

            if (animateOnStart)
            {
                StartAnimation();
            }
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        /// <summary>
        /// Starts the upgrade indicator animation with movement and optional fade effect.
        /// </summary>
        public void StartAnimation()
        {
            // Calculate target position based on selected movement axis
            Vector2 targetPos = originalAnchoredPosition;
            switch (movementAxis)
            {
                case MovementAxis.Horizontal:
                    targetPos.x += moveDistance;
                    break;
                case MovementAxis.Vertical:
                    targetPos.y += moveDistance;
                    break;
                case MovementAxis.Both:
                    targetPos += new Vector2(moveDistance, moveDistance);
                    break;
                default:
                    break;
            }

            // Apply movement tween if movement is enabled
            if (movementAxis != MovementAxis.None)
            {
                LTDescr tween = LeanTween.move(indicatorRect, new Vector3(targetPos.x, targetPos.y, 0f), animationDuration)
                    .setEase(movementEase);

                if (loopAnimation)
                {
                    if (usePingPongLoop)
                    {
                        tween.setLoopPingPong();
                    }
                    else
                    {
                        tween.setLoopClamp();
                    }
                }
            }

            // Apply fade tween concurrently if enabled
            if (applyFade && canvasGroup != null)
            {
                LTDescr fadeTween = LeanTween.alphaCanvas(canvasGroup, fadeToAlpha, animationDuration)
                    .setEase(LeanTweenType.easeInOutSine);

                if (loopAnimation)
                {
                    if (usePingPongLoop)
                    {
                        fadeTween.setLoopPingPong();
                    }
                    else
                    {
                        fadeTween.setLoopClamp();
                    }
                }
            }
        }

        /// <summary>
        /// Stops the upgrade indicator animation.
        /// </summary>
        public void StopAnimation()
        {
            LeanTween.cancel(indicatorRect.gameObject);
        }
    }
}
