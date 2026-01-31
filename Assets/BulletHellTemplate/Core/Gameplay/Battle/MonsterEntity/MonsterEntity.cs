using BulletHellTemplate.Core.Events;
using BulletHellTemplate.Core.FSM;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if FUSION2
    using Fusion;
#endif


namespace BulletHellTemplate
{
    /// <summary>
    /// AI controller for any monster. Handles navigation, skill logic and status
    /// effects, delegating health and drop logic to MonsterHealth + BaseMonsterEntity.
    /// </summary>
    public partial class MonsterEntity : BaseMonsterEntity, ICharacterAnimationContext
    {
        #region ───── Serialized Fields ─────────────────────────────────────────
        
        [Header("Base Stats")]
        [SerializeField] private CharacterStats stats;
        [Tooltip("Element or race information used for advantage calculation.")]
        [SerializeField] private CharacterTypeData characterTypeData;

        [Header("UI")]
        [SerializeField] private GameObject targetIndicator;  // indicates whether this is the current target
        [SerializeField] public Transform effectTransform;
        [SerializeField] private Transform damagePopupContainer;
        [SerializeField] private GameObject hpContainer;
        [SerializeField] private Image hpBar;
        [SerializeField] private Image hpBarDecrease;
        [SerializeField] private float decreaseDelay = 0.12f;
        [SerializeField] private float decreaseSpeed = 1f;
        [SerializeField] private AudioClip deathAudio;

        [Header("Animations")]
        [SerializeField] private Animator animator;
        [SerializeField] bool useHFSM = true;
        [SerializeField] bool playHitAnim = false;
        [Space]
        [Header("Directional Sets (8-Way)")]
        public DirectionalAnimSet idleSet;
        public DirectionalAnimSet moveSet;
        [Header("Combat Clips")]
        public SkillAnimData attack;
        public AnimClipData receiveDamage;
        public AnimClipData death;
        [Header("Skill Clips")]
        public SkillAnimData[] skillAnimations;
        CharacterAnimationFSM fsm;

        [Header("Advanced Behaviour")]
        [Tooltip("Distance this monster tries to keep from its current target (0 = melee).")]
        [SerializeField]private float preferredRange = 0f;
        [Tooltip("Dead-zone to avoid constant oscillation around preferredRange.")]
        [SerializeField] private float rangeTolerance = 0.5f;
        [SerializeField] private float delayToMoveAfterSpawn = 0f;
        [SerializeField] private Transform shootOrigin;
        [Tooltip("Time, in seconds, between basic contact attacks.")]
        [SerializeField] private float attackCooldown = 1.5f;
        [Tooltip("Is this monster considered the final boss of the stage?")]
        [SerializeField] private bool isFinalBoss = false;

        [Header("Skill Settings")]
        [Tooltip("Enable automatic skill usage.")]
        [SerializeField] private bool canUseSkill = true;
        [Tooltip("List of ScriptableObject-like skill descriptors.")]
        [SerializeField] private MonsterSkill[] monsterSkills;

        [Header("Visual Effect Flags")]
        [SerializeField] private bool showSlowEffect = false;
        [SerializeField] private bool showDOTEffect = false;
        [SerializeField] private bool showStunEffect = false;

        [Header("Effect Palette")]
        [SerializeField] private Color slowEffectColor = new(0.5f, 0.5f, 1f);
        [SerializeField] private Color dotEffectColor = new(0f, 1f, 0f);
        [SerializeField] private Color stunEffectColor = new(1f, 1f, 0f);
        [SerializeField] private float effectFlashInterval = 0.2f;

        [Header("Unity Events")]
        [SerializeField] private UnityEvent OnMonsterSpawn;
        [SerializeField] private UnityEvent OnMonsterUseSkill;
        [SerializeField] private UnityEvent OnMonsterReceiveDamage;
        [SerializeField] private UnityEvent OnMonsterDeath;

        public MonsterSkillRunner SkillRunner => MonsterSkillRunner;

        /* ─────  Network Anim  ───────────── */
        private enum AnimState : byte { Idle = 0, Move = 1, Attack = 2, Skill = 3 }

#if FUSION2
        private static ulong _contactHitSeq = 0;
#endif

