using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Struct to hold skill perk images for the UI.
    /// </summary>
    [System.Serializable]
    public struct SkillsPerkImage
    {
        [Tooltip("Image component to show the skill perk icon")]
        public Image perkIcon;
        public Image maxLevelPerkIcon;
        [Tooltip("Text to show the current level of the skill perk")]
        public TextMeshProUGUI perkLevel;
    }
}