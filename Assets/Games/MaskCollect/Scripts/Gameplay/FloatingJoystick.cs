using UnityEngine;
using UnityEngine.EventSystems;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Floating joystick for mobile touch input.
    /// The joystick appears where the player touches.
    /// </summary>
    public class FloatingJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private float handleRange = 1f;
        [SerializeField] private float deadZone = 0.1f;
        [SerializeField] private bool hideWhenInactive = true;
        [SerializeField] private float fadeSpeed = 10f;

        private Vector2 _input = Vector2.zero;
        private bool _isActive = false;

        public float Horizontal => _input.x;
        public float Vertical => _input.y;
        public Vector2 Direction => _input;
        public bool IsActive => _isActive;

        private void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (hideWhenInactive && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            // Smooth fade
            if (canvasGroup != null && hideWhenInactive)
            {
                float targetAlpha = _isActive ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isActive = true;

            // Move background to touch position
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
            {
                background.anchoredPosition = localPoint;
            }

            // Reset handle
            handle.anchoredPosition = Vector2.zero;

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 position))
            {
                return;
            }

            // Calculate position relative to background size
            Vector2 sizeDelta = background.sizeDelta;
            position = new Vector2(
                position.x / (sizeDelta.x / 2f),
                position.y / (sizeDelta.y / 2f)
            );

            // Clamp to circle
            if (position.magnitude > 1f)
            {
                position = position.normalized;
            }

            // Apply dead zone
            if (position.magnitude < deadZone)
            {
                _input = Vector2.zero;
            }
            else
            {
                _input = position;
            }

            // Move handle
            handle.anchoredPosition = _input * (sizeDelta.x / 2f) * handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isActive = false;
            _input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }
    }
}