        private CancellationTokenSource flashEffectCts;
        private bool _contactLock;
        private float moveDelayRemaining;
        private bool hasDied;
        public DirectionalAnimSet IdleSet => idleSet;
        public DirectionalAnimSet MoveSet => moveSet;
        public SkillAnimData Attack => attack;
        public AnimClipData ReceiveDamage => receiveDamage;
        public AnimClipData Death => death;

#endregion

        #region ───── Private State ────────────────────────────────────────────

        private Transform target;       

        private bool isPaused = false;
        private bool isSpawned = false;

        #endregion

        private CharacterStats Stats => HealthComponent ? HealthComponent.Stats : null;

        #region ───── Unity Lifecycle ─────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            if (animator == null) animator = GetComponent<Animator>();

            if (useHFSM)
            {
                // ---------- 1. AnimatorOverrideController ----------
                var baseCtrl = animator.runtimeAnimatorController;
                var aoc = new AnimatorOverrideController(baseCtrl);
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                void Override(string state, AnimationClip clip)
                {
                    if (clip == null) return;
                    var original = Array.Find(aoc.animationClips, c => c.name == state);
                    if (original != null)
                        overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(original, clip));
                }

                // Helper
                string[] dirSuffix = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

                AnimClipData[] idleClips = {
                idleSet.Forward,       idleSet.ForwardRight, idleSet.Right,      idleSet.BackRight,
                idleSet.Back,          idleSet.BackLeft,     idleSet.Left,       idleSet.ForwardLeft
                };

                AnimClipData[] moveClips = {
                moveSet.Forward,       moveSet.ForwardRight, moveSet.Right,      moveSet.BackRight,
                moveSet.Back,          moveSet.BackLeft,     moveSet.Left,       moveSet.ForwardLeft
                };

                // Para Idle_Blend
                for (int i = 0; i < dirSuffix.Length; i++)
                {
                    var clip = idleClips[i].Clip ?? idleSet.Forward.Clip;
                    Override($"Idle_{dirSuffix[i]}", clip);
                }
                for (int i = 0; i < dirSuffix.Length; i++)
                {
                    var clip = moveClips[i].Clip ?? moveSet.Forward.Clip;
                    Override($"Run_{dirSuffix[i]}", clip);
                }

                // Attack / Hit / Death
                Override("Attack", attack.Clip);
                Override("ReceiveDamage", receiveDamage.Clip);
                Override("Death", death.Clip);

                // Skills
                for (int i = 0; i < skillAnimations.Length; ++i)
                    Override($"Skill_{i}", skillAnimations[i].Clip);

                aoc.ApplyOverrides(overrides);
                animator.runtimeAnimatorController = aoc;

                // ---------- 2. FSM ----------
                fsm = GetComponent<CharacterAnimationFSM>();
                if (fsm == null) fsm = gameObject.AddComponent<CharacterAnimationFSM>();
                fsm.enabled = useHFSM;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus.Subscribe<EnemyTargetEvent>(OnEnemytargeted);

            if (GameplayManager.Singleton && !GameplayManager.Singleton.ActiveMonstersList.Contains(transform))
            {
                GameplayManager.Singleton.ActiveMonstersList.Add(transform);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus.Unsubscribe<EnemyTargetEvent>(OnEnemytargeted);

            if (GameplayManager.Singleton)
                GameplayManager.Singleton.ActiveMonstersList.Remove(transform);
        }

