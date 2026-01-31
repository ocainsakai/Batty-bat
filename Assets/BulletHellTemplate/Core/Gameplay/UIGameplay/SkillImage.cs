using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Struct to hold skill image components for the UI.
    /// </summary>
    [System.Serializable]
    public struct SkillImage
    {
        [Tooltip("Image component to show the skill icon")]
        public Image ImageComponent; // Image component to show the skill icon
        [Tooltip("Image to show cooldown progress")]
        public Image CooldownImage; // Image to show cooldown progress
        [Tooltip("Text to show remaining cooldown time")]
        public TextMeshProUGUI CooldownText; // Text to show remaining cooldown time
        public Image maxLevelSkillIcon; // Image to activate when skill is evolved
        [Tooltip("Index of the skill in the character's skill list")]
        public int Index; // Index of the skill in the character's skill list
    }
}