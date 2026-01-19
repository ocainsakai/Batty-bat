using UnityEngine;

namespace Core.Patterns.ObjectPool.Examples
{
    /// <summary>
    /// Example pooled projectile (bullet, arrow, etc.)
    /// Demonstrates IPoolable implementation and auto-despawn.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PooledProjectile : MonoBehaviour, IPoolable
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private int damage = 10;

        [Header("Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private string hitEffectPoolName = "HitEffect";

        private Rigidbody _rigidbody;
        private float _spawnTime;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void OnSpawn()
        {
            // Reset state when spawned from pool
            _spawnTime = Time.time;
            
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }

            // Launch projectile forward
            Launch(transform.forward);
        }

        public void OnDespawn()
        {
            // Cleanup when returned to pool
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void Launch(Vector3 direction)
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = direction.normalized * speed;
            }
        }

        private void Update()
        {
            // Auto-despawn after lifetime
            if (Time.time - _spawnTime >= lifetime)
            {
                Despawn();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Spawn hit effect
            if (hitEffectPrefab != null && PoolManager.HasInstance)
            {
                var effect = PoolManager.Instance.Spawn<ParticleSystem>(
                    hitEffectPoolName,
                    transform.position,
                    Quaternion.identity
                );

                if (effect != null)
                {
                    PoolManager.Instance.DespawnAfter(hitEffectPoolName, effect, 2f);
                }
            }

            // Apply damage (example)
            var damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            // Despawn projectile
            Despawn();
        }

        private void Despawn()
        {
            // Get pool name from PooledObject component if exists
            var pooledObject = GetComponent<PooledObject>();
            if (pooledObject != null && !string.IsNullOrEmpty(pooledObject.PoolName))
            {
                pooledObject.Despawn();
            }
            else
            {
                // Fallback: just deactivate
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Example interface for damageable objects
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }
}