        private void Update()
        {
#if FUSION2
            //if (Runner && !HasStateAuthority)
            //    return;
#endif

            if (!isSpawned || isPaused) return;
            if (moveDelayRemaining > 0f)
            {
                moveDelayRemaining -= Time.deltaTime;
                return;    
            }

            AcquireNearestTarget();
            MovementComponent.SetLookTarget(target);
            if (MovementComponent)        
            {
                if (target)
                {
                    float dist = Vector3.Distance(transform.position, target.position);

                    if (preferredRange <= 0f)
                    {
                        MovementComponent.SetLookTarget(target);
                        MovementComponent.Follow(target.position);                    
                    }                     
                    else if (dist > preferredRange + rangeTolerance)
                    {
                        MovementComponent.SetLookTarget(target);
                        MovementComponent.Follow(target.position);
                    }                            
                    else if (dist < preferredRange - rangeTolerance)
                    {
                        Vector3 away = transform.position -
                                       (target.position - transform.position).normalized *
                                       (preferredRange - dist);
                        MovementComponent.Flee(away);
                    }
                    else
                    {
                        MovementComponent.SetLookTarget(target);
                        MovementComponent.StrafeAround(target, preferredRange);
                    }                                                                    
                }
            }

            if (useHFSM && fsm)                
            {
                Vector3 wVel = MovementComponent ? MovementComponent.WorldVelocity
                                                 : Vector3.zero;

                if (wVel.sqrMagnitude < 0.001f)
                    wVel = Vector3.zero;

                Vector3 lVel = transform.InverseTransformDirection(wVel);  
                Vector2 moveXY = new(lVel.x, lVel.z);                    

                fsm.SetMove(moveXY);
            }
        }

        public void PlaySkillAnimNet(int skillIndex)
        {
            if (useHFSM) fsm.PlaySkill(skillIndex);
            OnMonsterUseSkill?.Invoke();
        }


        #endregion
        #region ───── Event Bus ─────────────────────────────────────────

        private void OnEnemytargeted(EnemyTargetEvent evt)
        {
            if(targetIndicator == null) return;

            targetIndicator.SetActive(evt.Target == this);
        }

        #endregion

        #region ───── Public Info ─────────────────────────────────────────────
        public void ConfigureMonster(int gold, int exp, bool network = false) => StartConfigureMonster(gold,exp);
        public MonsterHealth MonsterHealth => HealthComponent;
        public float GetCurrentHP => HealthComponent ? HealthComponent.CurrentHp : 0f;
        public float GetMaxHP => HealthComponent ? HealthComponent.MaxHp : 0f;
        public bool IsFinalBoss => isFinalBoss;
        public CharacterTypeData GetCharacterTypeData => characterTypeData;
        public Animator Animator => animator;
        public DirectionalAnimSet IdleSetAnim => idleSet;
        public DirectionalAnimSet MoveSetAnim => moveSet;
        public SkillAnimData AttackAnim => attack;
        public AnimClipData ReceiveDamageAnim => receiveDamage;
        public AnimClipData DeathAnim => death;

        public bool TryGetSkill(int index, out SkillAnimData data)
        {
            if (skillAnimations != null && index >= 0 && index < skillAnimations.Length)
            {
                data = skillAnimations[index];
                return true;
            }
            data = default;
            return false;
        }

        #endregion

        #region ───── Monster Setup ─────────────────────────────────────────────

        protected override void StartConfigureMonster(int gold, int exp)
        {
            base.StartConfigureMonster(gold, exp);
#if FUSION2
            bool netMode = GameplaySync.Instance && GameplaySync.Instance.RunnerActive;
            bool host = netMode ? GameplaySync.Instance.IsHost : true;
#else
            bool netMode = false;
            bool host = true;
#endif
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!shootOrigin) shootOrigin = transform;
            if (!HealthComponent)
            {
                Debug.LogError($"{name} is missing MonsterHealth.");
                return;
            }

            HealthComponent.Setup(stats, damagePopupContainer, hpContainer,
                  hpBar, hpBarDecrease, decreaseDelay, decreaseSpeed);

            HealthComponent.OnHealthChanged += HandleMonsterDamaged;
            HealthComponent.OnDeath += HandleMonsterDied;
           
            if (MovementComponent)
            {
                MovementComponent.Configure(stats.baseMoveSpeed);

#if FUSION2
                if (Runner && !HasStateAuthority)
                    MovementComponent.DisableAgentForReplica();
#endif
            }

            moveDelayRemaining = delayToMoveAfterSpawn;

            if (MonsterSkillRunner && canUseSkill)
                MonsterSkillRunner.Setup(this, monsterSkills, MovementComponent, shootOrigin,
                                          delayToMoveAfterSpawn,
                                          netMode, host);

            MonsterSkillRunner.OnUseSkill += HandleMonsterUseSkill;

            isSpawned = true;
            OnMonsterSpawn.Invoke();

        }

#endregion

        #region ───── Targeting & Movement ───────────────────────────────────

