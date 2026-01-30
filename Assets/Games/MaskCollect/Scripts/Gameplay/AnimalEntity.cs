using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
    /// Handles state management and help interactions.
    /// </summary>
    public class AnimalEntity : MonoBehaviour
    {
        [Header("Animal Info")]
        [SerializeField] private string animalId;
        [SerializeField] private string animalName;
        [SerializeField] private Sprite animalSprite;

        [Header("Help Configuration")]
        [SerializeField] private HelpType requiredHelpType = HelpType.Pet;
        [SerializeField] private float helpDuration = 1f;
        [SerializeField] private int clicksRequired = 1;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float leaveDuration = 1f;

        private AnimalState _currentState = AnimalState.Idle;
        private int _currentClicks = 0;

        // Events
        public event Action<AnimalEntity> OnNeedsHelp;
        public event Action<AnimalEntity> OnHelpStarted;
        public event Action<AnimalEntity> OnHelpCompleted;
        public event Action<AnimalEntity> OnAnimalLeft;

        // Properties
        public string AnimalId => animalId;
        public string AnimalName => animalName;
        public Sprite AnimalSprite => animalSprite;
        public AnimalState CurrentState => _currentState;
        public HelpType RequiredHelpType => requiredHelpType;
        public float HelpProgress => clicksRequired > 0 ? (float)_currentClicks / clicksRequired : 0f;

        protected virtual void Start()
        {
            SetState(AnimalState.NeedsHelp);
        }

        /// <summary>
        /// Called when player clicks/taps on the animal
        /// </summary>
        public virtual void OnInteract()
        {
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

        private void StartHelping()
        {
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
            
            // Wait for help duration (animation time)
            if (helpDuration > 0)
            {
                await UniTask.WaitForSeconds(helpDuration);
            }

            OnHelpCompleted?.Invoke(this);
            
            // Start leaving
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
            Destroy(gameObject);
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
            // Could play particle effects, sounds, etc.
            Debug.Log($"[AnimalEntity] Helping {animalName}: {_currentClicks}/{clicksRequired}");
        }

        protected virtual void PlayLeaveAnimation()
        {
            // Override in subclasses for custom leave animation
            if (animator != null)
            {
                animator.SetTrigger("Leave");
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
    }
}
