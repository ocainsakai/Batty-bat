using UnityEngine;
using UnityEngine.EventSystems;

namespace BulletHellTemplate
{
    public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform joystickTransform; // Transform do joystick handle
        [SerializeField] private RectTransform backgroundTransform; // Transform do joystick background

        private Vector2 inputVector;
        public float handleLimit = 1.0f;

        public float Horizontal => inputVector.x;
        public float Vertical => inputVector.y;

        private void Start()
        {
            if (backgroundTransform == null)
                backgroundTransform = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 direction = eventData.position - RectTransformUtility.WorldToScreenPoint(null, backgroundTransform.position);
            inputVector = (direction.magnitude > backgroundTransform.sizeDelta.x / 2.0f)
                ? direction.normalized
                : direction / (backgroundTransform.sizeDelta.x / 2.0f);

            joystickTransform.anchoredPosition = (inputVector * backgroundTransform.sizeDelta.x / 2.0f) * handleLimit;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            joystickTransform.anchoredPosition = Vector2.zero;
        }
    }
}
