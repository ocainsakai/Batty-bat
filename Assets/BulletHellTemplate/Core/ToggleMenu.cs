using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public class ToggleMenu : MonoBehaviour
    {
        public GameObject menuToToggle;

        /// <summary>
        /// Toggles the visibility of the menu GameObject when clicked.
        /// </summary>
        public void OnClickToggleMenu()
        {
            if (menuToToggle != null)
            {
                // Toggle the active state of the menu
                menuToToggle.SetActive(!menuToToggle.activeSelf);
            }
            else
            {
                Debug.LogError("No menu GameObject is assigned to toggle.");
            }
        }
    }
}
