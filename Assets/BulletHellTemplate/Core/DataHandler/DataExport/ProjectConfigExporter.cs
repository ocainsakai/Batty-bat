#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports core GameInstance settings and rules to a JSON file.
    /// </summary>
    public static class ProjectConfigExporter
    {
        /// <summary>
        /// Gathers selected GameInstance data and writes it as formatted JSON.
        /// </summary>
        public static void ExportProjectConfig()
        {
            if (GameInstance.Singleton == null)
            {
                Debug.LogError("GameInstance is not initialized.");
                return;
            }

            var gi = GameInstance.Singleton;

            var config = new ProjectConfigData
            {
                accountLevels = new AccountLevelsData
                {
                    accountMaxLevel = gi.accountLevels.accountMaxLevel,
                    initialExp = gi.accountLevels.initialExp,
                    finalExp = gi.accountLevels.finalExp,
                    accountExpCalculationMethod = gi.accountLevels.accountExpCalculationMethod.ToString(),
                    accountExpPerLevel = gi.accountLevels.accountExpPerLevel
                },
                characterMastery = new CharacterMasteryData
                {
                    maxMasteryLevel = gi.characterMastery.maxMasteryLevel,
                    initialExp = gi.characterMastery.initialExp,
                    finalExp = gi.characterMastery.finalExp,
                    characterExpCalculationMethod = gi.characterMastery.characterExpCalculationMethod.ToString(),
                    masteryExpPerLevel = gi.characterMastery.masteryExpPerLevel
                },
                masteryLevels = gi.masteryLevels
                                  .Select(m => new MasteryLevelData { masteryName = m.masteryName })
                                  .ToList(),

                advantageDamageIncrease = gi.advantageDamageIncrease,
                weaknessDamageReduction = gi.weaknessDamageReduction,

                battlePassSettings = new BattlePassSettingsData
                {
                    battlePassExp = gi.BattlePassEXP,
                    goldCurrency = gi.goldCurrency,
                    maxLevel = gi.maxLevelPass,
                    baseExp = gi.baseExpPass,
                    seasonLengthInDays = gi.SeasonLengthInDays,
                    percentageByLevel = gi.incPerLevelPass,
                    currencyID = gi.battlePassCurrencyID,
                    battlePassPrice = gi.battlePassPrice
                },

                nicknameChangeSettings = new NicknameChangeSettingsData
                {
                    changeNameTick = gi.changeNameTick,
                    minNameLength = gi.minNameLength,
                    maxNameLength = gi.maxNameLength,
                    ticketsToChange = gi.ticketsToChange,
                    needTicket = gi.needTicket
                },

                authSettings = new AuthSettingsData
                {
                    minPasswordLength = gi.minPasswordLength,
                    maxPasswordLength = gi.maxPasswordLength,
                    requireUppercase = gi.requireUppercase,
                    requireLowercase = gi.requireLowercase,
                    requireNumbers = gi.requireNumbers,
                    requireSpecial = gi.requireSpecial
                }
            };

            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            string folder = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "ProjectConfig.json");
            File.WriteAllText(path, json);
            Debug.Log($"Project config exported successfully to {path}");
        }

        // ---------- Data containers (POCO) ----------

        private class ProjectConfigData
        {
            public AccountLevelsData accountLevels;
            public CharacterMasteryData characterMastery;
            public List<MasteryLevelData> masteryLevels;
            public float advantageDamageIncrease;
            public float weaknessDamageReduction;
            public BattlePassSettingsData battlePassSettings;
            public NicknameChangeSettingsData nicknameChangeSettings;
            public AuthSettingsData authSettings;
        }

        private class AccountLevelsData
        {
            public int accountMaxLevel;
            public int initialExp;
            public int finalExp;
            public string accountExpCalculationMethod;
            public int[] accountExpPerLevel;
        }

        private class CharacterMasteryData
        {
            public int maxMasteryLevel;
            public int initialExp;
            public int finalExp;
            public string characterExpCalculationMethod;
            public int[] masteryExpPerLevel;
        }

        private class MasteryLevelData
        {
            public string masteryName;
        }

        private class BattlePassSettingsData
        {
            public int battlePassExp;
            public string goldCurrency;
            public int maxLevel;
            public float baseExp;
            public int seasonLengthInDays;
            public float percentageByLevel;
            public string currencyID;
            public int battlePassPrice;
        }

        private class NicknameChangeSettingsData
        {
            public string changeNameTick;
            public int minNameLength;
            public int maxNameLength;
            public int ticketsToChange;
            public bool needTicket;
        }

        private class AuthSettingsData
        {
            public int minPasswordLength;
            public int maxPasswordLength;
            public bool requireUppercase;
            public bool requireLowercase;
            public bool requireNumbers;
            public bool requireSpecial;
        }
    }
}
#endif