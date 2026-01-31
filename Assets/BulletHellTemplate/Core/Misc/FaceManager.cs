using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages facial expressions and eye movements for a character.
    /// </summary>
    public class FaceManager : MonoBehaviour
    {
        #region Blink Settings
        [Header("Blink Settings")]
        [Tooltip("Enables or disables the blinking system.")]
        public bool useBlinkEyes;

        [Tooltip("Minimum random time between blinks.")]
        public float minBlinkTime;

        [Tooltip("Maximum random time between blinks.")]
        public float maxBlinkTime;

        [Tooltip("Left eyelid GameObject.")]
        public GameObject eyelidL;

        [Tooltip("Right eyelid GameObject.")]
        public GameObject eyelidR;

        [Tooltip("Starting Y scale for eyelids (closed state, e.g., 0).")]
        public float startY;

        [Tooltip("Ending Y scale for eyelids (open state, e.g., 1).")]
        public float endY;

        [Tooltip("Duration of a single blink.")]
        public float blinkDuration;
        #endregion

        #region Look Around Settings
        [Header("Look around settings")]
        [Tooltip("Enables or disables the look-around system.")]
        public bool useLookAround;

        [Tooltip("Minimum random time before starting a look-around routine.")]
        public float minLookAroundTime;

        [Tooltip("Maximum random time before starting a look-around routine.")]
        public float maxLookAroundTime;

        [Tooltip("Left eye GameObject.")]
        public GameObject eyeL;

        [Tooltip("Right eye GameObject.")]
        public GameObject eyeR;

        [Tooltip("Array of possible eye routes for random look-around movements.")]
        public EyeRoute[] randomEyeRoutes;

        [Tooltip("Maximum duration for a single look-around routine.")]
        public float totalLookAroundDuration;

        [Tooltip("Maximum duration for transitioning between route positions.")]
        public float betweenEachRouteDuration;

        [Tooltip("Delay time to hold the eye at the current transform before moving to the next.")]
        public float delayToNextTransform;
        #endregion

        #region Mouth Settings
        [Header("Mouth settings")]
        [Tooltip("Default mouth GameObject.")]
        public GameObject defaultMouth;

        [Tooltip("List of different mouth GameObjects to switch between.")]
        public GameObject[] characterMouths;
        #endregion

        private Coroutine blinkLoopCoroutine;
        private Coroutine lookAroundLoopCoroutine;
        private Coroutine blinkAnimationCoroutine;

        /// <summary>
        /// Initializes the facial control systems if enabled.
        /// </summary>
        public void OnEnable()
        {
            if (useBlinkEyes)
            {
                StartBlinkCoroutine();
            }

            if (useLookAround)
            {
                StartLookAroundCoroutine();
            }
        }

        #region Blink Methods
        /// <summary>
        /// Starts the blinking loop coroutine, picking random intervals between blinks.
        /// </summary>
        private void StartBlinkCoroutine()
        {
            if (blinkLoopCoroutine != null)
            {
                StopCoroutine(blinkLoopCoroutine);
            }
            blinkLoopCoroutine = StartCoroutine(BlinkLoop());
        }

        /// <summary>
        /// Public function to trigger an immediate blink with the specified duration.
        /// If a blink is already happening, it resets eyelids and restarts the blink.
        /// </summary>
        /// <param name="duration">Blink duration.</param>
        public void BlinkEyes(float duration)
        {
            if (blinkAnimationCoroutine != null)
            {
                StopCoroutine(blinkAnimationCoroutine);
            }
            ResetEyelids();
            blinkAnimationCoroutine = StartCoroutine(BlinkAnimation(duration));
        }

        /// <summary>
        /// Resets the eyelids to the starting Y scale.
        /// </summary>
        private void ResetEyelids()
        {
            if (eyelidL != null)
            {
                Vector3 scaleL = eyelidL.transform.localScale;
                scaleL.y = startY;
                eyelidL.transform.localScale = scaleL;
            }
            if (eyelidR != null)
            {
                Vector3 scaleR = eyelidR.transform.localScale;
                scaleR.y = startY;
                eyelidR.transform.localScale = scaleR;
            }
        }

        /// <summary>
        /// Main blinking loop, waits a random time between blinks and triggers a blink.
        /// </summary>
        private IEnumerator BlinkLoop()
        {
            while (true)
            {
                float waitTime = Random.Range(minBlinkTime, maxBlinkTime);
                yield return new WaitForSeconds(waitTime);
                BlinkEyes(blinkDuration);
            }
        }

        /// <summary>
        /// Coroutine that animates eyelids' Y scale from startY to endY and back to startY over the given duration.
        /// </summary>
        /// <param name="duration">Duration of the entire blink animation.</param>
        private IEnumerator BlinkAnimation(float duration)
        {
            float halfDuration = duration / 2f;
            float timer = 0f;

            // Animate eyelids scaling up (open)
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                if (eyelidL != null)
                {
                    Vector3 scaleL = eyelidL.transform.localScale;
                    scaleL.y = Mathf.Lerp(startY, endY, t);
                    eyelidL.transform.localScale = scaleL;
                }
                if (eyelidR != null)
                {
                    Vector3 scaleR = eyelidR.transform.localScale;
                    scaleR.y = Mathf.Lerp(startY, endY, t);
                    eyelidR.transform.localScale = scaleR;
                }
                yield return null;
            }

            // Animate eyelids scaling down (close)
            timer = 0f;
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                if (eyelidL != null)
                {
                    Vector3 scaleL = eyelidL.transform.localScale;
                    scaleL.y = Mathf.Lerp(endY, startY, t);
                    eyelidL.transform.localScale = scaleL;
                }
                if (eyelidR != null)
                {
                    Vector3 scaleR = eyelidR.transform.localScale;
                    scaleR.y = Mathf.Lerp(endY, startY, t);
                    eyelidR.transform.localScale = scaleR;
                }
                yield return null;
            }
        }
        #endregion

        #region Look Around Methods
        /// <summary>
        /// Starts the look-around loop coroutine, picking random intervals before triggering look-around.
        /// </summary>
        private void StartLookAroundCoroutine()
        {
            if (lookAroundLoopCoroutine != null)
            {
                StopCoroutine(lookAroundLoopCoroutine);
            }
            lookAroundLoopCoroutine = StartCoroutine(LookAroundLoop());
        }

        /// <summary>
        /// Public function to trigger a look-around routine for a specified duration using a chosen EyeRoute.
        /// </summary>
        /// <param name="duration">Total duration of the look-around routine.</param>
        /// <param name="eyeRoute">The EyeRoute with transforms for right and left eye routes.</param>
        public void LookAround(float duration, EyeRoute eyeRoute)
        {
            StartCoroutine(LookAroundCoroutine(duration, eyeRoute));
        }

        /// <summary>
        /// Main look-around loop, waits for a random time, then chooses a random route to animate the eyes.
        /// </summary>
        private IEnumerator LookAroundLoop()
        {
            while (true)
            {
                float waitTime = Random.Range(minLookAroundTime, maxLookAroundTime);
                yield return new WaitForSeconds(waitTime);
                if (randomEyeRoutes != null && randomEyeRoutes.Length > 0)
                {
                    int index = Random.Range(0, randomEyeRoutes.Length);
                    EyeRoute chosenRoute = randomEyeRoutes[index];
                    StartCoroutine(LookAroundCoroutine(totalLookAroundDuration, chosenRoute));
                }
            }
        }

        /// <summary>
        /// Coroutine that animates the eyes' position based on the specified EyeRoute until the duration ends,
        /// then returns the eyes to their initial positions.
        /// Both eyes move concurrently at each transition step.
        /// The delayToNextTransform holds the eyes at the current position,
        /// and betweenEachRouteDuration defines the maximum transition time.
        /// The return to the initial positions also uses betweenEachRouteDuration for a smooth transition.
        /// </summary>
        /// <param name="duration">Total duration for the look-around routine.</param>
        /// <param name="eyeRoute">The EyeRoute containing target transforms for both eyes.</param>
        private IEnumerator LookAroundCoroutine(float duration, EyeRoute eyeRoute)
        {
            Vector3 initialPosL = (eyeL != null) ? eyeL.transform.localPosition : Vector3.zero;
            Vector3 initialPosR = (eyeR != null) ? eyeR.transform.localPosition : Vector3.zero;

            int steps = Mathf.Max(
                (eyeRoute.leftEyeRoute != null ? eyeRoute.leftEyeRoute.Length : 0),
                (eyeRoute.rightEyeRoute != null ? eyeRoute.rightEyeRoute.Length : 0)
            );
            float timer = 0f;

            for (int i = 0; i < steps && timer < duration; i++)
            {
                if (i > 0)
                {
                    yield return new WaitForSeconds(delayToNextTransform);
                    timer += delayToNextTransform;
                    if (timer >= duration) break;
                }

                Vector3 targetPosL = (eyeL != null && eyeRoute.leftEyeRoute != null && i < eyeRoute.leftEyeRoute.Length)
                    ? new Vector3(eyeRoute.leftEyeRoute[i].localPosition.x, eyeRoute.leftEyeRoute[i].localPosition.y, eyeL.transform.localPosition.z)
                    : (eyeL != null ? eyeL.transform.localPosition : Vector3.zero);
                Vector3 targetPosR = (eyeR != null && eyeRoute.rightEyeRoute != null && i < eyeRoute.rightEyeRoute.Length)
                    ? new Vector3(eyeRoute.rightEyeRoute[i].localPosition.x, eyeRoute.rightEyeRoute[i].localPosition.y, eyeR.transform.localPosition.z)
                    : (eyeR != null ? eyeR.transform.localPosition : Vector3.zero);

                // Move both eyes concurrently so that they arrive at their targets at the same time.
                yield return StartCoroutine(MoveBothEyesOverTime(eyeL, targetPosL, eyeR, targetPosR, betweenEachRouteDuration));
                timer += betweenEachRouteDuration;
            }

            // Return both eyes to their initial positions with a smooth transition.
            yield return StartCoroutine(MoveBothEyesOverTime(eyeL, initialPosL, eyeR, initialPosR, betweenEachRouteDuration));
            yield return new WaitForSeconds(betweenEachRouteDuration);
        }

        /// <summary>
        /// Smoothly moves both eyes' positions (only X and Y axes) concurrently from their current positions to the specified target positions over the given duration.
        /// Ensures that both eyes start and finish the movement at the same time.
        /// </summary>
        /// <param name="eyeA">The left eye GameObject.</param>
        /// <param name="targetA">Target local position for the left eye (X and Y; Z remains unchanged).</param>
        /// <param name="eyeB">The right eye GameObject.</param>
        /// <param name="targetB">Target local position for the right eye (X and Y; Z remains unchanged).</param>
        /// <param name="duration">Transition duration.</param>
        private IEnumerator MoveBothEyesOverTime(GameObject eyeA, Vector3 targetA, GameObject eyeB, Vector3 targetB, float duration)
        {
            Vector3 startA = (eyeA != null) ? eyeA.transform.localPosition : Vector3.zero;
            Vector3 startB = (eyeB != null) ? eyeB.transform.localPosition : Vector3.zero;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (eyeA != null)
                {
                    Vector3 newPosA = Vector3.Lerp(startA, targetA, t);
                    eyeA.transform.localPosition = new Vector3(newPosA.x, newPosA.y, startA.z);
                }
                if (eyeB != null)
                {
                    Vector3 newPosB = Vector3.Lerp(startB, targetB, t);
                    eyeB.transform.localPosition = new Vector3(newPosB.x, newPosB.y, startB.z);
                }
                yield return null;
            }
        }
        #endregion

        #region Mouth Methods
        /// <summary>
        /// Public function to change the mouth for a specified duration.
        /// After the duration, it reverts to the default mouth.
        /// </summary>
        /// <param name="mouthIndex">Index in the characterMouths array.</param>
        /// <param name="duration">How long the selected mouth stays active.</param>
        public void ChangeMouth(int mouthIndex, float duration)
        {
            StartCoroutine(ChangeMouthCoroutine(mouthIndex, duration));
        }

        public void ChangeMouth(int mouthIndex)
        {
            float duration = 1.5f;
            StartCoroutine(ChangeMouthCoroutine(mouthIndex, duration));
        }

        /// <summary>
        /// Coroutine that changes the mouth to a specific index for a given duration, then reverts to the default mouth.
        /// </summary>
        /// <param name="mouthIndex">Index in the characterMouths array.</param>
        /// <param name="duration">How long the new mouth stays active.</param>
        private IEnumerator ChangeMouthCoroutine(int mouthIndex, float duration)
        {
            if (mouthIndex < 0 || mouthIndex >= characterMouths.Length)
                yield break;

            if (defaultMouth)
                defaultMouth.SetActive(false);

            GameObject chosenMouth = characterMouths[mouthIndex];
            if (chosenMouth)
                chosenMouth.SetActive(true);

            yield return new WaitForSeconds(duration);

            if (chosenMouth)
                chosenMouth.SetActive(false);

            if (defaultMouth)
                defaultMouth.SetActive(true);
        }
        #endregion
    }

    /// <summary>
    /// Holds arrays of Transforms for the right and left eye routes.
    /// </summary>
    [System.Serializable]
    public struct EyeRoute
    {
        [Tooltip("Transform array for the right eye route.")]
        public Transform[] rightEyeRoute;

        [Tooltip("Transform array for the left eye route.")]
        public Transform[] leftEyeRoute;
    }
}
