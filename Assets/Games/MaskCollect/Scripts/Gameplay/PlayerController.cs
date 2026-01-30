using UnityEngine;
using UnityEngine.InputSystem;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Player movement controller supporting both keyboard (WASD) and touch joystick.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 15f;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private string horizontalParam = "Horizontal";
        [SerializeField] private string verticalParam = "Vertical";
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private bool flipSpriteOnDirection = true;

        [Header("Input")]
        [SerializeField] private bool useNewInputSystem = true;
        [SerializeField] private FloatingJoystick joystick; // For mobile

        [Header("Boundaries")]
        [SerializeField] private bool clampToScreen = true;
        [SerializeField] private MaskCollect.Environment.ScreenBoundaries screenBoundaries;

        private Rigidbody2D _rb;
        private Vector2 _moveInput;
        private Vector2 _currentVelocity;
        private bool _isMoving;

        // Input actions (New Input System)
        private InputAction _moveAction;

        public Vector2 MoveInput => _moveInput;
        public bool IsMoving => _isMoving;
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            SetupInput();
        }

        private void SetupInput()
        {
            if (useNewInputSystem)
            {
                var playerInput = GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    _moveAction = playerInput.actions["Move"];
                }
                else
                {
                    // Create default move action
                    _moveAction = new InputAction("Move", InputActionType.Value);
                    _moveAction.AddCompositeBinding("2DVector")
                        .With("Up", "<Keyboard>/w")
                        .With("Down", "<Keyboard>/s")
                        .With("Left", "<Keyboard>/a")
                        .With("Right", "<Keyboard>/d");
                    _moveAction.AddCompositeBinding("2DVector")
                        .With("Up", "<Keyboard>/upArrow")
                        .With("Down", "<Keyboard>/downArrow")
                        .With("Left", "<Keyboard>/leftArrow")
                        .With("Right", "<Keyboard>/rightArrow");
                    _moveAction.Enable();
                }
            }
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
        }

        private void Update()
        {
            ReadInput();
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            Move();
            ClampPosition();
        }

        private void ReadInput()
        {
            _moveInput = Vector2.zero;

            // Read from New Input System
            if (useNewInputSystem && _moveAction != null)
            {
                _moveInput = _moveAction.ReadValue<Vector2>();
            }

            // Read from joystick (mobile)
            if (joystick != null)
            {
                Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
                if (joystickInput.sqrMagnitude > 0.01f)
                {
                    _moveInput = joystickInput;
                }
            }

            // Legacy input fallback
            if (_moveInput == Vector2.zero)
            {
                _moveInput = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                );
            }

            // Normalize if magnitude > 1
            if (_moveInput.sqrMagnitude > 1f)
            {
                _moveInput.Normalize();
            }

            _isMoving = _moveInput.sqrMagnitude > 0.01f;
        }

        private void Move()
        {
            Vector2 targetVelocity = _moveInput * moveSpeed;

            float accel = _isMoving ? acceleration : deceleration;
            _currentVelocity = Vector2.MoveTowards(_currentVelocity, targetVelocity, accel * Time.fixedDeltaTime);

            _rb.linearVelocity = _currentVelocity;
        }

        private void ClampPosition()
        {
            if (!clampToScreen) return;

            if (screenBoundaries != null)
            {
                Vector2 clampedPos = screenBoundaries.ClampToBounds(transform.position);
                transform.position = new Vector3(clampedPos.x, clampedPos.y, transform.position.z);
            }
        }

        private void UpdateAnimation()
        {
            if (animator == null) return;

            // Set animator parameters
            animator.SetFloat(speedParam, _currentVelocity.magnitude);
            
            if (_isMoving)
            {
                animator.SetFloat(horizontalParam, _moveInput.x);
                animator.SetFloat(verticalParam, _moveInput.y);
            }

            // Flip sprite based on horizontal direction
            if (flipSpriteOnDirection && spriteRenderer != null && _moveInput.x != 0)
            {
                spriteRenderer.flipX = _moveInput.x < 0;
            }
        }

        /// <summary>
        /// Stop all movement
        /// </summary>
        public void Stop()
        {
            _moveInput = Vector2.zero;
            _currentVelocity = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
        }

        /// <summary>
        /// Set joystick reference (for runtime setup)
        /// </summary>
        public void SetJoystick(FloatingJoystick joystick)
        {
            this.joystick = joystick;
        }

        /// <summary>
        /// Enable/disable player input
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            if (enabled)
            {
                _moveAction?.Enable();
            }
            else
            {
                _moveAction?.Disable();
                Stop();
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_isMoving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, _moveInput * 2f);
            }
        }
#endif
    }
}
