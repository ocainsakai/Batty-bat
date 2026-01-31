using UnityEngine;
using DentedPixel;

namespace BulletHellTemplate
{
    /// <summary>
    /// UpgradeAnimation uses LeanTween to move a 3D GameObject on selected axes,
    /// rotate it on selected axes, and then return it to its original position and rotation.
    /// It supports both discrete rotation (by a fixed angle) and continuous rotation
    /// for a defined duration.
    /// </summary>
    public class UpgradeAnimation : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Target Settings")]
        /// <summary>
        /// Reference to the GameObject that will be moved and rotated.
        /// </summary>
        [Tooltip("Reference to the GameObject that will be moved and rotated.")]
        public GameObject targetObject;

        [Header("Movement Settings")]
        /// <summary>
        /// Movement offset for each axis. Set non-zero values for the axes you want to move.
        /// </summary>
        [Tooltip("Movement offset for each axis. Set non-zero values for the axes you want to move.")]
        public Vector3 moveDistance = new Vector3(5f, 0f, 0f);

        /// <summary>
        /// Time in seconds for moving from the start position to the target position.
        /// </summary>
        [Tooltip("Time in seconds for moving from the start position to the target position.")]
        public float moveTime = 1f;

        /// <summary>
        /// LeanTweenType used for the movement tween.
        /// </summary>
        [Tooltip("LeanTweenType used for the movement tween.")]
        public LeanTweenType moveTweenType = LeanTweenType.linear;

        [Header("Rotation Settings")]
        /// <summary>
        /// Delay in seconds before starting the rotation after the movement completes.
        /// </summary>
        [Tooltip("Delay in seconds before starting the rotation after the movement completes.")]
        public float delayBeforeRotation = 0.5f;

        /// <summary>
        /// Discrete rotation offset in degrees for each axis.
        /// Used when continuousRotation is false.
        /// </summary>
        [Tooltip("Discrete rotation offset in degrees for each axis. Used when continuousRotation is false.")]
        public Vector3 rotationAngle = new Vector3(45f, 0f, 0f);

        /// <summary>
        /// Time in seconds for performing the rotation tween.
        /// When continuousRotation is false, it rotates by rotationAngle.
        /// When continuousRotation is true, it continuously rotates for this duration.
        /// </summary>
        [Tooltip("Time in seconds for performing the rotation tween.")]
        public float rotationTime = 1f;

        /// <summary>
        /// Time in seconds for returning from the current rotation to the original rotation.
        /// </summary>
        [Tooltip("Time in seconds for returning from the current rotation to the original rotation.")]
        public float returnRotationTime = 1f;

        /// <summary>
        /// LeanTweenType used for the rotation tween.
        /// </summary>
        [Tooltip("LeanTweenType used for the rotation tween.")]
        public LeanTweenType rotationTweenType = LeanTweenType.linear;

        [Header("Continuous Rotation Settings")]
        /// <summary>
        /// If true, the object will continuously rotate for the duration specified in rotationTime,
        /// instead of performing a discrete rotation by a fixed angle.
        /// </summary>
        [Tooltip("If true, the object will continuously rotate for the duration specified in rotationTime.")]
        public bool continuousRotation = false;

        /// <summary>
        /// Rotation speed in degrees per second for continuous rotation.
        /// Only used if continuousRotation is true.
        /// </summary>
        [Tooltip("Rotation speed in degrees per second for continuous rotation. Only used if continuousRotation is true.")]
        public Vector3 continuousRotationSpeed = new Vector3(45f, 0f, 0f);

        [Header("Return Movement")]
        /// <summary>
        /// Time in seconds to move back to the original position.
        /// </summary>
        [Tooltip("Time in seconds to move back to the original position.")]
        public float returnMoveTime = 1f;

        /// <summary>
        /// LeanTweenType used for the return movement tween.
        /// </summary>
        [Tooltip("LeanTweenType used for the return movement tween.")]
        public LeanTweenType returnMoveTweenType = LeanTweenType.linear;