        private void AcquireNearestTarget()
        {
            float minSqr = float.PositiveInfinity;
            Transform best = null;

            foreach (var t in GameplayManager.Singleton.ActiveCharactersList)
            {
                float d = (t.position - transform.position).sqrMagnitude;
                if (d < minSqr) { minSqr = d; best = t; }
            }
            target = best;
        }

        public bool TryGetTarget(out Transform tgt, out Vector3 dir, float minDist = 0f)
        {
            tgt = null;
            dir = Vector3.zero;

            AcquireNearestTarget();
            if (target == null) return false;

            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < minDist) return false;

            tgt = target;
            dir = (target.position - transform.position).normalized;
            return true;
        }

        #endregion

        #region ───── Contact Damage ─────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
#if FUSION2
            if (Runner && !HasStateAuthority)
                return;
#endif
            if (_contactLock || GameplayManager.Singleton.IsPaused())
                return;

            if (!other.CompareTag("Character")) return;

            var player = other.GetComponent<CharacterEntity>();
            if (!player) return;

            bool isCrit = UnityEngine.Random.value < Stats.baseCriticalRate;
            float critMul = isCrit ? Stats.baseCriticalDamageMultiplier : 1f;
            float raw = Stats.baseDamage * critMul;

            raw = GameInstance.Singleton.TotalDamageWithElements(
                      characterTypeData,
                      player.GetCharacterType(),
                      raw);

            float dmg = Mathf.Max(raw - player.GetCurrentDefense(),
                                  GameplayManager.Singleton.minDamage);

#if FUSION2
            if (Runner && Runner.IsRunning)
            {
                var attackerNob = GetComponent<NetworkObject>();
                ulong hitId = ++_contactHitSeq; 
                player.ApplyDamageToSelfFromHit(attackerNob, hitId, dmg, isCrit);
            }
            else
            {
                player.ReceiveDamage(dmg);
            }
#else
            player.ReceiveDamage(dmg);
