using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles advanced 2D character movement using Rigidbody2D.
    /// Supports walking, dashing, and being pushed.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class AdvancedCharacterController2D : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float pushForce = 10f;
        public float rotationSpeed = 720f;
        public float dashDistance = 2f;
        public float dashDuration = 0.2f;

        private Rigidbody2D rb;
        private Vector2 moveDirection = Vector2.zero;
        private Vector2 pushDirection = Vector2.zero;
        private bool isMovementStopped = false;
        private bool isDashing = false;
        [HideInInspector] public bool CanRotateWhileStopped { get; private set; } = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }

        void FixedUpdate()
        {
            if (GameplayManager.Singleton.IsPaused() || isMovementStopped)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
            if (!isDashing)
            {
                rb.linearVelocity = moveDirection + pushDirection;
                pushDirection = Vector2.Lerp(pushDirection, Vector2.zero, Time.fixedDeltaTime * pushForce);
            }
        }

        /// <summary>
        /// Moves the character using a 2D direction.
        /// </summary>
        public void Move(Vector3 direction)
        {
            if (isMovementStopped)
            {
                moveDirection = Vector2.zero;
                return;
            }

            moveDirection = direction.normalized * moveSpeed;

            if (direction.magnitude > 0)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            }
        }

        /// <summary>
        /// Applies a pushing force.
        /// </summary>
        public void Push(Vector3 force)
        {
            pushDirection = force;
        }

        /// <summary>
        /// Returns the current movement speed.
        /// </summary>
        public float GetCurrentSpeed()
        {
            return rb.linearVelocity.magnitude;
        }

        /// <summary>
        /// Changes movement speed.
        /// </summary>
        public void AlterSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
        }

        /// <summary>
        /// Stops character movement with optional rotation.
        /// </summary>
        public void StopMovement(bool allowRotation = false)
        {
            isMovementStopped = true;
            CanRotateWhileStopped = allowRotation;
        }

        /// <summary>
        /// Resumes character movement.
        /// </summary>
        public void ResumeMovement()
        {
            isMovementStopped = false;
            CanRotateWhileStopped = false;
        }

        /// <summary>
        /// Performs a dash in the given direction.
        /// </summary>
        public IEnumerator Dash(Vector3 direction, float dashSpeed, float dashDuration)
        {
            
            isDashing = true;
            Vector2 dashDir = new Vector2(direction.x, direction.z).normalized;
            float elapsedTime = 0f;
            while (elapsedTime < dashDuration)
            {
                rb.linearVelocity = dashDir * dashSpeed;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            isDashing = false;
        }
    }
}