        [Header("Restart Settings")]
        /// <summary>
        /// If true, when StartMovement is called again, any ongoing tween sequence will be canceled and restarted.
        /// </summary>
        [Tooltip("If true, when StartMovement is called again, any ongoing tween sequence will be canceled and restarted.")]
        public bool restartIfCalledAgain = true;

        #endregion

        #region Private Fields

        /// <summary>
        /// The original local position of the target.
        /// </summary>
        private Vector3 originalPosition;

        /// <summary>
        /// The original local rotation of the target.
        /// </summary>
        private Quaternion originalRotation;

        #endregion

        #region Unity Callbacks

        /// <summary>
        /// Stores the initial position and rotation of the targetObject.
        /// </summary>
        private void Awake()
        {
            if (targetObject == null)
            {
                targetObject = this.gameObject;
            }
            originalPosition = targetObject.transform.localPosition;
            originalRotation = targetObject.transform.localRotation;
        }

        /// <summary>
        /// Cancels all LeanTween animations when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            LeanTween.cancel(targetObject);
        }

        /// <summary>
        /// Cancels all LeanTween animations when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            LeanTween.cancel(targetObject);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the movement and rotation sequence using LeanTween.
        /// Moves to a target position by adding moveDistance to the original position,
        /// then after a delay rotates according to the configuration:
        /// - If continuousRotation is false: rotates by rotationAngle then returns to original rotation.
        /// - If continuousRotation is true: continuously rotates using continuousRotationSpeed for rotationTime,
        ///   then returns to the original rotation.
        /// Finally, moves back to the original position.
        /// </summary>
        public void StartMovement()
        {
            if (restartIfCalledAgain)
            {
                LeanTween.cancel(targetObject);
            }

            // Restore the original local position and rotation before starting a new sequence.
            targetObject.transform.localPosition = originalPosition;
            targetObject.transform.localRotation = originalRotation;

            // Create a LeanTween sequence to chain the tweens.
            LTSeq sequence = LeanTween.sequence();

            // 1) Move to target position (originalPosition + moveDistance).
            sequence.append(
                LeanTween.moveLocal(targetObject, originalPosition + moveDistance, moveTime)
                         .setEase(moveTweenType)
            );

            // 2) Add delay before rotation.
            sequence.append(delayBeforeRotation);

            // 3) Apply rotation.
            if (continuousRotation)
            {
                // Continuous rotation: update rotation over time.
                sequence.append(
                    LeanTween.value(gameObject, 0f, rotationTime, rotationTime)
                             .setEase(LeanTweenType.linear)
                             .setOnUpdate((float t) =>
                             {
                                 Vector3 newEuler = originalRotation.eulerAngles + continuousRotationSpeed * t;
                                 targetObject.transform.localRotation = Quaternion.Euler(newEuler);
                             })
                );
            }
            else
            {
                // Discrete rotation: rotate to a fixed angle.
                Vector3 targetRotationEuler = originalRotation.eulerAngles + rotationAngle;
                sequence.append(
                    LeanTween.rotateLocal(targetObject, targetRotationEuler, rotationTime)
                             .setEase(rotationTweenType)
                );
                // Rotate back to original rotation.
                sequence.append(
                    LeanTween.rotateLocal(targetObject, originalRotation.eulerAngles, returnRotationTime)
                             .setEase(rotationTweenType)
                );
            }

            // 4) Return to original rotation if continuous rotation was applied.
            if (continuousRotation)
            {
                sequence.append(
                    LeanTween.rotateLocal(targetObject, originalRotation.eulerAngles, returnRotationTime)
                             .setEase(rotationTweenType)
                );
            }

            // 5) Move back to the original position.
            sequence.append(
                LeanTween.moveLocal(targetObject, originalPosition, returnMoveTime)
                         .setEase(returnMoveTweenType)
            );
        }

        #endregion
    }
}
