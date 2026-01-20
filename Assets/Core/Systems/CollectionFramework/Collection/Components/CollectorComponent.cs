using UnityEngine;
using Core.EventSystem;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// Component for collecting collectables via trigger collision.
    /// Implements ICollector interface.
    /// </summary>
    public class CollectorComponent : MonoBehaviour, ICollector
    {
        [Header("Collection Settings")]
        [SerializeField] private LayerMask collectableLayer;
        [SerializeField] private float collectionCooldown = 0.1f;

        [Header("Feedback")]
        [SerializeField] private ParticleSystem collectionParticles;
        [SerializeField] private AudioClip collectionSound;

        private CollectableInventory _inventory;
        private AudioSource _audioSource;
        private float _lastCollectionTime;

        public GameObject GameObject => gameObject;

        private void Awake()
        {
            _inventory = GetComponent<CollectableInventory>();
            _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[CollectorComponent] OnTriggerEnter2D with: {other.gameObject.name}");
            
            // Check cooldown
            if (Time.time - _lastCollectionTime < collectionCooldown)
            {
                Debug.Log("[CollectorComponent] Cooldown active, skipping");
                return;
            }

            // Check layer
            if (((1 << other.gameObject.layer) & collectableLayer) == 0)
            {
                Debug.Log($"[CollectorComponent] Layer mismatch. Object layer: {other.gameObject.layer}, collectableLayer mask: {collectableLayer.value}");
                return;
            }

            var collectable = other.GetComponent<ICollectable>();
            Debug.Log($"[CollectorComponent] ICollectable found: {collectable != null}");
            
            if (collectable != null && CanCollect(collectable))
            {
                Debug.Log("[CollectorComponent] Calling Collect");
                Collect(collectable);
                _lastCollectionTime = Time.time;
            }
        }

        public void Collect(ICollectable collectable)
        {
            if (collectable == null || collectable.Definition == null)
                return;

            // Check if can be collected
            if (!collectable.CanBeCollected(gameObject))
                return;

            var definition = collectable.Definition;

            // Add to inventory
            if (_inventory != null)
            {
                _inventory.Add(definition);
            }

            // Visual/audio feedback
            PlayFeedback(collectable);

            // Notify collectable
            collectable.OnCollected(gameObject);

            // Publish event
            EventBus.Publish(new CollectableCollectedEvent(definition, transform.position, gameObject));
        }

        public bool CanCollect(ICollectable collectable)
        {
            return collectable != null && collectable.Definition != null;
        }

        private void PlayFeedback(ICollectable collectable)
        {
            var definition = collectable.Definition;

            // Particles
            if (collectionParticles != null)
            {
                var main = collectionParticles.main;
                main.startColor = definition.CollectibleColor;
                collectionParticles.Play();
            }

            // Sound
            AudioClip sound = collectionSound ?? definition.CollectionSound;
            if (sound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sound);
            }

            // Effect prefab
            if (definition.CollectionEffect != null)
            {
                Instantiate(definition.CollectionEffect, transform.position, Quaternion.identity);
            }
        }
    }

    #region Events

    public class CollectableCollectedEvent : IEvent
    {
        public CollectibleDefinition Definition { get; set; }
        public Vector3 Position { get; set; }
        public GameObject Collector { get; set; }

        public CollectableCollectedEvent(CollectibleDefinition definition, Vector3 position, GameObject collector)
        {
            Definition = definition;
            Position = position;
            Collector = collector;
        }
    }

    #endregion
}
