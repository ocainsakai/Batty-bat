using UnityEngine;

namespace Core.Patterns.ObjectPool.Examples
{
    /// <summary>
    /// Example pooled audio source for playing one-shot sounds.
    /// Automatically despawns when audio finishes playing.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PooledAudioSource : MonoBehaviour, IPoolable
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource != null)
            {
                _audioSource.playOnAwake = false;
            }
        }

        public void OnSpawn()
        {
            // Reset audio source when spawned
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }

        public void OnDespawn()
        {
            // Stop audio when despawned
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }
        }

        /// <summary>
        /// Play an audio clip and auto-despawn when finished
        /// </summary>
        public void PlayOneShot(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null || _audioSource == null)
                return;

            transform.position = position;
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.Play();

            // Auto-despawn after clip duration
            var pooledObject = GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.DespawnAfter(clip.length);
            }
        }

        /// <summary>
        /// Play an audio clip with custom settings
        /// </summary>
        public void PlayOneShot(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            if (clip == null || _audioSource == null)
                return;

            transform.position = position;
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.pitch = pitch;
            _audioSource.Play();

            // Auto-despawn after clip duration (adjusted for pitch)
            var pooledObject = GetComponent<PooledObject>();
            if (pooledObject != null)
            {
                pooledObject.DespawnAfter(clip.length / Mathf.Abs(pitch));
            }
        }
    }
}
