using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MaskCollect.Data;

namespace MaskCollect.Gameplay
{
    public enum AnimalState
    {
        Idle,
        NeedsHelp,
        BeingHelped,
        Helped,
        Leaving
    }

    public enum HelpType
    {
        Feed,       // Give food
        Heal,       // Heal injury
        FindItem,   // Find lost item
        Pet,        // Simple interaction - just pet/comfort
        Play        // Play with the animal
    }

    /// <summary>
    /// Base class for all animals in the game.
    /// Handles state management, help interactions, movement and mask reveal.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AnimalEntity : MonoBehaviour
    {
        [Header("Animal Info")]
        [SerializeField] private string animalId;
        [SerializeField] private string animalName;
        [SerializeField] private Sprite animalSprite;
        [SerializeField] private Sprite happySprite;

        [Header("Mask Data")]
        [SerializeField] private MaskData associatedMask;
        [SerializeField] private float maskRevealDelay = 0.5f;
        [SerializeField] private bool hasBeenCollected = false;

        [Header("Movement")]
        [SerializeField] private bool canMove = true;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float wanderRadius = 3f;
        [SerializeField] private float wanderInterval = 3f;

        [Header("Help Configuration")]
        [SerializeField] private HelpType requiredHelpType = HelpType.Pet;
        [SerializeField] private float helpDuration = 1f;
        [SerializeField] private int clicksRequired = 1;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float leaveDuration = 1f;

        private AnimalState _currentState = AnimalState.Idle;
        private int _currentClicks = 0;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _wanderTimer;
        private bool _isRevealing = false;
        private bool _facingRight = true;

        // Events
        public event Action<AnimalEntity> OnNeedsHelp;
        public event Action<AnimalEntity> OnHelpStarted;
        public event Action<AnimalEntity> OnHelpCompleted;
        public event Action<AnimalEntity> OnAnimalLeft;
        public event Action<AnimalEntity, MaskData> OnMaskRevealed;

        // Properties
        public string AnimalId => animalId;
        public string AnimalName => animalName;
        public Sprite AnimalSprite => animalSprite;
        public MaskData AssociatedMask => associatedMask;
        public bool HasBeenCollected => hasBeenCollected;
        public AnimalState CurrentState => _currentState;
        public HelpType RequiredHelpType => requiredHelpType;
        public float HelpProgress => clicksRequired > 0 ? (float)_currentClicks / clicksRequired : 0f;

        protected virtual void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            _startPosition = transform.position;
            _targetPosition = _startPosition;
        }

        protected virtual void Start()
        {
            _wanderTimer = wanderInterval;
            SetState(AnimalState.NeedsHelp);
        }

        protected virtual void Update()
        {
            if (!canMove || _isRevealing || hasBeenCollected) return;
            if (_currentState == AnimalState.BeingHelped || _currentState == AnimalState.Helped) return;

            // Wander behavior
            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0)
            {
                PickNewWanderTarget();
                _wanderTimer = wanderInterval + UnityEngine.Random.Range(-1f, 1f);
            }

            // Move towards target
            if (Vector3.Distance(transform.position, _targetPosition) > 0.1f)
            {
                Vector3 direction = (_targetPosition - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;

                // Flip sprite
                if (direction.x > 0 && !_facingRight)
                    Flip();
                else if (direction.x < 0 && _facingRight)
                    Flip();
            }
        }

        private void PickNewWanderTarget()
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * wanderRadius;
            _targetPosition = _startPosition + new Vector3(randomOffset.x, 0, 0);
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        /// <summary>
        /// Called when player clicks/taps on the animal
        /// </summary>
        public virtual void OnInteract()
        {
            if (_isRevealing || hasBeenCollected) return;

            switch (_currentState)
            {
                case AnimalState.NeedsHelp:
                    StartHelping();
                    break;
                case AnimalState.BeingHelped:
                    ContinueHelping();
                    break;
            }
        }

