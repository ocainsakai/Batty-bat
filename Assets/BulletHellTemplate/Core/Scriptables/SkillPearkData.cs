using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents the data for skill perks, including icons and damage configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillPerkData", menuName = "Skills/SkillPerkData")]
    public class SkillPerkData : ScriptableObject
    {
        [Header("Skill Perk Settings")]
        [Tooltip("Icon representing the skill perk.")]
        public Sprite icon;
        [Tooltip("Icon representing the skill perk at max level.")]
        public Sprite maxLevelIcon;
        [Tooltip("Frame used when the skill perk is at max level.")]
        public Sprite maxLevelFrame;
        [Tooltip("Maximum level achievable for this skill perk.")]
        public int maxLevel = 5;
        [Tooltip("Description of the skill perk.")]
        public string description;
        [Tooltip("Type of damage dealt by the skill perk.")]
        public CharacterTypeData damageType;

        [Header("Damage Entity Prefabs")]
        [Tooltip("DamageEntity passEntryPrefab for the initial level.")]
        public DamageEntity initialDamagePrefab;
        [Tooltip("DamageEntity passEntryPrefab for the max level.")]
        public DamageEntity maxLvDamagePrefab;

        [Header("Effects On Enemy")]
        public GameObject HitEffect;

        public AudioClip HitAudio;

        [Header("Destroy Settings")]
        [Tooltip("Destroy the DamageEntity when hitting the first enemy.")]
        public bool destroyOnFirstHit;

        [Header("Explosion on Destroy Settings")]
        [Tooltip("When destroyed (by lifetime or hitting an enemy), instantiate an explosion DamageEntity.")]
        public bool explodeOnDestroy;

        [Tooltip("DamageEntity passEntryPrefab for the explosion.")]
        public DamageEntity explodeDamageEntity;

        [Tooltip("Skill level settings for the explosion.")]
        public SkillLevel explodeEntitySettings;

        [Tooltip("Change the size of the axes of the DamageEntity passEntryPrefab.")]
        public DamageEntitySizeChange sizeChangeConfig;

        [Header("Effects on Spawn Skill")]
        [Tooltip("Effect to spawn when the skill is activated.")]
        public GameObject spawnEffect;
        [Tooltip("Audio clip to play when the skill is activated.")]
        public AudioClip spawnAudio;
        [Tooltip("Duration of the spawn effect.")]
        public float effectDuration;
        [Tooltip("Speed of the damage entity.")]
        public float speed = 5f;
        [Tooltip("Lifetime of the damage entity.")]
        public float lifeTime = 5f;
        [Tooltip("Does this skill perk have no cooldown?")]
        public bool withoutCooldown;
        [Tooltip("Cooldown time between uses.")]
        public float cooldown = 1f;
        [Tooltip("Does the skill perk have lifesteal?")]
        public bool isHpLeech;

        [Header("Multi-Shot Settings")]
        [Tooltip("Is this a Multi-Shot skill perk?")]
        public bool isMultiShot;
        [Tooltip("Number of shots.")]
        public int shots;
        [Tooltip("Angle between shots.")]
        public float angle = 30;
        [Tooltip("Delay between each shot.")]
        public float delay = 0;
        [Tooltip("Do evolve changes affect Multi-Shot settings?")]
        public bool evolveChanges;
        [Tooltip("Number of shots when evolved.")]
        public int shotsEvolved;
        [Tooltip("Angle between shots when evolved.")]
        public float angleEvolved = 30;
        [Tooltip("Delay between shots when evolved.")]
        public float delayEvolved = 0;

        [Header("Orbital Settings")]
        [Tooltip("Is this an Orbital skill perk?")]
        public bool isOrbital;
        [Tooltip("Distance before starting to orbit.")]
        public float orbitalDistance = 5f;

        [Header("Boomerang Settings")]
        [Tooltip("Is this a Boomerang skill perk?")]
        public bool isBoomerangSkill;
        [Tooltip("Maximum distance before returning.")]
        public float maxDistance;

        [Header("Ricochet Settings")]
        [Tooltip("Does the skill perk ricochet off surfaces?")]
        public bool isRicochet;

        [Header("Dash Settings")]
        [Tooltip("Is this a Dash skill perk?")]
        public bool isDash;
        [Tooltip("Speed of the dash.")]
        public float dashSpeed;
        [Tooltip("Duration of the dash.")]
        public float dashDuration;

        [Header("Shield Settings")]
        [Tooltip("Is this a Shield skill perk?")]
        public bool isShield;
        [Tooltip("Amount of shield provided.")]
        public int shieldAmount;
        [Tooltip("Duration of the shield.")]
        public float shieldDuration;

        [Header("Melee Settings")]
        [Tooltip("Is this a Melee skill perk?")]
        public bool isMelee;

        [Header("Slow Effect Settings")]
        [Tooltip("Does the skill perk apply a slow effect?")]
        public bool applySlow;
        [Tooltip("Percentage to slow the target.")]
        public float slowPercent;
        [Tooltip("Duration of the slow effect.")]
        public float slowDuration;

        [Header("Knockback Effect Settings")]
        [Tooltip("Does the skill perk apply knockback?")]
        public bool applyKnockback;
        [Tooltip("Distance of the knockback.")]
        public float knockbackDistance;
        [Tooltip("Duration of the knockback effect.")]
        public float knockbackDuration;

        [Header("Stun Effect Settings")]
        [Tooltip("Does the skill perk apply stun?")]
        public bool applyStun;
        [Tooltip("Duration of the stun effect.")]
        public float stunDuration;

        [Header("Damage Over Time (DOT) Settings")]
        [Tooltip("Does the skill perk apply damage over time?")]
        public bool applyDOT;
        [Tooltip("Total DOT damage.")]
        public float dotAmount;
        [Tooltip("Duration of the DOT effect.")]
        public float dotDuration;

        [Header("Evolution Settings")]
        [Tooltip("Does this skill perk have an evolution?")]
        public bool hasEvolution;
        [Tooltip("Perk required to evolve this skill perk.")]
        public StatPerkData perkRequireToEvolveSkill;

        [Header("Damage Per Level")]
        [Tooltip("Damage values for each level.")]
        public List<float> damagePerLevel = new List<float> { 0, 0, 0, 0, 0 };

        [Header("Attacker Damage Rate Per Level")]
        [Tooltip("Attacker damage rate percentages for each level.")]
        public List<float> attackerDamageRate = new List<float> { 0, 0, 0, 0, 0 };

        /// <summary>
        /// Gets the damage for a given level.
        /// </summary>
        /// <param name="level">The level of the skill perk.</param>
        /// <returns>The damage for the specified level.</returns>
        public float GetDamageForLevel(int level)
        {
            int index = level - 1;
            if (index < 0 || index >= damagePerLevel.Count)
            {
                Debug.LogWarning($"Level {level} out of range. Returning 0 damage. Expected range: 1 to {damagePerLevel.Count}");
                return 0;
            }
            return damagePerLevel[index];
        }

        /// <summary>
        /// Gets the attacker damage rate for a given level.
        /// </summary>
        /// <param name="level">The level of the skill perk.</param>
        /// <returns>The attacker damage rate for the specified level.</returns>
        public float GetAttackerDamageRateForLevel(int level)
        {
            int index = level - 1;
            if (index < 0 || index >= attackerDamageRate.Count)
            {
                Debug.LogWarning($"Level {level} out of range. Returning 0 damage rate. Expected range: 1 to {attackerDamageRate.Count}");
                return 0;
            }
            return attackerDamageRate[index];
        }

        /// <summary>
        /// Determines if the skill perk can evolve based on the possession of the required stat perk.
        /// </summary>
        /// <param name="character">The character entity to check for the required perk.</param>
        /// <returns>True if the skill perk can evolve, otherwise false.</returns>
        public bool CanEvolve(CharacterEntity character)
        {
            if (!hasEvolution || perkRequireToEvolveSkill == null)
            {
                return false;
            }

            return character.GetStatsPerkData().Contains(perkRequireToEvolveSkill);
        }
    }
}
