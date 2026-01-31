using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents the data for a skill.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkillData", menuName = "BulletHellTemplate/Characters/Skill Data", order = 52)]
    public partial class SkillData : ScriptableObject
    {
        // -------------------------
        // 1) SKILL DETAILS
        // -------------------------
        [Header("Skill Details")]
        [Tooltip("Icon representing the skill.")]
        public Sprite icon;

        [Tooltip("Icon representing the evolved skill.")]
        public Sprite iconEvolved;

        [Tooltip("Frame used when the skill is evolved.")]
        public Sprite frameEvolved;

        [Tooltip("Name of the skill.")]
        public string skillName;

        public NameTranslatedByLanguage[] skillNameTranslated;

        [Tooltip("Description of the skill.")]
        public string skillDescription;

        public DescriptionTranslatedByLanguage[] skillDescriptionTranslated;

        [Tooltip("Type of damage dealt by the skill.")]
        public CharacterTypeData damageType;

        [Tooltip("Cooldown time between skill uses.")]
        public int cooldown;

        [Tooltip("Delay before the skill is launched.")]
        public float delayToLaunch = 0.3f;

        [Tooltip("Set a value above zero if you want the character to stop moving for a period of time.")]
        public float delayToMove = 0;

        [Tooltip("Check if you want the character to be able to turn while standing still.")]
        public bool canRotateWhileStopped;

        [Tooltip("Time to wait before allowing auto-attack after using this skill.")]
        public float autoAttackDelay = 1.0f;

        [Tooltip("Does it cost mana to use an ability?")]
        public float manaCost = 0;

        public AimType AimMode = AimType.InputDirection;

        [Tooltip("Default DamageEntity passEntryPrefab.")]
        public DamageEntity damageEntityPrefab;

        // -------------------------
        // 2) EFFECTS
        // -------------------------
        [Header("Effects")]
        [Tooltip("Effect to spawn when the skill is activated.")]
        public GameObject spawnEffect;

        [Tooltip("Audio clip to play when the skill is activated.")]
        public AudioClip spawnAudio;

        public bool playSpawnAudioEaShot;

        [Tooltip("Audio clip to play when the skill is in action.")]
        public AudioClip skillAudio;

        public bool playSkillAudioEaShot;

        [Tooltip("Delay before playing the skill audio.")]
        public float playSkillAudioAfter = 0.15f;

        [Header("Effects On Enemy")]
        public GameObject HitEffect;

        public AudioClip HitAudio;

        // -------------------------
        // 3) SKILL SETTINGS
        // -------------------------
        [Header("Skill Settings")]
        [Tooltip("Type of range indicator to display.")]
        public RangeIndicatorType rangeIndicatorType = RangeIndicatorType.None;

        [Tooltip("Settings for the radial range indicator.")]
        public RadialIndicatorSettings radialIndicatorSettings;

        [Tooltip("Settings for the radial area-of-effect (AoE) indicator.")]
        public RadialAoEIndicatorSettings radialAoEIndicatorSettings;

        [Tooltip("Settings for the arrow range indicator.")]
        public ArrowIndicatorSettings arrowIndicatorSettings;

        [Tooltip("Settings for the cone range indicator.")]
        public ConeIndicatorSettings coneIndicatorSettings;

        [Tooltip("Defines the launch method for the skill (e.g., Projectile, Targeted AoE, Targeted Air Strike).")]
        public LaunchType launchType = LaunchType.Projectile;

        [Tooltip("Height used for air strike skills.")]
        public float airStrikeHeight;

        [Tooltip("Should the character rotate towards the enemy?")]
        public bool isRotateToEnemy;

        [Tooltip("Duration of the rotation.")]
        public float rotateDuration;

        // -------------------------
        // 4) DESTROY SETTINGS
        // -------------------------
        [Header("Destroy Settings")]
        [Tooltip("Destroy the DamageEntity when hitting the first enemy.")]
        public bool destroyOnFirstHit;

        [Tooltip("Change the size of the axes of the DamageEntity passEntryPrefab.")]
        public DamageEntitySizeChange sizeChangeConfig;

        [Tooltip("Does the skill have lifesteal?")]
        public bool isHpLeech;

        [Header("Explosion on Destroy Settings")]
        [Tooltip("When destroyed (by lifetime or hitting an enemy), instantiate an explosion DamageEntity.")]
        public bool explodeOnDestroy;

        [Tooltip("DamageEntity passEntryPrefab for the explosion.")]
        public DamageEntity explodeDamageEntity;

        [Tooltip("Skill level settings for the explosion.")]
        public SkillLevel explodeEntitySettings;

        // -------------------------
        // 5) MULTI-SHOT SETTINGS
        // -------------------------
        [Header("Multi-Shot Settings")]
        [Tooltip("Is this a Multi-Shot skill?")]
        public bool isMultiShot;

        [Tooltip("Number of shots.")]
        public int shots;

        [Tooltip("Angle between shots.")]
        public float angle = 30;

        [Tooltip("Delay between each shot.")]
        public float delay = 0;

        // -------------------------
        // 6) ADVANCED MULTI-SHOT SETTINGS
        // -------------------------
        [Header("Advanced Multi-Shot Settings")]
        [Tooltip("Enable advanced multi-shot, using waves and angles.")]
        public bool isAdvancedMultiShot;

        public bool playAnimationEaShot = false;

        [Tooltip("List of shot waves for normal state.")]
        public List<ShotWave> shotWaves;

        [Tooltip("List of shot waves for evolved state.")]
        public List<ShotWave> shotWavesEvolved;

        // -------------------------
        // 7) ORBITAL SETTINGS
        // -------------------------
        [Header("Orbital Settings")]
        [Tooltip("Is this an Orbital skill?")]
        public bool isOrbital;

        [Tooltip("Distance before starting to orbit.")]
        public float orbitalDistance = 5f;

        // -------------------------
        // 8) RICOCHET SETTINGS
        // -------------------------
        [Header("Ricochet Settings")]
        [Tooltip("Does the skill ricochet off surfaces?")]
        public bool isRicochet;

        // -------------------------
        // 9) BOOMERANG SETTINGS
        // -------------------------
        [Header("Boomerang Settings")]
        [Tooltip("Is this a Boomerang skill?")]
        public bool isBoomerangSkill;

        [Tooltip("Maximum distance before returning.")]
        public float maxDistance;

        // -------------------------
        // 10) DASH SETTINGS
        // -------------------------
        [Header("Dash Settings")]
        [Tooltip("Is this a Dash skill?")]
        public bool isDash;

        [Tooltip("If true, the dash will move in the reverse direction.")]
        public bool isReverseDash;

        [Tooltip("Speed of the dash.")]
        public float dashSpeed;

        [Tooltip("Duration of the dash.")]
        public float dashDuration;

        /// <summary>
        /// Advanced dash settings for multiple dash waves and modes.
        /// </summary>
        [Tooltip("Advanced dash configuration.")]
        public AdvancedDashSettings advancedDashSettings = new AdvancedDashSettings();

        // -------------------------
        // 11) MELEE SETTINGS
        // -------------------------
        [Header("Melee Settings")]
        [Tooltip("Is this a Melee skill?")]
        public bool isMelee;

        // -------------------------
        // 12) SLOW EFFECT SETTINGS
        // -------------------------
        [Header("Slow Effect Settings")]
        [Tooltip("Does the skill apply a slow effect?")]
        public bool applySlow;

        [Tooltip("Percentage to slow the target.")]
        public float slowPercent;

        [Tooltip("Duration of the slow effect.")]
        public float slowDuration;

        // -------------------------
        // 13) KNOCKBACK EFFECT SETTINGS
        // -------------------------
        [Header("Knockback Effect Settings")]
        [Tooltip("Does the skill apply knockback?")]
        public bool applyKnockback;

        [Tooltip("Distance of the knockback.")]
        public float knockbackDistance;

        [Tooltip("Duration of the knockback effect.")]
        public float knockbackDuration;

        // -------------------------
        // 14) STUN EFFECT SETTINGS
        // -------------------------
        [Header("Stun Effect Settings")]
        [Tooltip("Does the skill apply stun?")]
        public bool applyStun;

        [Tooltip("Duration of the stun effect.")]
        public float stunDuration;

        // -------------------------
        // 15) DAMAGE OVER TIME (DOT) SETTINGS
        // -------------------------
        [Header("Damage Over Time (DOT) Settings")]
        [Tooltip("Does the skill apply damage over time?")]
        public bool applyDOT;

        [Tooltip("Total DOT damage.")]
        public int dotAmount;

        [Tooltip("Duration of the DOT effect.")]
        public float dotDuration;

        // -------------------------
        // 16) BUFFS AND DEBUFFS
        // -------------------------
        [Header("Self-characterBuffsComponent/Self-debuffs")]
        [Header("Buffs")]
        [Tooltip("Does this skill heal the user?")]
        public bool receiveHeal;
        [Tooltip("Heal amount if receiveHeal is true.")]
        public float healAmount;

        [Tooltip("Is this a Shield skill?")]
        public bool receiveShield;
        [Tooltip("Amount of shield provided.")]
        public int shieldAmount;
        [Tooltip("Duration of the shield.")]
        public float shieldDuration;

        [Tooltip("Does this skill grant a movement speed buff?")]
        public bool receiveMoveSpeed;
        [Tooltip("Movement speed increase amount.")]
        public float moveSpeedAmount;
        [Tooltip("Duration of the movement speed buff.")]
        public float moveSpeedDuration;

        [Tooltip("Does this skill grant an attack speed buff?")]
        public bool receiveAttackSpeed;
        [Tooltip("Attack speed increase amount.")]
        public float attackSpeedAmount;
        [Tooltip("Duration of the attack speed buff.")]
        public float attackSpeedDuration;

        [Tooltip("Does this skill grant a defense buff?")]
        public bool receiveDefense;
        [Tooltip("Defense increase amount.")]
        public float defenseAmount;
        [Tooltip("Duration of the defense buff.")]
        public float defenseDuration;

        [Tooltip("Does this skill grant a damage buff?")]
        public bool receiveDamage;
        [Tooltip("Damage increase amount.")]
        public float damageAmount;
        [Tooltip("Duration of the damage buff.")]
        public float damageDuration;

        [Tooltip("Does the skill grant invincibility?")]
        public bool isInvincible;
        [Tooltip("Duration of invincibility.")]
        public float invincibleDuration = 0;

        [Header("Debuff")]
        [Tooltip("Does this skill apply a slow to the user?")]
        public bool receiveSlow;

        [Tooltip("Amount of self-slow if receiveSlow is true (0 = no slow, 1 = 100% slow).")]
        [Range(0f, 1f)]
        public float receiveSlowAmount;

        [Tooltip("Duration of the self-slow.")]
        public float receiveSlowDuration;

        // -------------------------
        // 17) EVOLVE CHANGES
        // -------------------------
        [Tooltip("Evolved DamageEntity passEntryPrefab.")]
        public DamageEntity evolvedDamageEntityPrefab;

        [Tooltip("Do evolve changes affect Multi-Shot settings?")]
        public bool evolveChanges;

        [Tooltip("Number of shots when evolved.")]
        public int shotsEvolved;

        [Tooltip("Angle between shots when evolved.")]
        public float angleEvolved = 30;

        [Tooltip("Delay between shots when evolved.")]
        public float delayEvolved = 0;

        [Header("Skill Levels")]
        [Tooltip("Levels of the skill.")]
        public List<SkillLevel> skillLevels;

        [Header("Evolved Skill Settings")]
        [Tooltip("Stat required for skill evolution.")]
        public StatPerkData requireStatForEvolve;
                 

        /// <summary>
        /// Launches damage from a character.
        /// </summary>
        /// <param name="attackTransform">Transform from which the damage is launched.</param>
        /// <param name="direction">Direction to launch the damage.</param>
        /// <param name="attacker">Character who is attacking.</param>
        public void LaunchDamage(Transform attackTransform, Vector3 direction, CharacterEntity attacker, bool isReplica)
        {

            int level = GameplayManager.Singleton.GetBaseSkillLevel(this);
            SkillLevel levelData = skillLevels[Mathf.Clamp(level, 0, skillLevels.Count - 1)];

            LaunchDamageInternal(attackTransform, direction, levelData, attacker, isReplica).Forget();
        }

        /// <summary>
        /// Launches the damage entity (projectile) in the specified direction,
        /// handling orbital, multi-shot and advanced multi-shot.
        /// </summary>
        private async UniTaskVoid LaunchDamageInternal(
            Transform attackTransform,
            Vector3 direction,
            SkillLevel levelData,
            CharacterEntity attacker,
            bool isReplica)
        {
            bool useEvolved = levelData.isEvolved;
            int finalShots = useEvolved && evolveChanges ? shotsEvolved : shots;
            float finalAngle = useEvolved && evolveChanges ? angleEvolved : angle;
            float finalDelay = useEvolved && evolveChanges ? delayEvolved : delay;
            var finalWaves = useEvolved && evolveChanges ? shotWavesEvolved : shotWaves;
            var prefabToUse = useEvolved ? evolvedDamageEntityPrefab : damageEntityPrefab;

            if (prefabToUse == null || attackTransform == null)
            {
                Debug.LogWarning("DamageEntity passEntryPrefab ou attackTransform não atribuídos.");
                return;
            }

            /* ───── Orbital ───── */
            if (isOrbital)
            {
                var manager = new GameObject("OrbitManager").AddComponent<OrbitManager>();
                manager.InitializeOrbital(
                    this,
                    levelData,
                    attacker,
                    finalShots,
                    prefabToUse,
                    direction.normalized * levelData.speed,
                    isReplica);
                return;
            }

            /* ───── Advanced Multi-shot ───── */
            if (isAdvancedMultiShot && finalWaves?.Count > 0)
            {
                foreach (var wave in finalWaves)
                {
                    if (wave.shotAngles?.Count == 0) continue;

                    Vector3 baseDir = GetUpdatedDirection(attacker, direction);
                    var angles = GenerateShotAngles(
                                    wave.shotCount,
                                    wave.shotAngles[0],
                                    wave.useInitialAngle,
                                    wave.initialAngle,
                                    wave.shotAngles);

                    foreach (float a in angles)
                    {
                        Quaternion rot = Quaternion.AngleAxis(a, Vector3.up);
                        SpawnDamageEntity(
                            attackTransform,
                            rot * baseDir,
                            prefabToUse,
                            levelData,
                            attacker,
                            isReplica);
                    }

                    if (wave.delayBeforeNextWave > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(wave.delayBeforeNextWave));
                }
                return;
            }

            /* ───── Multi-shot simples ───── */
            if (isMultiShot)
            {
                if (finalShots <= 0) finalShots = 1;

                float step = finalShots > 1 ? finalAngle / (finalShots - 1) : 0f;
                float start = -finalAngle * 0.5f;

                for (int i = 0; i < finalShots; ++i)
                {
                    Vector3 curDir = GetUpdatedDirection(attacker, direction);
                    Quaternion rot = Quaternion.AngleAxis(start + step * i, Vector3.up);
                    SpawnDamageEntity(attackTransform, rot * curDir, prefabToUse, levelData, attacker, isReplica);

                    if (finalDelay > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(finalDelay));
                }
                return;
            }

            /* ───── Single shot ───── */
            SpawnDamageEntity(attackTransform, GetUpdatedDirection(attacker, direction), prefabToUse, levelData, attacker, isReplica);
        }

        /// <summary>
        /// Returns the direction to launch the projectile.
        /// Keeps the original aiming vector unless it is zero,
        /// falling back to the attacker's forward as a safe default.
        /// </summary>
        private Vector3 GetUpdatedDirection(CharacterEntity attacker, Vector3 originalDirection)
        {
            // Use the direction calculated pelo CharacterAttackComponent
            if (originalDirection.sqrMagnitude > 0.0001f)
                return originalDirection.normalized;

            // Fallback: attacker forward
            if (attacker != null && attacker.characterModelTransform != null)
                return attacker.characterModelTransform.forward.normalized;

            // Final fallback to world forward
            return Vector3.forward;
        }

        /// <summary>
        /// Generates shot angles based on wave data or a single baseAngle approach.
        /// This version supports passing an entire list of angles if needed.
        /// </summary>
        /// <param name="shotCount">Number of shots in the wave.</param>
        /// <param name="baseAngle">Base angle for shot increments.</param>
        /// <param name="useInitialAngle">If true, the first shot uses initialAngle.</param>
        /// <param name="initialAngle">The angle used if useInitialAngle is true.</param>
        /// <param name="shotAnglesList">The raw angles from the wave (optional).</param>
        /// <returns>List of angles for each shot in the wave.</returns>
        private List<float> GenerateShotAngles(int shotCount, float baseAngle, bool useInitialAngle, float initialAngle, List<float> shotAnglesList)
        {
            List<float> angles = new List<float>();
            if (shotCount <= 0) return angles;

            if (shotAnglesList != null && shotAnglesList.Count > 1)
            {
                for (int i = 0; i < shotCount; i++)
                    angles.Add(i < shotAnglesList.Count ? shotAnglesList[i] : shotAnglesList[^1]); // repeat last
            }
            else
            {
                if (shotCount == 1)
                {
                    angles.Add(useInitialAngle ? initialAngle : baseAngle);
                }
                else
                {
                    if (baseAngle != 0f)
                    {
                        if (useInitialAngle)
                        {
                            angles.Add(initialAngle);
                            for (int i = 1; i < shotCount; i++)
                            {
                                float angleOffset = ((i + 1) / 2) * baseAngle;
                                angles.Add(i % 2 == 1 ? initialAngle + angleOffset : initialAngle - angleOffset);
                            }
                        }
                        else
                        {
                            angles.Add(0f);
                            for (int i = 1; i < shotCount; i++)
                            {
                                float angleOffset = ((i + 1) / 2) * baseAngle;
                                angles.Add(i % 2 == 1 ? angleOffset : -angleOffset);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < shotCount; i++) angles.Add(0f);
                    }
                }
            }
            return angles;
        }

        /// <summary>
        /// Spawns a damage entity in the given direction.
        /// </summary>
        private void SpawnDamageEntity(Transform attackTransform, Vector3 direction, DamageEntity damageEntityPrefab, SkillLevel levelData, CharacterEntity attacker, bool isReplica)
        {
            /* ───── Audio & animation per-shot ───── */
            if (playSkillAudioEaShot && skillAudio != null)
                AudioManager.Singleton.PlayAudio(skillAudio, "vfx");

            if (playAnimationEaShot && attacker != null)
                attacker.PlaySkill(attacker.GetSkillIndex(this));

            if (playSpawnAudioEaShot && spawnAudio != null)
                AudioManager.Singleton.PlayAudio(spawnAudio, "vfx");

            /* ───── Build provider once ───── */
            var provider = new SkillDamageProvider(this, levelData, isReplica);

            /* ───── Spawn melee (child of character) ───── */
            if (isMelee && attacker != null && attacker.characterModelTransform != null)
            {
                GameObject go = GameEffectsManager.SpawnEffect(
                    damageEntityPrefab.gameObject,
                    attacker.characterModelTransform.position,
                    Quaternion.identity,
                    attacker.characterModelTransform); // parent

                var dmg = go.GetComponent<DamageEntity>();
                dmg.Init(provider, attacker, Vector3.zero,isReplica);

                if (sizeChangeConfig.enableSizeChange)
                    dmg.SetSizeChange(sizeChangeConfig);

                return;
            }

            /* ─── PROJECTILE ─── */
            Vector3 pos = attackTransform.position;
            Quaternion rot = Quaternion.LookRotation(direction);
            Vector3 velocity = direction.normalized * levelData.speed;

            GameObject projGO = GameEffectsManager.SpawnEffect(
                damageEntityPrefab.gameObject,
                pos,
                rot);

            var proj = projGO.GetComponent<DamageEntity>();
            proj.Init(provider, attacker, velocity, isReplica);
            if (sizeChangeConfig.enableSizeChange) proj.SetSizeChange(sizeChangeConfig);
        }
    }
}
