using UnityEngine;
using Core.EventSystem;
using Games.Worm.Events;
using Cysharp.Threading.Tasks;

namespace Games.Worm.Player
{
    /// <summary>
    /// Manages worm growth based on collected resources.
    /// Handles size scaling, collision updates, and growth animations.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class WormGrowth : MonoBehaviour
    {
        [Header("Growth Settings")]
        [SerializeField] private float baseRadius = 0.5f;
        [SerializeField] private float growthRate = 0.05f;
        [SerializeField] private float maxRadius = 10f;
        
        [Header("Animation")]
        [SerializeField] private float growthAnimationSpeed = 3f;
        [SerializeField] private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem growthParticles;
        [SerializeField] private float particleEmissionThreshold = 0.5f;

        private float _currentRadius;
        private float _targetRadius;
        private CircleCollider2D _collider;
        private SpriteRenderer _renderer;
        private float _lastParticleEmission;

        public float CurrentRadius => _currentRadius;
        public float TargetRadius => _targetRadius;
        public float GrowthProgress => _currentRadius / maxRadius;
        public bool IsMaxSize => _currentRadius >= maxRadius;

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            
            _currentRadius = baseRadius;
            _targetRadius = baseRadius;
            UpdateSize(true);
        }

        /// <summary>
        /// Add growth to the worm
        /// </summary>
        public void AddGrowth(int value)
        {
            if (IsMaxSize)
            {
                Debug.Log("[WormGrowth] Already at max size!");
                return;
            }

            float oldRadius = _currentRadius;
            _targetRadius = Mathf.Min(_targetRadius + (value * growthRate), maxRadius);
            
            // Emit particles if growth is significant
            if (_targetRadius - _currentRadius >= particleEmissionThreshold)
            {
                EmitGrowthParticles();
            }

            // Publish event
            EventBus.Publish(new WormGrowthChangedEvent(
                oldRadius,
                _targetRadius,
                value
            ));

            Debug.Log($"[WormGrowth] Grew from {oldRadius:F2} to {_targetRadius:F2} (+{value})");
        }

        /// <summary>
        /// Set radius directly (for loading saved game)
        /// </summary>
        public void SetRadius(float radius)
        {
            _currentRadius = Mathf.Clamp(radius, baseRadius, maxRadius);
            _targetRadius = _currentRadius;
            UpdateSize(true);
        }

        private void Update()
        {
            // Smooth growth animation
            if (!Mathf.Approximately(_currentRadius, _targetRadius))
            {
                float t = growthAnimationSpeed * Time.deltaTime;
                float curveValue = growthCurve.Evaluate(t);
                
                _currentRadius = Mathf.Lerp(
                    _currentRadius,
                    _targetRadius,
                    curveValue
                );
                
                UpdateSize(false);
            }
        }

        private void UpdateSize(bool immediate)
        {
            // Update visual scale
            float scale = _currentRadius * 2f;
            if (immediate)
            {
                transform.localScale = Vector3.one * scale;
            }
            else
            {
                transform.localScale = Vector3.one * scale;
            }

            // Update collider
            if (_collider != null)
            {
                _collider.radius = _currentRadius;
            }
        }

        private void EmitGrowthParticles()
        {
            if (growthParticles != null && Time.time - _lastParticleEmission > 0.2f)
            {
                growthParticles.Play();
                _lastParticleEmission = Time.time;
            }
        }

        /// <summary>
        /// Get growth data for saving
        /// </summary>
        public WormGrowthData GetGrowthData()
        {
            return new WormGrowthData
            {
                CurrentRadius = _currentRadius,
                TargetRadius = _targetRadius
            };
        }
    }

    [System.Serializable]
    public struct WormGrowthData
    {
        public float CurrentRadius;
        public float TargetRadius;
    }
}
