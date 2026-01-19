using UnityEngine;
using Core.Patterns.ObjectPool;
using Games.Worm.Data;

namespace Games.Worm.Resources
{
    /// <summary>
    /// MonoBehaviour component for a collectible resource.
    /// Pooled for performance.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Resource : MonoBehaviour, IPoolable
    {
        public ResourceDefinition Definition { get; private set; }

        private SpriteRenderer _renderer;
        private CircleCollider2D _collider;
        private Transform _transform;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
            _transform = transform;

            // Setup collider as trigger
            _collider.isTrigger = true;
        }

        /// <summary>
        /// Initialize resource with definition
        /// </summary>
        public void Initialize(ResourceDefinition definition)
        {
            Definition = definition;
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (Definition == null || _renderer == null)
                return;

            // Apply color
            _renderer.color = Definition.ResourceColor;

            // Apply sprite (if provided, otherwise use default circle)
            if (Definition.ResourceSprite != null)
            {
                _renderer.sprite = Definition.ResourceSprite;
            }

            // Apply size
            float scale = Definition.Size;
            _transform.localScale = Vector3.one * scale;

            // Update collider radius
            if (_collider != null)
            {
                _collider.radius = 0.5f; // Sprite is 1 unit, so 0.5 radius
            }
        }

        public void OnSpawn()
        {
            // Reset state when spawned from pool
            gameObject.SetActive(true);
            
            if (_renderer != null)
            {
                _renderer.enabled = true;
            }
        }

        public void OnDespawn()
        {
            // Cleanup when returned to pool
            gameObject.SetActive(false);
            
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }
    }
}
