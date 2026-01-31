using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// A script for controlling a top-down camera in Unity. The camera follows a target object
    /// (tagged as "Character") with smooth or fixed movement and allows for adjustable lateral view angle,
    /// as well as rotation adjustments on the X and Y axes.
    /// </summary>
    [ExecuteInEditMode]
    public class TopDownCameraController : MonoBehaviour
    {
        public static TopDownCameraController Singleton { get; private set; } // Singleton instance

        /// <summary>
        /// The camera to be controlled by this script.
        /// </summary>
        public Camera targetCamera;

        /// <summary>
        /// Offset from the target's position. Adjusts the camera's height and lateral position.
        /// </summary>
        public Vector3 targetOffset;

        /// <summary>
        /// Determines if the camera should follow the target smoothly or instantly.
        /// </summary>
        public bool smoothFollow = true;

        /// <summary>
        /// The smoothing factor used for smooth following. Higher values result in slower movement.
        /// </summary>
        public float followSmoothing = 10.0f;

        /// <summary>
        /// The angle to adjust the camera's lateral view.
        /// </summary>
        public float lateralViewAngle = 0f;

        /// <summary>
        /// The angle to rotate the camera around the X axis.
        /// </summary>
        public float xRotation = 90f;

        /// <summary>
        /// The angle to rotate the camera around the Y axis.
        /// </summary>
        public float yRotation = 0f;

        /// <summary>
        /// The intensity of the camera shake effect.
        /// </summary>
        public float shakeIntensity = 0.5f;

        /// <summary>
        /// The duration of the camera shake effect in seconds.
        /// </summary>
        public float shakeDuration = 0.5f;

        /// <summary>
        /// Caches the camera transform for optimization.
        /// </summary>
        private Transform cacheCameraTransform;

        /// <summary>
        /// Reference to the target transform (the character).
        /// </summary>
        private Transform target;

        /// <summary>
        /// Stores the original camera position before shaking.
        /// </summary>
        private Vector3 originalPosition;

        /// <summary>
        /// Flag to control the camera shake coroutine.
        /// </summary>
        private bool isShaking = false;

        /// <summary>
        /// Stores the target camera position after following the character, but before shake.
        /// </summary>
        private Vector3 finalCameraPosition;

        protected virtual void Awake()
        {
            // Implement Singleton pattern
            if (Singleton == null)
            {
                Singleton = this;
            }
            else if (Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            InitializeCamera();
        }

        /// <summary>
        /// Initializes the camera reference and caches the camera's transform.
        /// </summary>
        private void InitializeCamera()
        {
            if (targetCamera == null)
                targetCamera = GetComponent<Camera>();
            cacheCameraTransform = targetCamera.transform;
            originalPosition = cacheCameraTransform.localPosition; // Cache the original position for shake
        }

        /// <summary>
        /// Finds the target object with the "Character" tag in the scene.
        /// </summary>
        public void SetTarget(Transform _target)
        {
            target = _target;
        }

        protected virtual void LateUpdate()
        {
            if (target != null)
            {
                Vector3 targetPosition = target.position + targetOffset;

                if (smoothFollow)
                {
                    finalCameraPosition = Vector3.Lerp(cacheCameraTransform.position, targetPosition, followSmoothing * Time.deltaTime);
                }
                else
                {
                    finalCameraPosition = targetPosition;
                }

                // Apply lateral view angle
                finalCameraPosition += cacheCameraTransform.right * lateralViewAngle;

                // Apply rotation to the camera
                cacheCameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

                // Ensure the camera looks down on the target
                cacheCameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

                // Set the camera's final position before applying shake
                cacheCameraTransform.position = finalCameraPosition;

                // Apply camera shake, if active
                if (isShaking)
                {
                    ApplyCameraShake();
                }
            }
        }

        /// <summary>
        /// Triggers the camera shake effect with the specified intensity and duration.
        /// </summary>
        public void TriggerCameraShake()
        {
            if (!isShaking)
            {
                StartCoroutine(CameraShakeCoroutine());
            }
        }

        /// <summary>
        /// Coroutine that handles the camera shake effect.
        /// </summary>
        /// <returns>IEnumerator for the coroutine.</returns>
        private IEnumerator CameraShakeCoroutine()
        {
            isShaking = true;
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                ApplyCameraShake();
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Reset the camera's position after shaking
            cacheCameraTransform.position = finalCameraPosition;
            isShaking = false;
        }

        /// <summary>
        /// Applies the camera shake effect to the final camera position.
        /// </summary>
        private void ApplyCameraShake()
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            randomOffset.z = 0; // Prevent shaking in the Z-axis for 2D purposes

            cacheCameraTransform.position = finalCameraPosition + randomOffset;
        }
    }
}
