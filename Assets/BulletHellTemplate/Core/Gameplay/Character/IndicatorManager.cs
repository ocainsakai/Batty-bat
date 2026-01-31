using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages range indicators for a top-down game. Handles circle, arrow, cone, AoE, etc.
    /// </summary>
    [ExecuteAlways]
    public class IndicatorManager : MonoBehaviour
    {
        [Header("Circle Indicators")]
        public SpriteRenderer circleRangeIndicator;   // Large circle for max range
        public SpriteRenderer castCircleIndicator;    // Animated cast circle
        public SpriteRenderer damageIndicator;        // Position circle for AoE center
        public SpriteRenderer castDamageIndicator;    // Animated AoE cast circle

        [Header("Arrow Indicators")]
        public SpriteRenderer arrowIndicator;
        public SpriteRenderer castArrowIndicator;

        [Header("Cone Indicators")]
        public SpriteRenderer coneIndicator;
        public SpriteRenderer castConeIndicator;

        [Header("Extra")]
        public SpriteRenderer aoeCurveLine; // Optional curve line from character to AoE center

        public bool showCircleGizmo = false;
        public bool showArrowGizmo = false;
        public bool showConeGizmo = false;
        public Vector3 arrowRotation = Vector3.zero;
        public Vector3 arrowSize = Vector3.one;
        public Vector3 coneRotation = Vector3.zero;
        public Vector3 coneSize = Vector3.one;

        /// <summary>
        /// Shows a large circle representing max range.
        /// </summary>
        /// <param name="radius">Maximum radius for the skill.</param>
        public void ShowCircleRangeIndicator(float radius)
        {
            if (circleRangeIndicator != null)
            {
                circleRangeIndicator.gameObject.SetActive(true);
                circleRangeIndicator.transform.localScale = new Vector3(radius, radius, 1f);
            }
        }

        /// <summary>
        /// Shows the smaller circle representing the aimed AoE position, used for RadialAoE in real-time.
        /// </summary>
        /// <param name="radius">Radius for the AoE at the aimed position.</param>
        /// <param name="worldPosition">Where to place the circle in the world.</param>
        public void ShowPositionAoECircle(float radius, Vector3 worldPosition)
        {
            if (damageIndicator != null)
            {
                damageIndicator.gameObject.SetActive(true);
                damageIndicator.transform.position = worldPosition;
                damageIndicator.transform.localScale = new Vector3(radius, radius, 1f);
            }
        }

        /// <summary>
        /// Hides the smaller position AoE circle.
        /// </summary>
        public void HidePositionAoECircle()
        {
            if (damageIndicator != null)
            {
                damageIndicator.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Starts the cast circle animation from 0 to radius, then closes indicators.
        /// </summary>
        public void StartCastCircleIndicator(float radius, float castDuration)
        {
            if (castCircleIndicator != null)
            {
                castCircleIndicator.gameObject.SetActive(true);
                castCircleIndicator.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateCastCircleIndicator(castCircleIndicator, radius, castDuration));
            }
        }

        /// <summary>
        /// Starts the cast damage AoE animation from 0 to radius, then closes indicators.
        /// </summary>
        public void StartCastDamageIndicator(float radius, float castDuration)
        {
            if (castDamageIndicator != null)
            {
                castDamageIndicator.gameObject.SetActive(true);
                castDamageIndicator.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateCastCircleIndicator(castDamageIndicator, radius, castDuration));
            }
        }

        private IEnumerator AnimateCastCircleIndicator(SpriteRenderer target, float targetRadius, float duration)
        {
            float elapsed = 0f;
            Vector3 initialScale = Vector3.zero;
            Vector3 finalScale = new Vector3(targetRadius, targetRadius, 1f);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                target.transform.localScale = Vector3.Lerp(initialScale, finalScale, t);
                yield return null;
            }
            target.transform.localScale = finalScale;
            yield return new WaitForSeconds(0.1f);
            CloseIndicators();
        }

        /// <summary>
        /// Shows an arrow indicator. Rotation/scale handled externally.
        /// </summary>
        public void ShowArrowIndicator()
        {
            if (arrowIndicator != null)
            {
                arrowIndicator.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Sets arrow position and rotation in top-down (y-axis rotation).
        /// </summary>
        public void UpdateArrowIndicator(Vector3 position, float distance, float maxRange, Quaternion rotation)
        {
            if (arrowIndicator == null) return;

            arrowIndicator.transform.position = position;

            // scale factor: distance / maxRange
            float factor = Mathf.Clamp01(distance / maxRange);
            // assume arrowIndicator default scale is the "max" size
            arrowIndicator.transform.localScale = factor * Vector3.one;

            // only rotate on Y axis
            arrowIndicator.transform.rotation = rotation;
        }

        /// <summary>
        /// Starts the cast arrow animation from 0 to full scale.
        /// </summary>
        public void StartCastArrowIndicator(Vector3 finalSize, float castDuration)
        {
            if (castArrowIndicator != null)
            {
                castArrowIndicator.gameObject.SetActive(true);
                castArrowIndicator.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateCastArrowConeIndicator(castArrowIndicator, finalSize, castDuration));
            }
        }

        private IEnumerator AnimateCastArrowConeIndicator(SpriteRenderer rend, Vector3 targetSize, float duration)
        {
            float elapsed = 0f;
            Vector3 initialScale = Vector3.zero;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                rend.transform.localScale = Vector3.Lerp(initialScale, targetSize, t);
                yield return null;
            }
            rend.transform.localScale = targetSize;
            yield return new WaitForSeconds(0.1f);
            CloseIndicators();
        }

        /// <summary>
        /// Shows the cone indicator. Rotation/scale handled externally.
        /// </summary>
        public void ShowConeIndicator()
        {
            if (coneIndicator != null)
            {
                coneIndicator.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Updates cone position/rotation in top-down.
        /// </summary>
        public void UpdateConeIndicator(Vector3 position, float distance, float maxRange, Quaternion rotation)
        {
            if (coneIndicator == null) return;

            coneIndicator.transform.position = position;

            float factor = Mathf.Clamp01(distance / maxRange);
            coneIndicator.transform.localScale = factor * Vector3.one;

            coneIndicator.transform.rotation = rotation;
        }

        public void StartCastConeIndicator(Vector3 finalSize, float castDuration)
        {
            if (castConeIndicator != null)
            {
                castConeIndicator.gameObject.SetActive(true);
                castConeIndicator.transform.localScale = Vector3.zero;
                StartCoroutine(AnimateCastArrowConeIndicator(castConeIndicator, finalSize, castDuration));
            }
        }

        /// <summary>
        /// Shows a line from the caster to AoE center.
        /// </summary>
        public void ShowAoECurveLine(Vector3 startPos, Vector3 endPos)
        {
            if (aoeCurveLine == null) return;
            aoeCurveLine.gameObject.SetActive(true);

            Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f);
            aoeCurveLine.transform.position = midPoint;

            Vector3 dir = endPos - startPos;
            float dist = dir.magnitude;
            if (dist > 0.0001f)
            {
                // rotate around Y only
                Vector3 dirFlat = new Vector3(dir.x, 0f, dir.z);
                if (dirFlat.sqrMagnitude > 0.0001f)
                {
                    aoeCurveLine.transform.rotation = Quaternion.LookRotation(dirFlat, Vector3.up);
                }
                aoeCurveLine.transform.localScale = new Vector3(dist, dist, 1f);
            }
        }

        /// <summary>
        /// Hides the AoE curve line after a delay.
        /// </summary>
        public IEnumerator HideAoECurveLineAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (aoeCurveLine != null)
            {
                aoeCurveLine.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Closes all indicators immediately.
        /// </summary>
        public void CloseIndicators()
        {
            if (circleRangeIndicator != null) circleRangeIndicator.gameObject.SetActive(false);
            if (castCircleIndicator != null) castCircleIndicator.gameObject.SetActive(false);
            if (damageIndicator != null) damageIndicator.gameObject.SetActive(false);
            if (castDamageIndicator != null) castDamageIndicator.gameObject.SetActive(false);
            if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);
            if (castArrowIndicator != null) castArrowIndicator.gameObject.SetActive(false);
            if (coneIndicator != null) coneIndicator.gameObject.SetActive(false);
            if (castConeIndicator != null) castConeIndicator.gameObject.SetActive(false);
            if (aoeCurveLine != null) aoeCurveLine.gameObject.SetActive(false);
        }
    }
}