        /// <summary>
        /// Called when player taps/clicks on this animal (mouse input).
        /// </summary>
        private void OnMouseDown()
        {
            OnInteract();
        }

        private void StartHelping()
        {
            canMove = false;
            SetState(AnimalState.BeingHelped);
            _currentClicks = 0;
            OnHelpStarted?.Invoke(this);
            ContinueHelping();
        }

        private void ContinueHelping()
        {
            _currentClicks++;
            
            // Play feedback animation/sound here
            PlayHelpFeedback();

            if (_currentClicks >= clicksRequired)
            {
                CompleteHelp().Forget();
            }
        }

        private async UniTaskVoid CompleteHelp()
        {
            SetState(AnimalState.Helped);
            
            // Show happy sprite
            if (happySprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = happySprite;
            }

            // Wait for help duration (animation time)
            if (helpDuration > 0)
            {
                await UniTask.WaitForSeconds(helpDuration);
            }

            OnHelpCompleted?.Invoke(this);
            
            // Reveal mask if assigned
            if (associatedMask != null)
            {
                await RevealMask();
            }
            else
            {
                // No mask, just leave
                await Leave();
            }
        }

        private async UniTask RevealMask()
        {
            _isRevealing = true;

            await UniTask.WaitForSeconds(maskRevealDelay);

            Debug.Log($"[AnimalEntity] {animalName} revealed mask: {associatedMask.MaskName}");
            OnMaskRevealed?.Invoke(this, associatedMask);

            // Notify inventory manager
            var inventory = FindObjectOfType<InventoryManager>();
            if (inventory != null)
            {
                inventory.CollectMask(associatedMask);
            }

            hasBeenCollected = true;
            _isRevealing = false;

            // Leave after revealing mask
            await Leave();
        }

        private async UniTask Leave()
        {
            SetState(AnimalState.Leaving);
            
            // Play leave animation
            PlayLeaveAnimation();
            
            await UniTask.WaitForSeconds(leaveDuration);
            
            OnAnimalLeft?.Invoke(this);
            
            // Destroy or return to pool
            gameObject.SetActive(false);
        }

        protected virtual void SetState(AnimalState newState)
        {
            _currentState = newState;
            
            // Trigger animator if available
            if (animator != null)
            {
                animator.SetInteger("State", (int)newState);
            }

            if (newState == AnimalState.NeedsHelp)
            {
                OnNeedsHelp?.Invoke(this);
            }
        }

        protected virtual void PlayHelpFeedback()
        {
            // Override in subclasses for specific feedback
            Debug.Log($"[AnimalEntity] Helping {animalName}: {_currentClicks}/{clicksRequired}");
        }

        protected virtual void PlayLeaveAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Leave");
            }
        }

        /// <summary>
        /// Setup this animal with mask data.
        /// </summary>
        public void Setup(MaskData mask, Sprite sprite = null)
        {
            associatedMask = mask;
            if (mask != null)
            {
                animalName = mask.AssociatedAnimal;
                animalId = mask.MaskId;
            }

            if (sprite != null && spriteRenderer != null)
            {
                animalSprite = sprite;
                spriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// Reset animal state (for respawning).
        /// </summary>
        public void ResetAnimal()
        {
            hasBeenCollected = false;
            _isRevealing = false;
            canMove = true;
            _currentClicks = 0;
            transform.position = _startPosition;
            gameObject.SetActive(true);
            SetState(AnimalState.NeedsHelp);

            if (animalSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = animalSprite;
            }
        }

        /// <summary>
        /// Force complete the help (for debugging)
        /// </summary>
        [ContextMenu("Debug - Force Complete")]
        public void ForceComplete()
        {
            if (_currentState == AnimalState.NeedsHelp || _currentState == AnimalState.BeingHelped)
            {
                _currentClicks = clicksRequired;
                CompleteHelp().Forget();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 center = Application.isPlaying ? _startPosition : transform.position;
            Gizmos.DrawWireSphere(center, wanderRadius);
        }
    }
}