#endif
            if (useHFSM) fsm.PlayAttack();
            _ = ContactCooldownAsync();
        }


        private async UniTaskVoid ContactCooldownAsync()
        {
            _contactLock = true;
            await UniTask.Delay(TimeSpan.FromSeconds(attackCooldown));
            _contactLock = false;
        }

        #endregion

        #region ───── Stats / Status Effects ─────────────────────────────────────────

        /// <summary>
        /// Forwards incoming damage to the character's health component.
        /// Supports optional critical hit flag.
        /// </summary>
        /// <param name="amount">Amount of damage to apply.</param>
        /// <param name="critical">Whether the hit is a critical strike (default: false).</param>
        public void ApplyDamage(float amount, bool critical = false)
        {
            HealthComponent.ApplyDamageRequest(amount, critical);
        }

        /// <summary>
        /// Applies a temporary slow effect to the character's movement.
        /// </summary>
        /// <param name="pct">The percentage to slow the movement (e.g., 0.5 for 50% speed).</param>
        /// <param name="dur">The duration of the slow effect in seconds.</param>
        public void Slow(float pct, float dur)
        {
            MovementComponent.ApplySlow(pct, dur);
            if (showSlowEffect) PlayFlashEffect(slowEffectColor, dur);
        }

        /// <summary>
        /// Applies a temporary stun effect to the character, disabling movement.
        /// </summary>
        /// <param name="dur">The duration of the stun effect in seconds.</param>
        public void Stun(float dur)
        {
            MovementComponent.ApplyStun(dur);
            if (showStunEffect) PlayFlashEffect(stunEffectColor, dur);
        }

        /// <summary>
        /// Applies a knockback effect to the character from its current position,
        /// pushing it a specified distance over a set duration.
        /// </summary>
        ///  <param name="dist">The origin vector.</param>
        /// <param name="dist">The distance to push the character.</param>
        /// <param name="dur">The duration over which the knockback occurs.</param>      
        public void Knockback(Vector3 origin, float dist, float dur)
        {
            MovementComponent.ApplyKnockback(origin, dist, dur);
        }

        /// <summary>
        /// Applies life steal based on the damage dealt,
        /// healing the character by a percentage defined in the base characterStatsComponent.
        /// </summary>
        /// <param name="damageDealt">The amount of damage dealt to an enemy.</param>
        public void ApplyHpLeech(float damageDealt)
        {
            float amount = damageDealt * (Stats.baseHPLeech / 100f);
            HealthComponent.ApplyHeal(amount);
        }

        /// <summary>
        /// Applies a damage-over-time (DoT) effect using the given skill,
        /// dealing total damage over the specified duration. Triggers a visual effect if enabled.
        /// </summary>
        /// <param name="skill">The skill that causes the DoT effect.</param>
        /// <param name="totalDmg">Total damage to be dealt over time.</param>
        /// <param name="dur">Duration of the DoT effect in seconds.</param>
        public void ApplyDOT(SkillData skill, int totalDmg, float dur)
        {
            HealthComponent.ApplyDot(skill, totalDmg, dur);
            if (showDOTEffect) PlayFlashEffect(dotEffectColor, dur);
        }

        /// <summary>
        /// Applies a damage-over-time (DoT) effect using the given skill perk,
        /// dealing total damage over the specified duration. Triggers a visual effect if enabled.
        /// </summary>
        /// <param name="skillPerk">The skill perk that causes the DoT effect.</param>
        /// <param name="totalDmg">Total damage to be dealt over time.</param>
        /// <param name="dur">Duration of the DoT effect in seconds.</param>
        public void ApplyDOT(SkillPerkData skillPerk, int totalDmg, float dur)
        {
            HealthComponent.ApplyDot(skillPerk, totalDmg, dur);
            if (showDOTEffect) PlayFlashEffect(dotEffectColor, dur);
        }

        /// <summary>
        /// Flashes the character's renderers with a given color at set intervals for the specified duration,
        /// used to visually indicate status effects like stun or damage-over-time.
        /// </summary>
        /// <param name="effectColor">The color to flash.</param>
        /// <param name="duration">Total duration of the flashing effect in seconds.</param>
        public void PlayFlashEffect(Color effectColor, float duration)
        {
            flashEffectCts?.Cancel();
            flashEffectCts = new CancellationTokenSource();

            FlashEffectAsync(effectColor, duration, flashEffectCts.Token).Forget();
        }

        /// <summary>
        /// Async method that flashes the renderers' color to indicate visual effects (e.g., stun).
        /// </summary>
        /// <param name="effectColor">Flash color.</param>
        /// <param name="duration">Total duration of the flash effect.</param>
        /// <param name="token">Cancellation token to cancel effect early.</param>
        private async UniTaskVoid FlashEffectAsync(Color effectColor, float duration, CancellationToken token)
        {
            float elapsed = 0f;
            float interval = effectFlashInterval;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            try
            {
                while (elapsed < duration)
                {
                    foreach (var r in renderers)
                        r.material.color = effectColor;

                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);

                    foreach (var r in renderers)
                        r.material.color = Color.white;

                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);

                    elapsed += interval * 2f;
                }
            }
            catch (OperationCanceledException)
            {
                foreach (var r in renderers)
                    r.material.color = Color.white;
            }
        }

        #endregion
        /// <summary>
        /// Called when the monster use skill.
        /// </summary>
        private void HandleMonsterUseSkill()
        {
            if (useHFSM) fsm.PlaySkill(MonsterSkillRunner.LastSkillIndex);

            OnMonsterUseSkill?.Invoke();
        }

        /// <summary>
        /// Called whenever the monster receives damage.
        /// </summary>
        /// <param name="currentHp">The new HP value after taking damage.</param>
        private void HandleMonsterDamaged(float currentHp)
        {
            if (useHFSM && playHitAnim) fsm.PlayReceiveDamage();
            OnMonsterReceiveDamage?.Invoke();
        }

        /// <summary>
        /// Called when the monster's HP reaches zero.
        /// </summary>
        private void HandleMonsterDied()
        {
            //Effects
            if (useHFSM) fsm.PlayDeath();
            if (deathAudio) AudioManager.Singleton.PlayAudio(deathAudio, "vfx", transform.position);
            OnMonsterDeath?.Invoke();
        }
        protected override void HandleDeath()
        {
            //Logic
            base.HandleDeath();
        }
    }
}

