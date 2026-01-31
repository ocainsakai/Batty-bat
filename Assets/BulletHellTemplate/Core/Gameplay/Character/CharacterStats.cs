using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents the base characterStatsComponent for a character.
    /// </summary>
    [System.Serializable]
    public class CharacterStats
    {
        /// <summary>
        /// The base health points of the character.
        /// </summary>
        public float baseHP;

        /// <summary>
        /// The base health regeneration rate per second.
        /// </summary>
        public float baseHPRegen;

        /// <summary>
        /// The base health leech percentage (health gained from damage dealt).
        /// </summary>
        public float baseHPLeech;

        /// <summary>
        /// The base Mana points of the character.
        /// </summary>
        public float baseMP;

        /// <summary>
        /// The base Mana regeneration rate per second.
        /// </summary>
        public float baseMPRegen;

        /// <summary>
        /// The base damage dealt by the character.
        /// </summary>
        public float baseDamage;

        /// <summary>
        /// The base attack speed of the character.
        /// </summary>
        public float baseAttackSpeed;

        /// <summary>
        /// The base cooldown reduction percentage.
        /// </summary>
        public float baseCooldownReduction;

        /// <summary>
        /// The base critical hit rate percentage.
        /// </summary>
        public float baseCriticalRate;

        /// <summary>
        /// The base critical damage multiplier.
        /// </summary>
        public float baseCriticalDamageMultiplier;

        /// <summary>
        /// The base defense points of the character.
        /// </summary>
        public float baseDefense;

        /// <summary>
        /// The base shield points of the character.
        /// </summary>
        public float baseShield;

        /// <summary>
        /// The base movement speed of the character.
        /// </summary>
        public float baseMoveSpeed;

        // <summary>
        /// The base range to collect drops.
        /// </summary>
        public float baseCollectRange;
        // <summary>
        /// Max Stats to Choice from;
        /// </summary>
        public float baseMaxStats;
        // <summary>
        /// Max Skills to Choice from.
        /// </summary>
        public float baseMaxSkills;
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CharacterStats()
        {
        }

        /// <summary>
        /// Copy constructor that creates a new Singleton of CharacterStats with the same values as the provided characterStatsComponent.
        /// </summary>
        /// <param name="statsToCopy">The CharacterStats Singleton to copy values from.</param>
        public CharacterStats(CharacterStats statsToCopy)
        {
            baseHP = statsToCopy.baseHP;
            baseHPRegen = statsToCopy.baseHPRegen;
            baseHPLeech = statsToCopy.baseHPLeech;
            baseMP = statsToCopy.baseMP;
            baseMPRegen = statsToCopy.baseMPRegen;
            baseDamage = statsToCopy.baseDamage;
            baseAttackSpeed = statsToCopy.baseAttackSpeed;
            baseCooldownReduction = statsToCopy.baseCooldownReduction;
            baseCriticalRate = statsToCopy.baseCriticalRate;
            baseCriticalDamageMultiplier = statsToCopy.baseCriticalDamageMultiplier;
            baseDefense = statsToCopy.baseDefense;
            baseShield = statsToCopy.baseShield;
            baseMoveSpeed = statsToCopy.baseMoveSpeed;
            baseCollectRange = statsToCopy.baseCollectRange;
            baseMaxStats = statsToCopy.baseMaxStats;
            baseMaxSkills = statsToCopy.baseMaxSkills;
        }
    }
}
