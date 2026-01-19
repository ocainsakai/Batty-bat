using UnityEngine;

namespace Core.Patterns.ObjectPool.Examples
{
    /// <summary>
    /// Example pooled particle system.
    /// Automatically despawns when particle system finishes playing.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class PooledParticle : MonoBehaviour, IPoolable
    {
        private ParticleSystem _particleSystem;
        private float _duration;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem != null)
            {
                _duration = _particleSystem.main.duration + _particleSystem.main.startLifetime.constantMax;
            }
        }

        public void OnSpawn()
        {
            // Play particle system when spawned
            if (_particleSystem != null)
            {
                _particleSystem.Clear();
                _particleSystem.Play();
            }

            // Auto-despawn after duration
            var pooledObject = GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.DespawnAfter(_duration);
            }
        }

        public void OnDespawn()
        {
            // Stop particle system when despawned
            if (_particleSystem != null)
            {
                _particleSystem.Stop();
                _particleSystem.Clear();
            }
        }

        /// <summary>
        /// Play the particle system with custom settings
        /// </summary>
        public void Play(Vector3 position, Quaternion rotation, float scale = 1f)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = Vector3.one * scale;

            if (_particleSystem != null)
            {
                _particleSystem.Play();
            }
        }
    }
}
