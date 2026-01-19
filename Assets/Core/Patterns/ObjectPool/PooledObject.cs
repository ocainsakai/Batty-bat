using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Core.Patterns.ObjectPool
{
    /// <summary>
    /// Helper component for pooled objects.
    /// Attach this to prefabs that will be pooled for easy despawn functionality.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        [Header("Pool Settings")]
        [Tooltip("Name of the pool this object belongs to")]
        public string PoolName;

        [Header("Auto Despawn")]
        [Tooltip("Auto-despawn after this many seconds (0 = disabled)")]
        public float AutoDespawnTime = 0f;

        private float _spawnTime;
        private CancellationTokenSource _despawnCts;

        private void OnEnable()
        {
            _spawnTime = Time.time;

            // Start auto-despawn timer if enabled
            if (AutoDespawnTime > 0f)
            {
                AutoDespawnAsync(this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private void OnDisable()
        {
            // Cancel any pending despawn
            _despawnCts?.Cancel();
            _despawnCts?.Dispose();
            _despawnCts = null;
        }

        /// <summary>
        /// Despawn this object back to its pool
        /// </summary>
        public void Despawn()
        {
            if (string.IsNullOrEmpty(PoolName))
            {
                Debug.LogWarning($"[PooledObject] {gameObject.name} has no pool name set. Cannot despawn.");
                return;
            }

            // Try to despawn using PoolManager
            if (PoolManager.HasInstance)
            {
                PoolManager.Instance.Despawn(PoolName, this);
            }
            else
            {
                // Fallback: just deactivate
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Despawn this object after a delay using UniTask
        /// </summary>
        public async void DespawnAfter(float delay)
        {
            // Cancel previous despawn if any
            _despawnCts?.Cancel();
            _despawnCts?.Dispose();
            _despawnCts = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(delay),
                    cancellationToken: _despawnCts.Token
                );

                Despawn();
            }
            catch (System.OperationCanceledException)
            {
                // Despawn was cancelled - this is expected
            }
            finally
            {
                _despawnCts?.Dispose();
                _despawnCts = null;
            }
        }

        private async UniTaskVoid AutoDespawnAsync(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(AutoDespawnTime),
                    cancellationToken: cancellationToken
                );

                Despawn();
            }
            catch (System.OperationCanceledException)
            {
                // Auto-despawn was cancelled - this is expected
            }
        }

        /// <summary>
        /// Get how long this object has been alive since spawn
        /// </summary>
        public float GetLifetime()
        {
            return Time.time - _spawnTime;
        }

        private void OnDestroy()
        {
            _despawnCts?.Cancel();
            _despawnCts?.Dispose();
            _despawnCts = null;
        }
    }
}
