using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Allows a UI Button to toggle the active state of a target GameObject.
    /// </summary>
    public class UIButtonToggle : MonoBehaviour
    {
        [Header("Target Settings")]
        [Tooltip("The GameObject that will be toggled on or off.")]
        public GameObject target;

        [Header("Optional Button Reference")]
        [Tooltip("The Button that triggers the toggle. If null, the current GameObject's Button is used.")]
        public Button toggleButton;

        private void Start()
        {
            if (target != null)
                target.SetActive(false);
        }

        private void Awake()
        {
            if (toggleButton == null)
                toggleButton = GetComponent<Button>();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleTarget);
        }

        /// <summary>
        /// Toggles the active state of the target GameObject.
        /// </summary>
        public void ToggleTarget()
        {
            if (target != null)
                target.SetActive(!target.activeSelf);
        }
    }
}
