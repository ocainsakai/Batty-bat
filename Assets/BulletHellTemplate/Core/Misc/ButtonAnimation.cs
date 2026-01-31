using DentedPixel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BulletHellTemplate
{
    /// <summary>
    /// EnhancedButtonAnimation provides advanced button interaction animations such as scale, position shift, fade, shake, and a looping pulse effect.
    /// Attach this script to a UI button or any UI element to animate hover, press, and release interactions.
    /// </summary>
    public class EnhancedButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Enum for selecting the axis along which to move the UI element.
        /// </summary>
        public enum MovementAxis
        {
            None,
            Horizontal,
            Vertical,
            Both
        }

        [Header("Button Transform Settings")]
        /// <summary>
        /// The transform of the button to animate.
        /// </summary>
        [Tooltip("The transform of the button to animate.")]
        public Transform buttonTransform;

        /// <summary>
        /// Optional CanvasGroup for fade animations. If not assigned, it will be added automatically.
        /// </summary>
        [Tooltip("Optional CanvasGroup for fade animations. If not assigned, it will be added automatically.")]
        public CanvasGroup canvasGroup;

        private Vector3 originalScale;
        private Vector3 originalPosition;

        [Header("Scale Animation Settings")]
        /// <summary>
        /// Scale factor applied when the button is hovered.
        /// </summary>
        [Tooltip("Scale factor applied when the button is hovered.")]
        public float hoverScaleFactor = 1.05f;

        /// <summary>
        /// Scale factor applied when the button is pressed.
        /// </summary>
        [Tooltip("Scale factor applied when the button is pressed.")]
        public float pressScaleFactor = 0.95f;

        /// <summary>
        /// Duration for scale animations.
        /// </summary>
        [Tooltip("Duration for scale animations.")]
        public float scaleAnimationDuration = 0.1f;

        [Header("Position Shift Settings")]
        /// <summary>
        /// Axis along which the button will shift on hover.
        /// </summary>
        [Tooltip("Axis along which the button will shift on hover.")]
        public MovementAxis hoverMovementAxis = MovementAxis.None;

        /// <summary>
        /// Distance in pixels for the position shift on hover.
        /// </summary>
        [Tooltip("Distance in pixels for the position shift on hover.")]
        public float hoverMoveDistance = 10f;

        [Header("Fade Animation Settings")]
        /// <summary>
        /// If true, the button will fade when hovered.
        /// </summary>
        [Tooltip("If true, the button will fade when hovered.")]
        public bool applyFadeOnHover = false;

        /// <summary>
        /// Target alpha for the fade effect on hover.
        /// </summary>
        [Tooltip("Target alpha for the fade effect on hover.")]
        public float hoverFadeAlpha = 0.8f;

        /// <summary>
        /// Duration for fade animations.
        /// </summary>
        [Tooltip("Duration for fade animations.")]
        public float fadeAnimationDuration = 0.1f;

        [Header("Shake Animation Settings")]
        /// <summary>
        /// If true, a shake effect will be applied when the button is clicked.
        /// </summary>
        [Tooltip("If true, a shake effect will be applied when the button is clicked.")]
        public bool applyShakeOnClick = false;

        /// <summary>
        /// Duration of the shake effect.
        /// </summary>
        [Tooltip("Duration of the shake effect.")]
        public float shakeDuration = 0.2f;

        /// <summary>
        /// Strength of the shake effect in pixels.
        /// </summary>
        [Tooltip("Strength of the shake effect in pixels.")]
        public float shakeStrength = 10f;

        [Header("Loop Animation Settings (Pulse)")]
        /// <summary>
        /// If true, the button will continuously pulse (scale up and down).
        /// </summary>
        [Tooltip("If true, the button will continuously pulse (scale up and down).")]
        public bool enablePulseLoop = false;

        /// <summary>
        /// Scale factor for the pulse loop.
        /// </summary>
        [Tooltip("Scale factor for the pulse loop.")]
        public float pulseScaleFactor = 1.1f;

        /// <summary>
        /// Duration for one pulse cycle.
        /// </summary>
        [Tooltip("Duration for one pulse cycle.")]
        public float pulseDuration = 0.5f;

        private void Awake()
        {
            if (buttonTransform == null)
            {
                buttonTransform = transform;
            }
            originalScale = buttonTransform.localScale;
            originalPosition = buttonTransform.localPosition;

            if (applyFadeOnHover && canvasGroup == null)
            {
                canvasGroup = buttonTransform.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = buttonTransform.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            // Optionally start a pulse loop animation
            if (enablePulseLoop)
            {
                StartPulseLoop();
            }
        }

        /// <summary>
        /// Starts a continuous pulse animation on the button.
        /// </summary>
        public void StartPulseLoop()
        {
            LeanTween.scale(buttonTransform.gameObject, originalScale * pulseScaleFactor, pulseDuration)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong();
        }

        /// <summary>
        /// Called when the pointer enters the button area.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Scale animation on hover
            LeanTween.scale(buttonTransform.gameObject, originalScale * hoverScaleFactor, scaleAnimationDuration)
                .setEase(LeanTweenType.easeOutQuad);

            // Position shift on hover based on selected axis
            Vector3 targetPos = originalPosition;
            switch (hoverMovementAxis)
            {
                case MovementAxis.Horizontal:
                    targetPos.x += hoverMoveDistance;
                    break;
                case MovementAxis.Vertical:
                    targetPos.y += hoverMoveDistance;
                    break;
                case MovementAxis.Both:
                    targetPos += new Vector3(hoverMoveDistance, hoverMoveDistance, 0f);
                    break;
                default:
                    break;
            }
            LeanTween.moveLocal(buttonTransform.gameObject, targetPos, scaleAnimationDuration)
                .setEase(LeanTweenType.easeOutQuad);

            // Fade effect on hover
            if (applyFadeOnHover && canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, hoverFadeAlpha, fadeAnimationDuration)
                    .setEase(LeanTweenType.easeOutQuad);
            }
        }

        /// <summary>
        /// Called when the pointer exits the button area.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            // Revert scale animation
            LeanTween.scale(buttonTransform.gameObject, originalScale, scaleAnimationDuration)
                .setEase(LeanTweenType.easeOutQuad);

            // Revert position shift
            LeanTween.moveLocal(buttonTransform.gameObject, originalPosition, scaleAnimationDuration)
                .setEase(LeanTweenType.easeOutQuad);

            // Revert fade effect
            if (applyFadeOnHover && canvasGroup != null)
            {
                LeanTween.alphaCanvas(canvasGroup, 1f, fadeAnimationDuration)
                    .setEase(LeanTweenType.easeOutQuad);
            }
        }

        /// <summary>
        /// Called when the pointer is pressed on the button.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            LeanTween.scale(buttonTransform.gameObject, originalScale * pressScaleFactor, scaleAnimationDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }

        /// <summary>
        /// Called when the pointer is released from the button.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            LeanTween.scale(buttonTransform.gameObject, originalScale, scaleAnimationDuration)
                .setEase(LeanTweenType.easeOutQuad);

            // Optionally apply a shake effect on click release
            if (applyShakeOnClick)
            {
                ApplyShakeEffect();
            }
        }

        /// <summary>
        /// Applies a shake effect to the button.
        /// </summary>
        private void ApplyShakeEffect()
        {
            Vector3 currentPos = buttonTransform.localPosition;
            LeanTween.value(gameObject, 0f, 1f, shakeDuration)
                .setOnUpdate((float val) =>
                {
                    // Generate a random offset inside a circle
                    Vector2 shakeOffset = Random.insideUnitCircle * shakeStrength;
                    buttonTransform.localPosition = currentPos + new Vector3(shakeOffset.x, shakeOffset.y, 0f);
                })
                .setOnComplete(() =>
                {
                    buttonTransform.localPosition = currentPos;
                });
        }

        /// <summary>
        /// Ensures all LeanTween animations on the button are canceled when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (buttonTransform != null)
            {
                LeanTween.cancel(buttonTransform.gameObject);
            }
        }
    }
}
