using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace MaskCollect.Environment
{
    /// <summary>
    /// Simple AI that makes an object wander around randomly within a defined area.
    /// Good for making animals feel alive in the scene.
    /// </summary>
    public class SimpleWander : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float minWaitTime = 1f;
        [SerializeField] private float maxWaitTime = 3f;
        [SerializeField] private float minMoveDistance = 1f;
        [SerializeField] private float maxMoveDistance = 3f;

        [Header("Wander Area")]
        [SerializeField] private bool useLocalBounds = true;
        [SerializeField] private Vector2 wanderAreaSize = new(5f, 5f);
        [SerializeField] private Vector2 wanderAreaCenter = Vector2.zero;
        [SerializeField] private ScreenBoundaries screenBoundaries; // Optional: use screen bounds

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string moveParameterName = "IsMoving";
        [SerializeField] private string horizontalParameterName = "Horizontal";
        [SerializeField] private string verticalParameterName = "Vertical";
        [SerializeField] private bool flipSpriteOnDirection = true;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("State")]
        [SerializeField] private bool startWandering = true;
        [SerializeField] private bool pauseWhenInteracting = true;

        private Vector2 _startPosition;
        private Vector2 _targetPosition;
        private bool _isMoving = false;
        private bool _isWandering = false;
        private bool _isPaused = false;
        private CancellationTokenSource _cts;

        public bool IsMoving => _isMoving;
        public bool IsWandering => _isWandering;
        public bool IsPaused => _isPaused;

        private void Start()
        {
            _startPosition = transform.position;
            
            if (useLocalBounds)
            {
                wanderAreaCenter = _startPosition;
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (startWandering)
            {
                StartWandering();
            }
        }

        private void OnDestroy()
        {
            StopWandering();
        }

        /// <summary>
        /// Start the wandering behavior
        /// </summary>
        public void StartWandering()
        {
            if (_isWandering) return;

            _isWandering = true;
            _cts = new CancellationTokenSource();
            WanderLoop(_cts.Token).Forget();
        }

        /// <summary>
        /// Stop the wandering behavior
        /// </summary>
        public void StopWandering()
        {
            _isWandering = false;
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            SetMovingState(false);
        }

        /// <summary>
        /// Temporarily pause wandering (e.g., when player is interacting)
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
            SetMovingState(false);
        }

        /// <summary>
        /// Resume wandering after pause
        /// </summary>
        public void Resume()
        {
            _isPaused = false;
        }

        private async UniTaskVoid WanderLoop(CancellationToken ct)
        {
            while (_isWandering && !ct.IsCancellationRequested)
            {
                // Wait for random time
                float waitTime = Random.Range(minWaitTime, maxWaitTime);
                await UniTask.WaitForSeconds(waitTime, cancellationToken: ct);

                if (ct.IsCancellationRequested) break;
                if (_isPaused)
                {
                    await UniTask.WaitUntil(() => !_isPaused, cancellationToken: ct);
                }

                // Pick random destination
                _targetPosition = GetRandomDestination();

                // Move to destination
                await MoveToPosition(_targetPosition, ct);
            }
        }

        private Vector2 GetRandomDestination()
        {
            Vector2 currentPos = transform.position;
            
            // Random direction
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minMoveDistance, maxMoveDistance);
            
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            Vector2 targetPos = currentPos + offset;

            // Clamp to wander area
            if (screenBoundaries != null)
            {
                targetPos = screenBoundaries.ClampToBounds(targetPos);
            }
            else
            {
                Vector2 min = wanderAreaCenter - wanderAreaSize / 2f;
                Vector2 max = wanderAreaCenter + wanderAreaSize / 2f;
                
                targetPos = new Vector2(
                    Mathf.Clamp(targetPos.x, min.x, max.x),
                    Mathf.Clamp(targetPos.y, min.y, max.y)
                );
            }

            return targetPos;
        }

        private async UniTask MoveToPosition(Vector2 target, CancellationToken ct)
        {
            _isMoving = true;
            SetMovingState(true);

            Vector2 startPos = transform.position;
            Vector2 direction = (target - startPos).normalized;

            // Update sprite direction
            UpdateSpriteDirection(direction);

            float distance = Vector2.Distance(startPos, target);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration && !ct.IsCancellationRequested && !_isPaused)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                transform.position = Vector2.Lerp(startPos, target, t);
                
                await UniTask.Yield(ct);
            }

            if (!ct.IsCancellationRequested && !_isPaused)
            {
                transform.position = target;
            }

            _isMoving = false;
            SetMovingState(false);
        }

        private void SetMovingState(bool isMoving)
        {
            if (animator != null && !string.IsNullOrEmpty(moveParameterName))
            {
                animator.SetBool(moveParameterName, isMoving);
            }
        }

        private void UpdateSpriteDirection(Vector2 direction)
        {
            // Update animator parameters
            if (animator != null)
            {
                if (!string.IsNullOrEmpty(horizontalParameterName))
                {
                    animator.SetFloat(horizontalParameterName, direction.x);
                }
                if (!string.IsNullOrEmpty(verticalParameterName))
                {
                    animator.SetFloat(verticalParameterName, direction.y);
                }
            }

            // Flip sprite based on horizontal direction
            if (flipSpriteOnDirection && spriteRenderer != null)
            {
                if (direction.x != 0)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
            }
        }

        /// <summary>
        /// Move to a specific position (e.g., when called by another system)
        /// </summary>
        public async UniTask MoveTo(Vector2 position)
        {
            Pause();
            await MoveToPosition(position, destroyCancellationToken);
            Resume();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 center = useLocalBounds ? (Vector2)transform.position : wanderAreaCenter;
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(center, new Vector3(wanderAreaSize.x, wanderAreaSize.y, 0.1f));
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(center, new Vector3(wanderAreaSize.x, wanderAreaSize.y, 0.1f));

            // Draw current target if moving
            if (_isMoving)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _targetPosition);
                Gizmos.DrawWireSphere(_targetPosition, 0.2f);
            }
        }
#endif
    }
}
