using BulletHellTemplate.Core.Events;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles advanced character movement, including walking, jumping, and being pushed.
    /// Requires a CharacterController component to function correctly.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerComponent : MonoBehaviour
    {
        public float moveSpeed = 5f; // Speed at which the character moves
        public float pushForce = 10f; // Force applied when being pushed
        public float rotationSpeed = 720f; // Speed of character rotation
        public float jumpHeight = 2f; // Height the character can jump
        public float gravity = 9.81f; // Gravity applied to the character
        public float maxFallVelocity = 40f; // Maximum velocity when falling

        [SerializeField] private LayerMask teleportGroundMask = ~0; 
        [SerializeField] private float teleportGroundProbeUp = 0.5f;  
        [SerializeField] private float teleportGroundProbeDown = 5.0f;


        private CharacterController characterController; // The CharacterController component
        private Vector3 moveDirection = Vector3.zero; // Current direction of movement
        private Vector3 pushDirection = Vector3.zero; // Direction of any applied push force
        private float verticalVelocity; // Vertical velocity for gravity and jumping
        private bool isJumping = false; // Indicates if the character is currently jumping
        private bool isMovementStopped = false;


        private CharacterEntity characterOwner;
        private CharacterEntity Owner => characterOwner;
        //Unitask
        private CancellationTokenSource dashCts;
        private CancellationTokenSource knockCts;

        private bool OwnerIsStunned => characterOwner && characterOwner.IsStunned;
        [HideInInspector]public bool CanRotateWhileStopped { get; private set; } = false;
        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterOwner = GetComponent<CharacterEntity>();
        }

        void Update()
        {
            ApplyGravity();
            MoveCharacter();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerDashEvent>(OnDash);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerDashEvent>(OnDash);
        }

        /// <summary>
        /// Moves the character in a specified direction.
        /// </summary>
        /// <param name="direction">Direction vector for movement.</param>
        public void Move(Vector3 direction)
        {
            if (GameplayManager.Singleton.IsPaused())
            {
                moveDirection = Vector3.zero;
                return;
            }

            if (isMovementStopped)
            {
                if (CanRotateWhileStopped && direction.magnitude > 0)
                {
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                    transform.rotation = Quaternion.Euler(0, angle, 0);
                }
                moveDirection = Vector3.zero;
                return;
            }

            if (direction.magnitude > 0)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
                transform.rotation = Quaternion.Euler(0, angle, 0);

                moveDirection = direction.normalized * moveSpeed;
            }
            else
            {
                moveDirection = Vector3.zero;
            }
        }

        /// <summary>
        /// Makes the character jump if grounded.
        /// </summary>
        public void Jump()
        {
            if (characterController.isGrounded)
            {
                isJumping = true;
                verticalVelocity = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
        }

        /// <summary>
        /// Applies a force to push the character.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        public void Push(Vector3 force)
        {
            pushDirection = force;
        }

        /// <summary>
        /// Alters the character's movement speed.
        /// </summary>
        /// <param name="newSpeed">The new speed value.</param>
        public void AlterSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
        }

        /// <summary>
        /// Gets the current speed of the character.
        /// </summary>
        /// <returns>The magnitude of the character's movement velocity.</returns>
        public float GetCurrentSpeed()
        {
            Vector3 horizontalVelocity = new Vector3(moveDirection.x + pushDirection.x, 0, moveDirection.z + pushDirection.z);
            return horizontalVelocity.magnitude;
        }

        private void ApplyGravity()
        {
            if (characterController.isGrounded)
            {
                if (!isJumping)
                {
                    verticalVelocity = -gravity * Time.deltaTime;
                }
                else
                {
                    verticalVelocity -= gravity * Time.deltaTime;
                    isJumping = false;
                }
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
                if (verticalVelocity < -maxFallVelocity)
                {
                    verticalVelocity = -maxFallVelocity;
                }
            }
        }

        private void MoveCharacter()
        {
            if (isMovementStopped)
            {
                characterController.Move(Vector3.zero);
                return;
            }

            Vector3 velocity = moveDirection + pushDirection;
            velocity.y = verticalVelocity;

            if (GameplayManager.Singleton.IsPaused())
            {
                characterController.Move(Vector3.zero);
            }
            else
            {
                characterController.Move(velocity * Time.deltaTime);
            }

            // Gradually reduce push force over time
            pushDirection = Vector3.Lerp(pushDirection, Vector3.zero, Time.deltaTime * pushForce);
        }

        /// <summary>
        /// Stops the movement of the character temporarily, allowing optional rotation.
        /// </summary>
        /// <param name="allowRotation">Whether rotation is allowed while movement is stopped.</param>
        public void StopMovement(bool allowRotation = false)
        {
            isMovementStopped = true;
            CanRotateWhileStopped = allowRotation;
            moveSpeed = 0f;             
        }

        /// <summary>
        /// Resumes the movement of the character.
        /// </summary>
        public void ResumeMovement()
        {
            isMovementStopped = false;
            moveSpeed = Owner ? Owner.GetCurrentMoveSpeed()
                              : moveSpeed;             
        }

        private async void OnDash(PlayerDashEvent evt)
        {
            if (evt.Target != characterOwner || OwnerIsStunned) return;

            await DashAsync(evt.dir, evt.dashSpeed, evt.dashDuration, evt.dashCts);
        }

        /// <summary>
        /// Cancels any running dash coroutine, if one is active.
        /// </summary>
        public void CancelDash()
        {
            dashCts?.Cancel();
        }

        /// <summary>
        /// Applies a knock-back impulse that pushes the character away from
        /// <paramref name="from"/> by <paramref name="dist"/> units over
        /// <paramref name="dur"/> seconds. Behaves like the monster version,
        /// but uses <see cref="CharacterController"/> instead of a NavMeshAgent.
        /// </summary>
        /// <param name="from">World position the push originates from.</param>
        /// <param name="dist">Total distance to travel.</param>
        /// <param name="dur">Total time in seconds.</param>
        public void ApplyKnockback(Vector3 from, float dist, float dur)
        {
            Vector3 dir = (transform.position - from);
            dir.y = 0f;                                   // keep push on the ground plane
            if (dir.sqrMagnitude < 0.001f) dir = -transform.forward;
            dir = dir.normalized;

            knockCts?.Cancel();                           // abort previous knock-back
            knockCts = new CancellationTokenSource();
            KnockRoutine(dir, dist, dur, knockCts.Token).Forget();
        }

        /// <summary>
        /// Performs a dash in the specified direction while ensuring that the character does not pass through walls.
        /// The dash increases the movement speed temporarily, locks the movement direction, and ensures the character faces the dash direction.
        /// </summary>
        /// <param name="dashSpeed">The speed to dash at.</param>
        /// <param name="dashDuration">The duration of the dash in seconds.</param>
        /// <param name="direction">The direction vector in which to dash, typically coming from the joystick input.</param>
        private async UniTask DashAsync(Vector3 direction,
                                   float dashSpeed,
                                   float dashDuration,
                                   CancellationToken cancellationToken)
        {
            dashCts?.Cancel();
            dashCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Vector3 dashDirection = direction.normalized;

            if (dashDirection.sqrMagnitude > 0f)
            {
                float targetAngle = Mathf.Atan2(dashDirection.x, dashDirection.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            }
            float originalSpeed = Owner ? Owner.GetCurrentMoveSpeed() : moveSpeed;
            moveSpeed = 0f;                    

            float elapsed = 0f;
            try
            {
                while (elapsed < dashDuration)
                {
                    dashCts.Token.ThrowIfCancellationRequested();

                    Vector3 dashMovement = dashDirection * dashSpeed * Time.deltaTime;
                    characterController.Move(dashMovement);

                    elapsed += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, dashCts.Token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                moveSpeed = Owner ? Owner.GetCurrentMoveSpeed() : originalSpeed;
            }
        }

        /// <summary>
        /// Internal coroutine that displaces the character while game-pausing,
        /// cancels and state-restoration are handled correctly.
        /// </summary>
        private async UniTaskVoid KnockRoutine(Vector3 dir, float dist, float dur, CancellationToken token)
        {
            //   Disable normal locomotion but allow facing updates if desired
            bool prevStopped = isMovementStopped;
            StopMovement();                   // freezes MoveCharacter() loop

            float elapsed = 0f;
            try
            {
                while (elapsed < dur && !token.IsCancellationRequested)
                {
                    if (GameplayManager.Singleton.IsPaused())
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        continue;             // wait while the game is paused
                    }
                    // Distance per frame = totalDist / totalTime * Δt
                    Vector3 step = dir * (dist / dur) * Time.deltaTime;
                    characterController.Move(step);

                    elapsed += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { /* ignored */ }
            finally
            {
                if (!prevStopped) ResumeMovement();
            }
        }

        public void Teleport(Vector3 targetPosition, Quaternion targetRotation, bool snapToGround = false)
        {
            CancelDash();
            knockCts?.Cancel();

            moveDirection = Vector3.zero;
            pushDirection = Vector3.zero;
            verticalVelocity = 0f;
            isJumping = false;

            Vector3 finalPos = targetPosition;
            if (snapToGround)
            {
                Vector3 rayOrigin = targetPosition + Vector3.up * teleportGroundProbeUp;
                if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, teleportGroundProbeDown + teleportGroundProbeUp,
                                    teleportGroundMask, QueryTriggerInteraction.Ignore))
                {
                    finalPos = hit.point + Vector3.up * 0.02f; 
                }
            }

            bool wasEnabled = characterController.enabled;
            if (wasEnabled) characterController.enabled = false;

            transform.SetPositionAndRotation(finalPos, targetRotation);

            if (wasEnabled) characterController.enabled = true;

            characterController.Move(Vector3.zero);

        }

        public void Teleport(Vector3 targetPosition, bool snapToGround = false)
        {
            Teleport(targetPosition, transform.rotation, snapToGround);
        }
    }
}
