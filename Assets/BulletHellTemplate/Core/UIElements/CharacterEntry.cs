using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a UI entry for a single character in the character selection screen.
    /// </summary>
    public class CharacterEntry : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("The image component displaying the character's icon.")]
        public Image characterIcon;
        [Tooltip("The image component indicating whether this character is currently selected.")]
        public Image selected;
        [Tooltip("The image component displaying the character's element icon.")]
        public Image elementIcon;

        public Image classIcon;

        [Tooltip("The text component displaying the character's name.")]
        public TextMeshProUGUI characterName;

        public TextMeshProUGUI characterLevel;
        public TextMeshProUGUI characterCurrentExp;

        public Image curentProgressLevelExpBar;

        public TextMeshProUGUI characterCurrentMasteryExp;
        public Image curentProgressMasteryExpBar;
        public Image masteryIcon;


        [Tooltip("The image component displaying the character's tier icon.")]
        public Image tierIcon;

        [Header("Rarity Settings")]
        [Tooltip("If true, displays the corresponding frame for the character's rarity.")]
        public bool useRarityFrames;
        [Tooltip("If true, applies the text gradient based on rarity.")]
        public bool useTextRarityGradient;

        [Header("Rarity Frames")]
        [Tooltip("Frame displayed for 'Common' rarity.")]
        public GameObject commonFrame;
        [Tooltip("Frame displayed for 'Uncommon' rarity.")]
        public GameObject uncommonFrame;
        [Tooltip("Frame displayed for 'Rare' rarity.")]
        public GameObject rareFrame;
        [Tooltip("Frame displayed for 'Epic' rarity.")]
        public GameObject epicFrame;
        [Tooltip("Frame displayed for 'Legendary' rarity.")]
        public GameObject legendaryFrame;

        [Header("Rarity Text Gradient")]
        [Tooltip("Gradient colors used for 'Common' rarity.")]
        public TextRarityGradient commonTextColor;
        [Tooltip("Gradient colors used for 'Uncommon' rarity.")]
        public TextRarityGradient uncommonTextColor;
        [Tooltip("Gradient colors used for 'Rare' rarity.")]
        public TextRarityGradient rareTextColor;
        [Tooltip("Gradient colors used for 'Epic' rarity.")]
        public TextRarityGradient epicTextColor;
        [Tooltip("Gradient colors used for 'Legendary' rarity.")]
        public TextRarityGradient legendaryTextColor;

        /// <summary>
        /// The ID of the character associated with this entry.
        /// </summary>
        private int characterID;

        /// <summary>
        /// Sets up the character entry with the provided character data.
        /// This includes setting text, icons, and applying rarity frames and text gradient if enabled.
        /// </summary>
        /// <param name="_characterData">The character data to setup the entry.</param>
        public void Setup(CharacterData _characterData, string _characterName)
        {
            characterID = _characterData.characterId;

            if (characterName != null)
                characterName.text = _characterName;

            if (characterIcon != null)
                characterIcon.sprite = _characterData.icon;

            if (elementIcon != null)
                elementIcon.sprite = _characterData.GetCharacterTypeIcon();

            if (tierIcon != null)
            {
                tierIcon.sprite = _characterData.tierIcon;
            }

            if (classIcon != null) classIcon.sprite = _characterData.characterClassIcon;

            int lvl = PlayerSave.GetCharacterLevel(_characterData.characterId);
            int currExp = PlayerSave.GetCharacterCurrentExp(_characterData.characterId);
           


            if (characterLevel != null) characterLevel.text = lvl.ToString();
            if (lvl < _characterData.maxLevel && lvl < _characterData.expPerLevel.Length)
            {
                int requiredExp = _characterData.expPerLevel[lvl];
                if (characterCurrentExp != null) characterCurrentExp.text = $"{currExp}/{requiredExp}";
                if (curentProgressLevelExpBar != null) curentProgressLevelExpBar.fillAmount = (float)currExp / requiredExp;
            }
            else
            {
                if (characterCurrentExp != null) characterCurrentExp.text = "MAX Level";
                if (curentProgressLevelExpBar != null) curentProgressLevelExpBar.fillAmount = 1f;
            }


            int currentMasteryLevel = PlayerSave.GetCharacterMasteryLevel(_characterData.characterId);
            CharacterMasteryLevel masteryInfo = GameInstance.Singleton.GetMasteryLevel(currentMasteryLevel);          
            if (masteryIcon != null) masteryIcon.sprite = masteryInfo.masteryIcon;
            int currMasteryExp = PlayerSave.GetCharacterCurrentMasteryExp(_characterData.characterId);
            if (currentMasteryLevel < GameInstance.Singleton.characterMastery.maxMasteryLevel)
            {
                int requiredMasteryExp = GameInstance.Singleton.GetMasteryExpForLevel(currentMasteryLevel);
                if (characterCurrentMasteryExp != null) characterCurrentMasteryExp.text = $"{currMasteryExp}/{requiredMasteryExp}";
                if (curentProgressMasteryExpBar != null) curentProgressMasteryExpBar.fillAmount = (float)currMasteryExp / requiredMasteryExp;
            }
            else
            {
                if (characterCurrentMasteryExp != null) characterCurrentMasteryExp.text = $"{currMasteryExp}/MAX";
                if (curentProgressMasteryExpBar != null) curentProgressMasteryExpBar.fillAmount = 1f;
            }
          
            if (useRarityFrames)
            {
                ApplyRarityFrame(_characterData.characterRarity.ToString());
            }

            if (useTextRarityGradient)
            {
                ApplyTextRarityGradient(_characterData.characterRarity.ToString());
            }
        }

        /// <summary>
        /// Sets the character ID for this entry.
        /// </summary>
        /// <param name="_characterID">The ID of the character.</param>
        public void SetCharacterId(int _characterID)
        {
            characterID = _characterID;
        }

        /// <summary>
        /// Gets the character ID associated with this entry.
        /// </summary>
        /// <returns>The character ID.</returns>
        public int GetCharacterId()
        {
            return characterID;
        }

        /// <summary>
        /// Called when the user clicks on this character entry.
        /// Opens the character details panel rather than immediately selecting the character.
        /// </summary>
        public void OnClickOpenDetails()
        {
            if (UICharacterMenu.Singleton != null)
            {
                UICharacterMenu.Singleton.ShowCharacterDetails(characterID);
            }
        }

        /// <summary>
        /// Applies the rarity frame based on the character's rarity.
        /// All rarity frames are deactivated before the specific one is activated.
        /// </summary>
        /// <param name="rarity">The rarity as a string.</param>
        private void ApplyRarityFrame(string rarity)
        {
            if (commonFrame != null) commonFrame.SetActive(false);
            if (uncommonFrame != null) uncommonFrame.SetActive(false);
            if (rareFrame != null) rareFrame.SetActive(false);
            if (epicFrame != null) epicFrame.SetActive(false);
            if (legendaryFrame != null) legendaryFrame.SetActive(false);

            switch (rarity)
            {
                case "Common":
                    if (commonFrame != null) commonFrame.SetActive(true);
                    break;
                case "Uncommon":
                    if (uncommonFrame != null) uncommonFrame.SetActive(true);
                    break;
                case "Rare":
                    if (rareFrame != null) rareFrame.SetActive(true);
                    break;
                case "Epic":
                    if (epicFrame != null) epicFrame.SetActive(true);
                    break;
                case "Legendary":
                    if (legendaryFrame != null) legendaryFrame.SetActive(true);
                    break;
                default:
                    if (commonFrame != null) commonFrame.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// Applies the text rarity gradient based on the character's rarity.
        /// Sets the TextMeshPro vertex gradient using the specified top and bottom colors.
        /// </summary>
        /// <param name="rarity">The rarity as a string.</param>
        private void ApplyTextRarityGradient(string rarity)
        {
            TextRarityGradient textRarityGradient;
            switch (rarity)
            {
                case "Common":
                    textRarityGradient = commonTextColor;
                    break;
                case "Uncommon":
                    textRarityGradient = uncommonTextColor;
                    break;
                case "Rare":
                    textRarityGradient = rareTextColor;
                    break;
                case "Epic":
                    textRarityGradient = epicTextColor;
                    break;
                case "Legendary":
                    textRarityGradient = legendaryTextColor;
                    break;
                default:
                    textRarityGradient = commonTextColor;
                    break;
            }

            if (characterName != null)
            {
                characterName.enableVertexGradient = true;
                characterName.colorGradient = new VertexGradient(
                    textRarityGradient.topColor,
                    textRarityGradient.topColor,
                    textRarityGradient.botColor,
                    textRarityGradient.botColor
                );
            }
        }
    }

    /// <summary>
    /// Represents a top and bottom color gradient.
    /// </summary>
    [System.Serializable]
    public struct TextRarityGradient
    {
        public Color topColor;
        public Color botColor;
    }
}
