using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Core data for any monster skill. Extend via partial classes
    /// without touching this file.
    /// </summary>
    /// 
    [CreateAssetMenu(fileName = "NewMonsterSkill",
                 menuName = "BulletHellTemplate/Monster/Monster Skill",
                 order = 52)]
    public partial class MonsterSkill : ScriptableObject
    {
        [Header("Basic Info")]
        public string skillName;
        public Sprite icon;
        public CharacterTypeData damageType;
        [Tooltip("Cooldown (s) between casts.")]
        public float cooldown = 1f;
        // ── Launch basics ─────────────────────────────────────────────
        public MonsterDamageEntity damagePrefab;
        public Vector3 spawnOffset;
        public float baseDamage;
        public float launchSpeed;
        public float lifeTime;
        public float minDistance;
        public int animationIndex;
        [Tooltip("Delay before the skill is launched.")]
        public float delayToLaunch = 0.3f;
        [Tooltip("Set a value above zero if you want the character to stop moving for a period of time.")]
        public float delayToMove = 0;

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

        // ── Multi-shot / advanced multi-shot ──────────────────────────
        public bool isMultiShot;
        public int shots = 1;
        public float angle = 30f;
        public float delayBetweenShots;

        public bool isAdvancedMultiShot;
        public List<ShotWave> shotWaves = new();

        // ── Special modes ─────────────────────────────────────────────
        public bool isOrbital;
        public float orbitalDistance = 5f;

        public bool isDash;
        public bool isReverseDash;
        public float dashSpeed;
        public float dashDuration;

        public bool isTrapSkill;
        public MonsterTrapEntity trapPrefab;
        public float trapLifetime = 6f;

        public bool isRicochet;
        public bool isBoomerang;
        public float maxBoomerangDistance;

        public bool isMelee;
        public bool isRanged = true;

        // ── Effects ──────────────────────────────────────────────────
        public SlowEffect slow;
        public StunEffect stun;
        public KnockbackEffect knockback;
        public DotEffect dot;

        public SelfHealEffect healSelf;
        public TrapHealEffect healTrap;
    }

}
