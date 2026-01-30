using UnityEngine;

namespace MaskCollect.Environment
{
    /// <summary>
    /// Simple camera follow script for 2D games.
    /// For more advanced features, consider using Cinemachine.
    /// </summary>
    public class SimpleCameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private bool findPlayerOnStart = true;
        [SerializeField] private string playerTag = "Player";

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new(0, 0, -10f);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private bool smoothFollow = true;

        [Header("Bounds (Optional)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 boundsMin = new(-10f, -10f);
        [SerializeField] private Vector2 boundsMax = new(10f, 10f);
        [SerializeField] private ScreenBoundaries screenBoundaries;

        [Header("Dead Zone")]
        [SerializeField] private bool useDeadZone = false;
        [SerializeField] private Vector2 deadZoneSize = new(1f, 1f);

        [Header("Look Ahead")]
        [SerializeField] private bool useLookAhead = false;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 3f;

        private Vector3 _velocity;
        private Vector3 _lookAheadOffset;
        private Vector3 _lastTargetPosition;
        private Camera _camera;

        private void Start()
        {
            _camera = GetComponent<Camera>();

            if (findPlayerOnStart && target == null)
            {
                var player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null)
            {
                _lastTargetPosition = target.position;
                // Snap to target on start
                transform.position = CalculateTargetPosition();
            }

            // Auto-setup bounds from ScreenBoundaries if available
            if (screenBoundaries != null && useBounds)
            {
                UpdateBoundsFromScreenBoundaries();
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = CalculateTargetPosition();

            if (smoothFollow)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    targetPosition,
                    ref _velocity,
                    1f / smoothSpeed
                );
            }
            else
            {
                transform.position = targetPosition;
            }

            _lastTargetPosition = target.position;
        }

        private Vector3 CalculateTargetPosition()
        {
            Vector3 targetPos = target.position + offset;

            // Dead zone check
            if (useDeadZone)
            {
                Vector3 currentPos = transform.position;
                Vector3 diff = target.position - currentPos;

                if (Mathf.Abs(diff.x) < deadZoneSize.x / 2f)
                {
                    targetPos.x = currentPos.x;
                }
                if (Mathf.Abs(diff.y) < deadZoneSize.y / 2f)
                {
                    targetPos.y = currentPos.y;
                }
            }

            // Look ahead
            if (useLookAhead)
            {
                Vector3 moveDir = (target.position - _lastTargetPosition).normalized;
                Vector3 desiredLookAhead = moveDir * lookAheadDistance;
                _lookAheadOffset = Vector3.Lerp(_lookAheadOffset, desiredLookAhead, Time.deltaTime * lookAheadSpeed);
                targetPos += _lookAheadOffset;
            }

            // Clamp to bounds
            if (useBounds)
            {
                targetPos = ClampToBounds(targetPos);
            }

            return targetPos;
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            if (_camera == null) return position;

            float camHeight = _camera.orthographicSize;
            float camWidth = camHeight * _camera.aspect;

            Vector2 min = boundsMin;
            Vector2 max = boundsMax;

            // Adjust bounds for camera size
            float clampedX = Mathf.Clamp(position.x, min.x + camWidth, max.x - camWidth);
            float clampedY = Mathf.Clamp(position.y, min.y + camHeight, max.y - camHeight);

            return new Vector3(clampedX, clampedY, position.z);
        }

        /// <summary>
        /// Update camera bounds from ScreenBoundaries component
        /// </summary>
        public void UpdateBoundsFromScreenBoundaries()
        {
            if (screenBoundaries == null) return;

            var bounds = screenBoundaries.ScreenBounds;
            boundsMin = bounds.min;
            boundsMax = bounds.max;
        }

        /// <summary>
        /// Set a new target to follow
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                _lastTargetPosition = target.position;
            }
        }

        /// <summary>
        /// Instantly snap camera to target position
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            transform.position = target.position + offset;
            _velocity = Vector3.zero;
            _lookAheadOffset = Vector3.zero;
        }

        /// <summary>
        /// Set camera bounds programmatically
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            useBounds = true;
            boundsMin = min;
            boundsMax = max;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!useBounds) return;

            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3(
                (boundsMin.x + boundsMax.x) / 2f,
                (boundsMin.y + boundsMax.y) / 2f,
                0
            );
            Vector3 size = new Vector3(
                boundsMax.x - boundsMin.x,
                boundsMax.y - boundsMin.y,
                0
            );
            Gizmos.DrawWireCube(center, size);

            // Draw dead zone
            if (useDeadZone)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneSize.x, deadZoneSize.y, 0));
            }
        }
#endif
    }
}
