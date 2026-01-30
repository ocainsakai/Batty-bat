using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Handles click/tap interaction for helping animals.
    /// Attach this to animal GameObjects with a Collider2D.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AnimalQuest : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("References")]
        [SerializeField] private AnimalEntity animalEntity;
        [SerializeField] private MaskData rewardMask; // The mask this animal gives

        [Header("Visual Feedback")]
        [SerializeField] private GameObject helpIndicator; // UI showing "Help me!" 
        [SerializeField] private GameObject progressIndicator; // Progress bar/circles
        [SerializeField] private ParticleSystem helpParticles;
        [SerializeField] private ParticleSystem completeParticles;

        [Header("Settings")]
        [SerializeField] private bool useHoldToHelp = false;
        [SerializeField] private float holdHelpRate = 2f; // Clicks per second when holding

        private bool _isHolding = false;
        private float _holdTimer = 0f;

        public MaskData RewardMask => rewardMask;

        private void Awake()
        {
            if (animalEntity == null)
            {
                animalEntity = GetComponent<AnimalEntity>();
            }
        }

        private void OnEnable()
        {
            if (animalEntity != null)
            {
                animalEntity.OnNeedsHelp += HandleNeedsHelp;
                animalEntity.OnHelpStarted += HandleHelpStarted;
                animalEntity.OnHelpCompleted += HandleHelpCompleted;
            }
        }

        private void OnDisable()
        {
            if (animalEntity != null)
            {
                animalEntity.OnNeedsHelp -= HandleNeedsHelp;
                animalEntity.OnHelpStarted -= HandleHelpStarted;
                animalEntity.OnHelpCompleted -= HandleHelpCompleted;
            }
        }

        private void Update()
        {
            if (useHoldToHelp && _isHolding)
            {
                _holdTimer += Time.deltaTime;
                float interval = 1f / holdHelpRate;
                
                while (_holdTimer >= interval)
                {
                    _holdTimer -= interval;
                    animalEntity?.OnInteract();
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!useHoldToHelp)
            {
                HandleClick();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (useHoldToHelp)
            {
                _isHolding = true;
                _holdTimer = 0f;
                // Immediate first interaction
                HandleClick();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isHolding = false;
        }

        private void HandleClick()
        {
            if (animalEntity == null) return;
            
            // Play click feedback
            PlayClickFeedback();
            
            // Tell entity about interaction
            animalEntity.OnInteract();
        }

        private void PlayClickFeedback()
        {
            if (helpParticles != null)
            {
                helpParticles.Play();
            }

            // Optional: Scale punch animation
            DOTween.Kill(gameObject);
            transform.localScale = Vector3.one;
            transform.DOScale(Vector3.one * 1.1f, 0.1f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    transform.DOScale(Vector3.one, 0.1f)
                        .SetEase(Ease.InQuad);
                });
        }

        private void HandleNeedsHelp(AnimalEntity animal)
        {
            if (helpIndicator != null)
            {
                helpIndicator.SetActive(true);
            }
        }

        private void HandleHelpStarted(AnimalEntity animal)
        {
            if (helpIndicator != null)
            {
                helpIndicator.SetActive(false);
            }
            if (progressIndicator != null)
            {
                progressIndicator.SetActive(true);
            }
        }

        private void HandleHelpCompleted(AnimalEntity animal)
        {
            if (progressIndicator != null)
            {
                progressIndicator.SetActive(false);
            }
            if (completeParticles != null)
            {
                completeParticles.Play();
            }

            // Trigger reward
            RewardSystem.Instance?.GiveReward(rewardMask);
        }

        // For 2D physics-based clicking (alternative to EventSystem)
        private void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; // Let UI handle it
            }

            if (useHoldToHelp)
            {
                _isHolding = true;
                _holdTimer = 0f;
            }
            HandleClick();
        }

        private void OnMouseUp()
        {
            _isHolding = false;
        }
    }
}
