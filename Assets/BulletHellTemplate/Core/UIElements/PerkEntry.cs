using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents an entry for displaying either skill or stat perks in the game's UI.
    /// </summary>
    public class PerkEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Icon representing the skill or stat.")]
        public Image icon;
        public Sprite defaultFrame;
        [Tooltip("Frame image that can change to indicate special statuses like readiness to evolve.")]
        public Image frame;

        [Tooltip("Text displaying detailed information about the skill or stat.")]
        public TextMeshProUGUI statText;

        private SkillPerkData skillData; // Skill data associated with this entry if it's a skill perk.
        private StatPerkData statData; // Stat data associated with this entry if it's a stat perk.
        private SkillData baseSkillData; // Base skill data if the entry is a base skill from the character's core abilities.
        private bool isSkill; // Flag to determine if this entry represents a skill.
        private string currentLang;
        /// <summary>
        /// Configures the entry based on the type of data provided (either skill perk, stat perk, or base skill).
        /// </summary>
        /// <param name="data">The skill or stat data to be displayed.</param>
        public void SetupEntry(object data)
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            if (data is SkillPerkData skillPerkData)
            {
                SetupSkillEntry(skillPerkData);
            }
            else if (data is StatPerkData statPerkData)
            {
                SetupStatEntry(statPerkData);
            }
            else if (data is SkillData skillData)
            {
                SetupBaseSkillEntry(skillData);
            }
        }

        /// <summary>
        /// Sets up the entry with skill perk data, adjusting visual elements like icons and texts.
        /// </summary>
        /// <param name="data">The skill perk data to display.</param>
        public void SetupSkillEntry(SkillPerkData data)
        {
            skillData = data;
            isSkill = true;

            int currentLevel = GameplayManager.Singleton.GetSkillLevel(skillData);
            int nextLevel = currentLevel + 1;
            bool willReachMaxLevel = nextLevel == skillData.maxLevel;
            bool canEvolve = skillData.hasEvolution && CheckForRequiredStat(skillData);
            if (willReachMaxLevel && canEvolve)
            {
                icon.sprite = skillData.maxLevelIcon ?? skillData.icon;
                frame.sprite = skillData.maxLevelFrame ?? defaultFrame;
            }
            else
            {
                icon.sprite = skillData.icon;
                frame.sprite = defaultFrame;
            }

            statText.text = $"Skill: {skillData.name}";
        }



        /// <summary>
        /// Checks if the required stat perk for skill evolution is met.
        /// </summary>
        /// <param name="skillData">Skill perk data which may require a stat perk to evolve.</param>
        /// <returns>True if the required stat perk is active; otherwise, false.</returns>
        private bool CheckForRequiredStat(SkillPerkData skillData)
        {
            return skillData.perkRequireToEvolveSkill == null || GameplayManager.Singleton.GetCharacterEntity().HasStatPerk(skillData.perkRequireToEvolveSkill);
        }

        /// <summary>
        /// Sets up the entry with stat data, adjusting visual elements like icons and texts.
        /// </summary>
        /// <param name="data">The stat data to display.</param>
        public void SetupStatEntry(StatPerkData data)
        {
            statData = data;
            isSkill = false;
            icon.sprite = statData.icon;
            frame.sprite = defaultFrame; // Reset frame
            statText.text = $"Stat: {statData.statType}";
        }

        /// <summary>
        /// Sets up the entry with base skill data, typically representing core character abilities.
        /// </summary>
        /// <param name="data">The base skill data to display.</param>
        public void SetupBaseSkillEntry(SkillData data)
        {
            baseSkillData = data;
            isSkill = true;

            int currentLevel = GameplayManager.Singleton.GetBaseSkillLevel(data);
            int nextLevel = currentLevel + 1;
            int maxLevelIndex = data.skillLevels.Count - 1;
            bool willReachMaxLevel = nextLevel == maxLevelIndex;
            bool canEvolve = data.skillLevels[nextLevel].isEvolved && CheckForRequiredStat(data);

            if (willReachMaxLevel || canEvolve)
            {
                icon.sprite = data.iconEvolved ?? data.icon;
                frame.sprite = data.frameEvolved ?? defaultFrame;
            }
            else
            {
                icon.sprite = data.icon;
                frame.sprite = defaultFrame;
            }
            string skillName = GetTranslatedString(data.skillDescriptionTranslated, data.skillName, currentLang);
            statText.text = $"Skill: {data.skillName}";
        }


        /// <summary>
        /// Checks if the required stat perk for base skill evolution is met.
        /// </summary>
        /// <param name="skillData">Base skill data which may require a stat perk to evolve.</param>
        /// <returns>True if the required stat perk is active; otherwise, false.</returns>
        private bool CheckForRequiredStat(SkillData skillData)
        {
            return skillData.requireStatForEvolve == null || GameplayManager.Singleton.GetCharacterEntity().HasStatPerk(skillData.requireStatForEvolve);
        }

        public void ApplyPerk()
        {
            GameplayManager gameplayManager = GameplayManager.Singleton;
            CharacterEntity characterEntity = gameplayManager.GetCharacterEntity();
            if (characterEntity == null)
            {
                Debug.LogWarning("Character entity not found.");
                return;
            }

            if (isSkill)
            {
                if (skillData != null)
                {
                    int currentLevel = gameplayManager.GetSkillLevel(skillData);
                    if (currentLevel < skillData.maxLevel)
                    {
                        gameplayManager.SetPerkLevel(skillData); // Update the skill level
                        characterEntity.ApplySkillPerk(skillData); // Apply the skill perk
                    }
                    else
                    {
                        Debug.LogWarning($"Skill {skillData.name} is already at max level.");
                    }
                }
                else if (baseSkillData != null)
                {
                    int currentLevel = gameplayManager.GetBaseSkillLevel(baseSkillData);
                    if (currentLevel < baseSkillData.skillLevels.Count - 1)
                    {
                        gameplayManager.LevelUpBaseSkill(baseSkillData); // Level up the base skill
                    }
                    else
                    {
                        Debug.LogWarning($"Base skill {baseSkillData.skillName} is already at max level.");
                    }
                }
            }
            else if (statData != null)
            {
                int currentLevel = gameplayManager.GetPerkLevel(statData);
                if (currentLevel < gameplayManager.maxLevelStatPerks)
                {
                    gameplayManager.SetPerkLevel(statData); // Update the stat level
                    characterEntity.ApplyStatPerk(statData, currentLevel + 1); // Apply the stat perk
                }
                else
                {
                    Debug.LogWarning($"Stat {statData.statType} is already at max level.");
                }
            }

            UIGameplay.Singleton.CloseChoicePowerUp();
        }
        public string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        public string GetTranslatedString(DescriptionTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }


    }
}
