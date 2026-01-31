using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Controls a 2D top-down camera that follows a target tagged as "Character".
    /// Includes smoothing, zoom control, bounds restriction and camera shake.
    /// </summary>
    [ExecuteInEditMode]
    public class TopDown2DCameraController : MonoBehaviour
    {
        public static TopDown2DCameraController Instance { get; private set; }

        [Header("Camera Target")]
        [Tooltip("The camera will follow this target. If left null, it will search for GameObject with tag 'Character'.")]
        public Transform target;

        [Header("Camera Settings")]
        [Tooltip("Camera offset relative to the target.")]
        public Vector3 targetOffset = new Vector3(0f, 0f, -10f);

        [Tooltip("Enable smooth camera following.")]
        public bool smoothFollow = true;

        [Tooltip("Smoothing speed (higher = slower).")]
        public float followSmoothing = 10f;

        [Tooltip("Orthographic size (zoom) of the camera.")]
        public float cameraZoom = 5f;

        [Header("Bounds Restriction")]
        [Tooltip("Restrict camera movement within these world bounds (optional).")]
        public bool limitCameraBounds = false;
        public Vector2 minBounds;
        public Vector2 maxBounds;

        [Header("Camera Shake")]
        [Tooltip("Shake intensity for camera shake effect.")]
        public float shakeIntensity = 0.5f;

        [Tooltip("Duration of the shake in seconds.")]
        public float shakeDuration = 0.5f;

        private Camera cam;
        private Vector3 targetPosition;
        private Vector3 finalCameraPosition;
        private Vector3 shakeOffset;
        private bool isShaking = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (target == null)
                target = GameObject.FindWithTag("Character")?.transform;

            if (target == null || cam == null)
                return;

            cam.orthographicSize = cameraZoom;
            targetPosition = target.position + targetOffset;

            if (smoothFollow)
                finalCameraPosition = Vector3.Lerp(transform.position, targetPosition, followSmoothing * Time.deltaTime);
            else
                finalCameraPosition = targetPosition;

            // Apply shake offset
            finalCameraPosition += isShaking ? shakeOffset : Vector3.zero;

            // Apply bounds
            if (limitCameraBounds)
            {
                float camHeight = cam.orthographicSize;
                float camWidth = camHeight * cam.aspect;

                finalCameraPosition.x = Mathf.Clamp(finalCameraPosition.x, minBounds.x + camWidth, maxBounds.x - camWidth);
                finalCameraPosition.y = Mathf.Clamp(finalCameraPosition.y, minBounds.y + camHeight, maxBounds.y - camHeight);
            }

            // Only X and Y are affected in 2D
            transform.position = new Vector3(finalCameraPosition.x, finalCameraPosition.y, targetOffset.z);
        }

        /// <summary>
        /// Starts a camera shake with the default intensity and duration.
        /// </summary>
        public void TriggerCameraShake()
        {
            if (!isShaking)
                StartCoroutine(CameraShakeCoroutine());
        }

        private IEnumerator CameraShakeCoroutine()
        {
            isShaking = true;
            float timer = 0f;

            while (timer < shakeDuration)
            {
                shakeOffset = (Vector3)(Random.insideUnitCircle * shakeIntensity);
                timer += Time.deltaTime;
                yield return null;
            }

            shakeOffset = Vector3.zero;
            isShaking = false;
        }
    }
}
