using UnityEngine;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Simple parallax scrolling component for background layers.
    /// Moves slower/faster relative to camera movement.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("How fast this layer scrolls relative to camera (0 = static, 1 = moves with camera)")]
        [Range(0f, 1f)]
        public float scrollSpeed = 0.5f;

        [Tooltip("Enable vertical parallax as well")]
        public bool verticalParallax = false;

        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;
        private Vector3 _startPosition;

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                _lastCameraPosition = _cameraTransform.position;
                _startPosition = transform.position;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
            
            float xOffset = deltaMovement.x * scrollSpeed;
            float yOffset = verticalParallax ? deltaMovement.y * scrollSpeed : 0f;
            
            transform.position += new Vector3(xOffset, yOffset, 0);
            _lastCameraPosition = _cameraTransform.position;
        }

        /// <summary>
        /// Reset parallax to starting position.
        /// </summary>
        public void ResetPosition()
        {
            transform.position = _startPosition;
            if (_cameraTransform != null)
            {
                _lastCameraPosition = _cameraTransform.position;
            }
        }
    }
}
