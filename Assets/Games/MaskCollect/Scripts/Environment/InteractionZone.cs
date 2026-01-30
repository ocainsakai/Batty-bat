using System;
using UnityEngine;

namespace MaskCollect.Environment
{
    /// <summary>
    /// Detects when the player enters/exits an interaction zone.
    /// Use this for animals to know when player is nearby.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractionZone : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private bool useTag = true;
        [SerializeField] private bool useLayer = false;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject interactionPrompt; // "Press E" or similar
        [SerializeField] private bool showPromptWhenInRange = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private Collider2D _collider;
        private bool _playerInRange = false;
        private GameObject _playerInZone;

        // Events
        public event Action<GameObject> OnPlayerEnter;
        public event Action<GameObject> OnPlayerExit;
        public event Action OnInteract;

        public bool PlayerInRange => _playerInRange;
        public GameObject PlayerObject => _playerInZone;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            
            // Ensure it's a trigger
            if (!_collider.isTrigger)
            {
                _collider.isTrigger = true;
            }

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayer(other.gameObject)) return;

            _playerInRange = true;
            _playerInZone = other.gameObject;

            if (showPromptWhenInRange && interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }

            OnPlayerEnter?.Invoke(other.gameObject);

            if (showDebugLogs)
            {
                Debug.Log($"[InteractionZone] Player entered zone: {gameObject.name}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayer(other.gameObject)) return;

            _playerInRange = false;
            _playerInZone = null;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            OnPlayerExit?.Invoke(other.gameObject);

            if (showDebugLogs)
            {
                Debug.Log($"[InteractionZone] Player exited zone: {gameObject.name}");
            }
        }

        private bool IsPlayer(GameObject obj)
        {
            if (useTag && obj.CompareTag(playerTag))
            {
                return true;
            }

            if (useLayer && ((1 << obj.layer) & playerLayer) != 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Call this when player presses interact button while in range
        /// </summary>
        public void TriggerInteraction()
        {
            if (!_playerInRange) return;

            OnInteract?.Invoke();

            if (showDebugLogs)
            {
                Debug.Log($"[InteractionZone] Interaction triggered: {gameObject.name}");
            }
        }

        /// <summary>
        /// Set the interaction zone radius (for CircleCollider2D)
        /// </summary>
        public void SetRadius(float radius)
        {
            if (_collider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = radius;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = _playerInRange ? Color.green : Color.yellow;

            if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            else if (col is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
        }
#endif
    }
}
