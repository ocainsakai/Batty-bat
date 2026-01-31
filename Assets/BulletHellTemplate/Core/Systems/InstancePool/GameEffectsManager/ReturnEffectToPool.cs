using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Returns this GameObject to GameEffectsManager after a delay.
    /// </summary>
    public sealed class ReturnEffectToPool : MonoBehaviour
    {
        [Tooltip("Seconds before the effect is recycled.")]
        public float Delay = 1f;

        private void OnEnable()
        {
            // fire-and-forget task
            AutoReturnAsync().Forget();
        }

        private async UniTaskVoid AutoReturnAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(Delay), ignoreTimeScale: false);
            GameEffectsManager.ReleaseEffect(gameObject);
        }
    }
}
