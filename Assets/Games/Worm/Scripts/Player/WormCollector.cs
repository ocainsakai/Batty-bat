using UnityEngine;
using Core.EventSystem;
using Core.Patterns.ObjectPool;
using Games.Worm.Events;
using Games.Worm.Resources;

namespace Games.Worm.Player
{
    /// <summary>
    /// Handles resource collection when worm collides with resources.
    /// Integrates with WormGrowth and ResourceInventory.
    /// </summary>
    public class WormCollector : MonoBehaviour
    {
        [Header("Collection")]
        [SerializeField] private LayerMask resourceLayer;
        [SerializeField] private float collectionCooldown = 0.1f;

        [Header("Visual Feedback")]
        [SerializeField] private ParticleSystem collectionParticles;
        [SerializeField] private AudioClip collectionSound;

        private WormGrowth _growth;
        private ResourceInventory _inventory;
        private float _lastCollectionTime;
        private AudioSource _audioSource;

        private void Awake()
        {
            _growth = GetComponent<WormGrowth>();
            _inventory = GetComponent<ResourceInventory>();
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check cooldown
            if (Time.time - _lastCollectionTime < collectionCooldown)
                return;

            // Check if it's a resource
            if (((1 << other.gameObject.layer) & resourceLayer) == 0)
                return;

            var resource = other.GetComponent<Resource>();
            if (resource != null && resource.Definition != null)
            {
                CollectResource(resource);
                _lastCollectionTime = Time.time;
            }
        }

        private void CollectResource(Resource resource)
        {
            var definition = resource.Definition;
            
            // Add to inventory
            _inventory.AddResource(definition);
            
            // Grow worm
            _growth.AddGrowth(definition.GrowthValue);
            
            // Visual feedback
            PlayCollectionEffects(resource.transform.position, definition.ResourceColor);
            
            // Publish event
            EventBus.Publish(new ResourceCollectedEvent(
                definition,
                resource.transform.position
            ));
            
            // Despawn resource (return to pool)
            string poolName = "Resource_" + definition.ResourceID;
            if (PoolManager.HasInstance)
            {
                PoolManager.Instance.Despawn(poolName, resource);
                
                // Schedule respawn
                ResourceSpawner.Instance?.ScheduleRespawn(definition);
            }
            else
            {
                // Fallback: destroy
                Destroy(resource.gameObject);
            }
        }

        private void PlayCollectionEffects(Vector3 position, Color color)
        {
            // Particles
            if (collectionParticles != null)
            {
                collectionParticles.transform.position = position;
                var main = collectionParticles.main;
                main.startColor = color;
                collectionParticles.Play();
            }

            // Sound
            if (collectionSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(collectionSound);
            }
        }
    }
}
