using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class SkillInfoEntry : MonoBehaviour
    {
        /// <summary>
        /// The image component that will display the skill icon.
        /// </summary>
        public Image icon;

        /// <summary>
        /// The title of the skill, used for display purposes.
        /// </summary>
        private string title;

        /// <summary>
        /// The description of the skill, providing details about its effects.
        /// </summary>
        private string description;

        /// <summary>
        /// Tracks the currently displayed skill info in the menu.
        /// </summary>
        private static Sprite currentIcon;
        private static string currentTitle;
        private static string currentDescription;

        /// <summary>
        /// Sets the skill information for this entry.
        /// </summary>
        /// <param name="_icon">The icon representing the skill.</param>
        /// <param name="_title">The title of the skill.</param>
        /// <param name="_description">The description of the skill.</param>
        public void SetSkillInfo(Sprite _icon, string _title, string _description)
        {
            icon.sprite = _icon;
            title = _title;
            description = _description;
        }

        /// <summary>
        /// Toggles the visibility of the skill information menu when the icon is clicked.
        /// If the same skill is selected again, it closes the menu.
        /// If a different skill is selected, it updates the menu without closing it.
        /// </summary>
        public void OnClickShowInfoMenu()
        {
            if (icon != null)
            {
                // Check if the selected skill is the same as the currently displayed one
                bool isSameSkill = (currentIcon == icon.sprite && currentTitle == title && currentDescription == description);
                bool isMenuActive = UICharacterMenu.Singleton.uiSkillInfo.gameObject.activeSelf;

                // If it's the same skill, toggle the menu visibility
                if (isSameSkill && isMenuActive)
                {
                    UICharacterMenu.Singleton.uiSkillInfo.gameObject.SetActive(false);
                }
                else
                {
                    // If a different skill is selected or the menu is not active, update the info and show the menu
                    UICharacterMenu.Singleton.uiSkillInfo.SetSkillInfo(icon.sprite, title, description);
                    UICharacterMenu.Singleton.uiSkillInfo.gameObject.SetActive(true);

                    // Update the currently displayed skill info
                    currentIcon = icon.sprite;
                    currentTitle = title;
                    currentDescription = description;
                }
            }
        }
    }
}
