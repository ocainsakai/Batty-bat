using DentedPixel;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Plays various hit feedback animations using LeanTween on a target transform.
    /// Supports shake, pulse, squash, and custom slime-style animations.
    /// </summary>
    [DisallowMultipleComponent]
    public class HitAnimationTween : MonoBehaviour
    {
        public enum HitAnimationType
        {
            None,
            Shake,
            Pulse,
            Squash,
            SlimeBounce
        }

        [Header("Target")]
        [Tooltip("If empty, will use this GameObject's transform.")]
        public Transform targetTransform;

        [Header("Animation Type")]
        public HitAnimationType hitAnimationType = HitAnimationType.Shake;

        [Header("General Settings")]
        public float animationDuration = 0.2f;

        [Header("Shake Settings")]
        public float shakeStrength = 0.35f;
        public int shakeVibrato = 12;

        [Header("Pulse Settings")]
        public float pulseScale = 1.15f;

        [Header("Squash Settings")]
        public float squashScaleX = 1.25f;
        public float squashScaleY = 0.75f;

        [Header("Slime Bounce Settings")]
        public float slimeSquashScaleY = 0.6f;
        public float slimeBounceScaleY = 1.20f;
        public float slimeDuration = 0.16f;

        private LTDescr currentTween;
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private bool isAnimating = false;

        private void Awake()
        {
            if (targetTransform == null)
                targetTransform = transform;

            originalScale = targetTransform.localScale;
            originalPosition = targetTransform.localPosition;
        }

        /// <summary>
        /// Triggers the selected hit animation, but ignores new calls if an animation is already running.
        /// </summary>
        public void PlayHitAnimation()
        {
            if (isAnimating)
                return;

            isAnimating = true;

            // Cancela para garantir que o objeto volte ao estado original se interrompido
            if (currentTween != null && LeanTween.isTweening(targetTransform.gameObject))
                LeanTween.cancel(targetTransform.gameObject);

            switch (hitAnimationType)
            {
                case HitAnimationType.Shake:
                    Shake();
                    break;
                case HitAnimationType.Pulse:
                    Pulse();
                    break;
                case HitAnimationType.Squash:
                    Squash();
                    break;
                case HitAnimationType.SlimeBounce:
                    SlimeBounce();
                    break;
            }
        }

        /// <summary>
        /// Plays a shake animation on the target transform.
        /// </summary>
        private void Shake()
        {
            targetTransform.localPosition = originalPosition;
            currentTween = LeanTween.moveLocal(targetTransform.gameObject,
                    originalPosition + (Vector3)Random.insideUnitCircle * shakeStrength,
                    animationDuration * 0.5f)
                .setEasePunch()
                .setOnComplete(() =>
                {
                    targetTransform.localPosition = originalPosition;
                    isAnimating = false;
                });
        }



        /// <summary>
        /// Plays a pulse (scale up and down) animation.
        /// </summary>
        private void Pulse()
        {
            targetTransform.localScale = originalScale;
            currentTween = LeanTween.scale(targetTransform.gameObject, originalScale * pulseScale, animationDuration * 0.5f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.scale(targetTransform.gameObject, originalScale, animationDuration * 0.5f)
                        .setEase(LeanTweenType.easeInQuad)
                        .setOnComplete(() => isAnimating = false);
                });
        }

        /// <summary>
        /// Plays a squash and stretch animation (like classic cartoon hit).
        /// </summary>
        private void Squash()
        {
            targetTransform.localScale = originalScale;
            currentTween = LeanTween.scale(targetTransform.gameObject, new Vector3(originalScale.x * squashScaleX, originalScale.y * squashScaleY, originalScale.z), animationDuration * 0.5f)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnComplete(() =>
                {
                    LeanTween.scale(targetTransform.gameObject, originalScale, animationDuration * 0.5f)
                        .setEase(LeanTweenType.easeInCubic)
                        .setOnComplete(() => isAnimating = false);
                });
        }

        /// <summary>
        /// Plays a slime-style squash and bounce animation.
        /// </summary>
        private void SlimeBounce()
        {
            targetTransform.localScale = originalScale;
            LeanTween.scaleY(targetTransform.gameObject, originalScale.y * slimeSquashScaleY, slimeDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() =>
                {
                    LeanTween.scaleY(targetTransform.gameObject, originalScale.y * slimeBounceScaleY, slimeDuration)
                        .setEase(LeanTweenType.easeOutElastic)
                        .setOnComplete(() =>
                        {
                            LeanTween.scaleY(targetTransform.gameObject, originalScale.y, slimeDuration * 0.8f)
                                .setEase(LeanTweenType.easeOutBounce)
                                .setOnComplete(() => isAnimating = false);
                        });
                });
        }

    }
}