using DentedPixel;
using UnityEngine;
using UnityEngine.UI;

namespace PetCareGame.UI
{
    /// <summary>
    /// Handles the infinite rotation of a loading spinner image using LeanTween.
    /// </summary>
    public class LoadingSpinner : MonoBehaviour
    {
        [Header("Spinner Target")]
        [Tooltip("Circular Image to be rotated as loading spinner.")]
        public RectTransform spinnerImage;

        [Header("Rotation Settings")]
        [Tooltip("Rotation speed in seconds per full loop.")]
        public float rotationDuration = 1f;

        private int tweenId = -1;

        private void OnEnable()
        {
            StartRotation();
        }

        private void OnDisable()
        {
            StopRotation();
        }

        /// <summary>
        /// Starts the infinite rotation loop using LeanTween.
        /// </summary>
        public void StartRotation()
        {
            if (spinnerImage == null) return;

            tweenId = LeanTween.rotateAroundLocal(spinnerImage.gameObject, Vector3.forward, -360, rotationDuration)
                .setLoopClamp()
                .setEaseLinear()
                .uniqueId;
        }

        /// <summary>
        /// Stops the rotation if active.
        /// </summary>
        public void StopRotation()
        {
            if (LeanTween.isTweening(tweenId))
            {
                LeanTween.cancel(tweenId);
            }
        }
    }
}
