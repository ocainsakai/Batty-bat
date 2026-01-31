using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Struct to hold stat perk images for the UI.
    /// </summary>
    [System.Serializable]
    public struct StatPerkImage
    {
        [Tooltip("Image component to show the stat perk icon")]
        public Image perkIcon; // Image component to show the stat perk icon
        [Tooltip("Text to show the current level of the stat perk")]
        public TextMeshProUGUI perkLevel; // Text to show the current level of the stat perk
        public Image maxLevelStatIcon;
    }
}