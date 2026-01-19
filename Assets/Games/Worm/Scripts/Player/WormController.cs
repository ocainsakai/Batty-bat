using UnityEngine;
using Core.EventSystem;
using Games.Worm.Events;

namespace Games.Worm.Player
{
    /// <summary>
    /// Controls worm movement based on player input.
    /// Supports keyboard/gamepad input with smooth acceleration.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class WormController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float deceleration = 8f;

        [Header("Input")]
        [SerializeField] private bool useMouseControl = false;

        private Rigidbody2D _rigidbody;
        private Vector2 _moveInput;
        private Vector2 _currentVelocity;
        private WormGrowth _growth;
        private Camera _mainCamera;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _growth = GetComponent<WormGrowth>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleInput();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void HandleInput()
        {
            if (useMouseControl)
            {
                // Mouse/touch control - move towards cursor
                Vector3 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (mousePos - transform.position).normalized;
                _moveInput = direction;
            }
            else
            {
                // Keyboard/gamepad control
                _moveInput = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                ).normalized;
            }
        }

        private void Move()
        {
            Vector2 targetVelocity = _moveInput * moveSpeed;
            
            // Smooth acceleration/deceleration
            float accel = _moveInput.magnitude > 0 ? acceleration : deceleration;
            _currentVelocity = Vector2.Lerp(
                _currentVelocity,
                targetVelocity,
                accel * Time.fixedDeltaTime
            );

            _rigidbody.linearVelocity = _currentVelocity;
        }

        /// <summary>
        /// Set movement speed (can be modified by power-ups)
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// Get current movement direction
        /// </summary>
        public Vector2 GetMoveDirection()
        {
            return _moveInput;
        }
    }
}
