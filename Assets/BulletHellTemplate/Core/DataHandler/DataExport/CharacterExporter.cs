#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports all character-related data into JSON for server-side validation.
    /// </summary>
    public static class CharacterExporter
    {
        /// <summary>
        /// Gathers data from GameInstance.Singleton and writes to a file under Assets/ExportedData.
        /// </summary>
        public static void ExportCharacterData()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameInstance is not initialized.");
                return;
            }

            var characterList = new List<CharacterExportData>();

            foreach (var character in GameInstance.Singleton.characterData)
            {
                if (character == null)
                    continue;

                // Initialize export DTO, including skins list
                var characterExport = new CharacterExportData
                {
                    characterId = character.characterId,
                    characterClassType = character.characterClassType,
                    characterRarity = character.characterRarity.ToString(),
                    isUnlocked = character.CheckUnlocked,
                    baseStats = new BaseStatsData
                    {
                        baseHP = character.baseStats.baseHP,
                        baseHPRegen = character.baseStats.baseHPRegen,
                        baseHPLeech = character.baseStats.baseHPLeech,
                        baseMP = character.baseStats.baseMP,
                        baseMPRegen = character.baseStats.baseMPRegen,
                        baseDamage = character.baseStats.baseDamage,
                        baseAttackSpeed = character.baseStats.baseAttackSpeed,
                        baseCooldownReduction = character.baseStats.baseCooldownReduction,
                        baseCriticalRate = character.baseStats.baseCriticalRate,
                        baseCriticalDamageMultiplier = character.baseStats.baseCriticalDamageMultiplier,
                        baseDefense = character.baseStats.baseDefense,
                        baseShield = character.baseStats.baseShield,
                        baseMoveSpeed = character.baseStats.baseMoveSpeed,
                        baseCollectRange = character.baseStats.baseCollectRange,
                        baseMaxStats = character.baseStats.baseMaxStats,
                        baseMaxSkills = character.baseStats.baseMaxSkills
                    },
                    maxLevel = character.maxLevel,
                    statsPercentageIncreaseByLevel = character.statsPercentageIncreaseByLevel,
                    expCalculationMethod = character.expCalculationMethod.ToString(),
                    initialExp = character.initialExp,
                    finalExp = character.finalExp,
                    expPerLevel = character.expPerLevel,
                    currencyId = character.currencyId,
                    initialUpgradeCost = character.initialUpgradeCost,
                    finalUpgradeCost = character.finalUpgradeCost,
                    upgradeCostCalculationMethod = character.upgradeCostCalculationMethod.ToString(),
                    upgradeCostPerLevel = character.upgradeCostPerLevel,
                    itemSlots = character.itemSlots,
                    runeSlots = character.runeSlots,
                    statUpgrades = new List<StatUpgradeExportData>(),
                    skills = new List<SkillExportData>(),
                    skins = new List<SkinExportData>()  // <- aqui inicializamos!
                };

                // Export stat upgrades
                if (character.statUpgrades != null)
                {
                    foreach (var statUpgrade in character.statUpgrades)
                    {
                        if (statUpgrade == null) continue;

                        characterExport.statUpgrades.Add(new StatUpgradeExportData
                        {
                            statType = statUpgrade.statType.ToString(),
                            upgradeMaxLevel = statUpgrade.upgradeMaxLevel,
                            upgradeAmounts = statUpgrade.upgradeAmounts,
                            upgradeCosts = statUpgrade.upgradeCosts,
                            currencyId = statUpgrade.currencyTag,
                        });
                    }
                }

                // Export skills (auto-attack + other skills)
                if (character.autoAttack != null)
                {
                    characterExport.skills.Add(new SkillExportData
                    {
                        skillName = character.autoAttack.skillName,
                        cooldown = character.autoAttack.cooldown,
                        manaCost = character.autoAttack.manaCost
                    });
                }

                if (character.skills != null)
                {
                    foreach (var skill in character.skills)
                    {
                        if (skill == null) continue;

                        characterExport.skills.Add(new SkillExportData
                        {
                            skillName = skill.skillName,
                            cooldown = skill.cooldown,
                            manaCost = skill.manaCost
                        });
                    }
                }

                // Export skins
                if (character.characterSkins != null)
                {
                    for (int i = 0; i < character.characterSkins.Length; i++)
                    {
                        var skin = character.characterSkins[i];
                        characterExport.skins.Add(new SkinExportData
                        {
                            index = i,
                            skinName = skin.skinName,
                            isUnlocked = skin.isUnlocked,
                            currencyId = skin.unlockCurrencyId,
                            price = skin.unlockPrice
                        });
                    }
                }

                characterList.Add(characterExport);
            }

            // Serialize and write file
            string json = JsonConvert.SerializeObject(characterList, Formatting.Indented);
            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "CharacterData.json");
            File.WriteAllText(filePath, json);
            Debug.Log($"Character data exported successfully to {filePath}");
        }

        /// <summary>
        /// Data transfer object for exporting character.
        /// </summary>
        private class CharacterExportData
        {
            public int characterId;
            public string characterClassType;
            public string characterRarity;
            public bool isUnlocked;
            public BaseStatsData baseStats;
            public int maxLevel;
            public float statsPercentageIncreaseByLevel;
            public string expCalculationMethod;
            public int initialExp;
            public int finalExp;
            public int[] expPerLevel;
            public string currencyId;
            public int initialUpgradeCost;
            public int finalUpgradeCost;
            public string upgradeCostCalculationMethod;
            public int[] upgradeCostPerLevel;
            public string[] itemSlots;
            public string[] runeSlots;
            public List<StatUpgradeExportData> statUpgrades;
            public List<SkillExportData> skills;
            public List<SkinExportData> skins;
        }

        /// <summary>
        /// Base stats snapshot for export.
        /// </summary>
        private class BaseStatsData
        {
            public float baseHP;
            public float baseHPRegen;
            public float baseHPLeech;
            public float baseMP;
            public float baseMPRegen;
            public float baseDamage;
            public float baseAttackSpeed;
            public float baseCooldownReduction;
            public float baseCriticalRate;
            public float baseCriticalDamageMultiplier;
            public float baseDefense;
            public float baseShield;
            public float baseMoveSpeed;
            public float baseCollectRange;
            public float baseMaxStats;
            public float baseMaxSkills;
        }

        /// <summary>
        /// DTO for stat upgrades.
        /// </summary>
        private class StatUpgradeExportData
        {
            public string statType;
            public int upgradeMaxLevel;
            public float[] upgradeAmounts;
            public int[] upgradeCosts;
            public string currencyId;
        }

        /// <summary>
        /// DTO for skills.
        /// </summary>
        private class SkillExportData
        {
            public string skillName;
            public int cooldown;
            public float manaCost;
        }

        /// <summary>
        /// DTO for skins.
        /// </summary>
        private class SkinExportData
        {
            public int index;
            public string skinName;
            public bool isUnlocked;
            public string currencyId;
            public int price;
        }
    }
}
#endif