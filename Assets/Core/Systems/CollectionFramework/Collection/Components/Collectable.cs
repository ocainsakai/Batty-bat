using UnityEngine;
using Core.Patterns.ObjectPool;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// MonoBehaviour component for a collectable item.
    /// Implements ICollectable and IPoolable for object pooling.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectable : MonoBehaviour, ICollectable, IPoolable
    {
        public CollectibleDefinition Definition { get; private set; }

        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private Transform _transform;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _transform = transform;

            // Setup collider as trigger
            _collider.isTrigger = true;
        }

        /// <summary>
        /// Initialize collectable with definition
        /// </summary>
        public void Initialize(CollectibleDefinition definition)
        {
            Definition = definition;
            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (Definition == null || _renderer == null)
                return;

            // Apply color
            _renderer.color = Definition.CollectibleColor;

            // Apply sprite
            if (Definition.CollectibleSprite != null)
            {
                _renderer.sprite = Definition.CollectibleSprite;
            }

            // Apply size
            float scale = Definition.Size;
            _transform.localScale = Vector3.one * scale;
        }

        public void OnCollected(GameObject collector)
        {
            // Override in derived classes for custom behavior
            Debug.Log($"[Collectable] {Definition.CollectibleName} collected by {collector.name}");
        }

        public virtual bool CanBeCollected(GameObject collector)
        {
            // Override in derived classes for custom logic
            return true;
        }

        #region IPoolable Implementation

        public void OnSpawn()
        {
            gameObject.SetActive(true);
            
            if (_renderer != null)
            {
                _renderer.enabled = true;
            }
        }

        public void OnDespawn()
        {
            gameObject.SetActive(false);
            
            if (_renderer != null)
            {
                _renderer.enabled = false;
            }
        }

        #endregion
    }
}
