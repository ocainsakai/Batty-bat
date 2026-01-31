using BulletHellTemplate.Core.Events;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Unity.Collections.Unicode;


#if FUSION2
using Fusion;
#endif


namespace BulletHellTemplate
{
    public partial class CharacterAttackComponent : MonoBehaviour
    {
        private CharacterData characterData;
        private CharacterBuffsComponent characterBuffsComponent;
        private CharacterControllerComponent characterControllerComponent;
        private CharacterStatsComponent characterStatsComponent;
        private CharacterEntity characterOwner;
        public CharacterEntity CharacterOwner => characterOwner;

        private Transform launchTransform;
        private Transform effectsTransform;

        private CancellationTokenSource autoAttackCts;
        private readonly Dictionary<SkillPerkData, CancellationTokenSource> activeSkillTokens = new();
        private readonly HashSet<SkillPerkData> skillsPerkData = new();
        private bool ShouldDriveAttacks
        {
            get
            {
#if FUSION2
                if (GameplayManager.Singleton.IsRunnerActive)
                {
                    var no = GetComponent<NetworkObject>();
                    return no && no.HasInputAuthority;
                }
#endif
                return true; // offline
            }
        }

        private void Awake()
        {

            characterBuffsComponent = GetComponent<CharacterBuffsComponent>();
            characterControllerComponent = GetComponent<CharacterControllerComponent>();
            characterStatsComponent = GetComponent<CharacterStatsComponent>();
            characterOwner = GetComponent<CharacterEntity>();
        }

        public void Initialize(CharacterData data, Transform launch, Transform effects)
        {
            characterData = data;
            launchTransform = launch;
            effectsTransform = effects;

            if (ShouldDriveAttacks)
                StartAutoAttack(this.GetCancellationTokenOnDestroy());
        }


        /// <summary>
        /// Starts auto-attack loop with cancelable token.
        /// </summary>
        private void StartAutoAttack(CancellationToken externalToken)
        {
            autoAttackCts?.Cancel();
            autoAttackCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            AutoAttackLoop(autoAttackCts.Token).Forget();
        }

        /// <summary>
        /// Cancels the auto-attack loop.
        /// </summary>
        public void StopAutoAttack()
        {
            autoAttackCts?.Cancel();
            autoAttackCts = null;
        }
        public void ResumeAutoAttack()
        {
            if (ShouldDriveAttacks)
                StartAutoAttack(this.GetCancellationTokenOnDestroy());
        }

        /// <summary>
        /// Cancels the current auto-attack loop and restarts it after <paramref name="delay"/> seconds.
        /// </summary>
        /// <param name="delay">Time in seconds before auto attack resumes.</param>
        public void ResetAutoAttackDelay(float delay)
        {
            if (!ShouldDriveAttacks) return;
            StopAutoAttack();
            ResumeAutoAttackAfterDelayAsync(delay).Forget();
        }

        private async UniTaskVoid ResumeAutoAttackAfterDelayAsync(float delay)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay),
                                    cancellationToken: this.GetCancellationTokenOnDestroy());
                StartAutoAttack(this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { }
        }
        private static bool ShouldWaitPvpRules()
        {
#if FUSION2
            var gm = GameplayManager.Singleton;
            return gm != null
                && gm.IsRunnerActive     
                && gm.IsPvp              
                && PvpSync.Instance != null
                && !PvpSync.Instance.RulesReady;
#else
         return false;
#endif
        }

        /// <summary>
        /// Performs auto-attacks with dynamic cooldown using UniTask.
        /// </summary>
        private async UniTaskVoid AutoAttackLoop(CancellationToken token)
        {
            Debug.Log($"[ATK] Start loop – auth={ShouldDriveAttacks} pvp={GameplayManager.Singleton.IsPvp}");
            try
            {
                while (!token.IsCancellationRequested)
                {
                    while (GameplayManager.Singleton.IsPaused() ||
                        characterOwner == null ||
                        characterOwner.IsDead ||
                        !ShouldDriveAttacks
#if FUSION2
                        || (GameplayManager.Singleton.IsPvp && (PvpSync.Instance != null && !PvpSync.Instance.RulesReady))
#endif
                       )
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }

                    Attack();
                    float cooldown = Mathf.Max(0.05f, characterStatsComponent.CurrentAttackSpeed);
                    await UniTask.Delay(TimeSpan.FromSeconds(cooldown), cancellationToken: token);
                }
            }
            catch (OperationCanceledException) { }
        }


        /// <summary>
        /// Performs a skill use: applies damage locally and replicates visuals to other clients.
        /// </summary>
        /// <param name="index">Index of the skill in characterData.skills.</param>
        /// <param name="skill">The SkillData to use.</param>
        /// <param name="inputDirection">Raw input direction vector.</param>
        public void UseSkill(int index, SkillData skill, Vector2 inputDirection)
        {
#if FUSION2
            if (PvpSync.Instance && !PvpSync.Instance.RulesReady) { Debug.Log("[SKILL] blocked: RulesReady==false"); return; }
#endif
            if (characterOwner == null || characterOwner.IsDead) return;
            if (!ShouldDriveAttacks) return;


#if FUSION2
            bool runnerActive = false;
            runnerActive = GameplayManager.Singleton != null && GameplayManager.Singleton.IsRunnerActive;
#endif

            bool amAuthority =
#if FUSION2
                !runnerActive || (characterOwner && characterOwner.Object && characterOwner.Object.HasStateAuthority);
#else
        true;
#endif

            UseSkillInternal(index, skill, inputDirection, isReplica: !amAuthority);

#if FUSION2
            if (runnerActive)
                GameplaySync.Instance?.SyncPlayerUseSkill(characterOwner, index, inputDirection);
#endif
        }

        /// <summary>
        /// Internal skill logic: spawns visuals and optionally applies damage.
        /// </summary>
        /// <param name="index">Index of the skill in characterData.skills.</param>
        /// <param name="skill">The SkillData to use.</param>
        /// <param name="inputDirection">Raw input direction vector.</param>
        /// <param name="isReplica">True = visual only; False = visual + damage.</param>
        public void UseSkillInternal(int index, SkillData skill, Vector2 inputDirection, bool isReplica)
        {
            if (characterOwner == null || characterOwner.IsDead) return;
            // Spawn effect
            if (skill.spawnEffect != null)
            {
                var effectInstance = GameEffectsManager.SpawnEffect(
                   skill.spawnEffect,
                   effectsTransform.position,
                   effectsTransform.rotation);

                var auto = effectInstance.GetComponent<ReturnEffectToPool>() ??
                        effectInstance.AddComponent<ReturnEffectToPool>();
            }

            // Play spawn audio
            if (skill.spawnAudio != null && !skill.playSpawnAudioEaShot)
                AudioManager.Singleton.PlayAudio(skill.spawnAudio, "vfx");

            // Play skill audio
            if (skill.skillAudio != null && !skill.playSkillAudioEaShot)
                PlaySkillAudioAsync(skill.skillAudio, skill.playSkillAudioAfter, this.GetCancellationTokenOnDestroy()).Forget();

            // Apply buffs/debuffs
            ApplySkillBuffs(skill);

            // Determine skill direction
            Vector3 dir = GetSkillDirection(skill.AimMode, inputDirection, launchTransform);

            // Rotate model if needed
            if (skill.isRotateToEnemy)
                characterOwner.ApplyRotateCharacterModel(dir, skill.rotateDuration, this.GetCancellationTokenOnDestroy());

            // Movement delay
            if (skill.delayToMove > 0f)
                characterOwner.ApplyStopMovement(skill.delayToMove, skill.canRotateWhileStopped);

            // Dash logic
            if (skill.advancedDashSettings != null && skill.advancedDashSettings.enableAdvancedDash)
            {
                HandleAdvancedDash(skill.advancedDashSettings, dir, FindNearestTarget(), index);
            }
            else if (skill.isDash)
            {
                Vector3 dashDir = skill.isReverseDash ? -dir : dir;
                EventBus.Publish(new PlayerDashEvent(characterOwner, dashDir, skill.dashSpeed, skill.dashDuration, this.GetCancellationTokenOnDestroy()));
            }

            // Reset auto‐attack cooldown
            ResetAutoAttackDelay(skill.autoAttackDelay);

            // Launch damage or skip if replica
            _ = LaunchSkillAsync(
                skill,
                launchTransform,
                dir,
                isAutoAttack: false,
                index: index,
                isReplica,
                this.GetCancellationTokenOnDestroy()
            );
        }



        /// <summary>
        /// Public API to perform auto‐attack: dispara dano local + RPC para réplica visual.
        /// </summary>
        public void Attack()
        {
            if (characterOwner == null || characterOwner.IsDead) return;
            if (!ShouldDriveAttacks) return;

            Transform target = FindNearestTarget();
            if (target == null) return;

            Vector3 dir = (target.position - launchTransform.position);
            dir.y = 0f;
            dir.Normalize();


#if FUSION2
            bool runnerActive = false;
            runnerActive = GameplayManager.Singleton != null && GameplayManager.Singleton.IsRunnerActive;
#endif

            bool amAuthority =
#if FUSION2
                !runnerActive || (characterOwner && characterOwner.Object && characterOwner.Object.HasStateAuthority);
#else
        true;
#endif

            AttackInternal(dir, isReplica: !amAuthority);

#if FUSION2
            if (runnerActive)
                GameplaySync.Instance?.SyncPlayerAttack(characterOwner, dir);
#endif
        }

        /// <summary>
        /// Internal attack logic: spawns visuals and optionally applies damage.
        /// </summary>
        /// <param name="direction">Normalized attack direction.</param>
        /// <param name="isReplica">
        /// If true, only visual effects/play animations; 
        /// if false, also launch damage entities.
        /// </param>
        public void AttackInternal(Vector3 direction, bool isReplica)
        {
            characterOwner.PlayAttack();
            var skill = characterData.autoAttack;

            if (skill.spawnEffect != null)
            {
                var effectInstance = GameEffectsManager.SpawnEffect(
                  skill.spawnEffect,
                  effectsTransform.position,
                  effectsTransform.rotation);

                var auto = effectInstance.GetComponent<ReturnEffectToPool>() ??
                        effectInstance.AddComponent<ReturnEffectToPool>();
            }
            // Play spawn audio
            if (skill.spawnAudio != null && !skill.playSpawnAudioEaShot)
                AudioManager.Singleton.PlayAudio(skill.spawnAudio, "vfx");

            // Play skill audio
            if (skill.skillAudio != null && !skill.playSkillAudioEaShot)
                PlaySkillAudioAsync(skill.skillAudio, skill.playSkillAudioAfter, this.GetCancellationTokenOnDestroy()).Forget();


            if (skill.isRotateToEnemy) characterOwner.ApplyRotateCharacterModel(direction, skill.rotateDuration, this.GetCancellationTokenOnDestroy());

            _ = LaunchSkillAsync(
                skill,
                launchTransform,
                direction,
                isAutoAttack: true,
                index: -1,
                isReplica,
                this.GetCancellationTokenOnDestroy()
            );
        }


        /// <summary>
        /// Launches a skill after an optional delay, without waiting for the animation to finish.
        /// Uses async UniTask with cancellation for performance.
        /// </summary>
        /// <param name="skill">The skill data.</param>
        /// <param name="launchTransform">The transform from which the skill originates.</param>
        /// <param name="direction">The direction to launch the skill.</param>
        /// <param name="isAutoAttack">If true, triggers attack animation instead of skill.</param>
        /// <param name="index">The index used for naming the trigger animation.</param>
        /// <param name="token">Optional cancellation token.</param>
        public async UniTask LaunchSkillAsync(
            SkillData skill,
            Transform launchTransform,
            Vector3 direction,
            bool isAutoAttack,
            int index,
            bool isReplica,
            CancellationToken token = default)
        {
            string trigger = isAutoAttack ? "Attack" : $"UseSkill{index}";

            if (isAutoAttack)
                characterOwner.PlayAttack();
            else
                characterOwner.PlaySkill(index);

            if (skill.delayToLaunch > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(skill.delayToLaunch), cancellationToken: token);

            skill.LaunchDamage(launchTransform, direction, characterOwner, isReplica);
        }

        public List<SkillPerkData> GetSkillsPerkData() => skillsPerkData.ToList();

        /// <summary>
        /// Applies a skill perk and manages its execution based on level and cooldown.
        /// </summary>
        /// <param name="skillPerk">The skill perk to apply.</param>
        public void ApplySkillPerk(SkillPerkData skillPerk, bool isReplica)
        {
            if (skillPerk == null)
            {
                Debug.LogWarning("SkillPerkData is null.");
                return;
            }

            int currentLevel = GameplayManager.Singleton.GetSkillLevel(skillPerk);

            if (skillsPerkData.Contains(skillPerk))
            {
                if (activeSkillTokens.TryGetValue(skillPerk, out var oldToken))
                {
                    oldToken.Cancel();
                    oldToken.Dispose();
                }

                var newToken = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                activeSkillTokens[skillPerk] = newToken;

                ExecuteSkillAsync(skillPerk, currentLevel, isReplica, newToken.Token).Forget();
            }
            else
            {
                skillsPerkData.Add(skillPerk);

                var token = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                activeSkillTokens.Add(skillPerk, token);

                ExecuteSkillAsync(skillPerk, currentLevel, isReplica, token.Token).Forget();
            }
        }

        /// <summary>
        /// Executes a skill repeatedly based on cooldown and current targets.
        /// </summary>
        /// <param name="skillPerk">The skill perk data.</param>
        /// <param name="level">The current level of the skill.</param>
        /// <param name="token">Cancellation token.</param>
        private async UniTask ExecuteSkillAsync(SkillPerkData skillPerk, int level,bool isReplica ,CancellationToken token)
        {
            float cooldown = skillPerk.withoutCooldown ? 0f : skillPerk.cooldown;

            while (!token.IsCancellationRequested)
            {
                while (GameplayManager.Singleton.IsPaused())
                    await UniTask.Yield(PlayerLoopTiming.Update, token);

                token.ThrowIfCancellationRequested();

                Transform nearestTarget = FindNearestTarget();

                if (nearestTarget == null)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(cooldown), cancellationToken: token);
                    continue;
                }

                Vector3 direction = (nearestTarget.position - transform.position).normalized;

                // Skill multishot
                if (skillPerk.isMultiShot)
                {
                    bool isMaxLevel = level == skillPerk.maxLevel;
                    int shots = isMaxLevel && skillPerk.evolveChanges ? skillPerk.shotsEvolved : skillPerk.shots;
                    float angle = isMaxLevel && skillPerk.evolveChanges ? skillPerk.angleEvolved : skillPerk.angle;
                    float delay = isMaxLevel && skillPerk.evolveChanges ? skillPerk.delayEvolved : skillPerk.delay;

                    await MultiShotDamageEntityAsync(skillPerk, level, direction, shots, angle, delay, isReplica, token);
                }
                else
                {
                    SpawnDamageEntity(skillPerk, level, direction,isReplica);
                }

                // Dash
                if (skillPerk.isDash)
                {
                    EventBus.Publish(new PlayerDashEvent(
                        characterOwner,
                        transform.forward,
                        skillPerk.dashSpeed,
                        skillPerk.dashDuration,
                        token));
                }

                // Shield
                if (skillPerk.isShield)
                {
                    characterBuffsComponent.ApplyShield(
                        skillPerk.shieldAmount,
                        skillPerk.shieldDuration,
                        true);
                }
                float cooldownReduction = characterStatsComponent.CurrentCooldownReduction / 100f;
                float finalCooldown = cooldown * (1 - cooldownReduction);

                await UniTask.Delay(TimeSpan.FromSeconds(finalCooldown), cancellationToken: token);
            }
        }


        /// <summary>
        /// Spawns a damage entity from a SkillPerk at a given level,
        /// re-using the pool and configuring all runtime data.
        /// </summary>
        /// <param name="skillPerk">SkillPerk data.</param>
        /// <param name="level">Current level of that perk.</param>
        /// <param name="direction">Launch direction.</param>
        private void SpawnDamageEntity(SkillPerkData perk, int level, Vector3 dir, bool isReplica)
        {
            DamageEntity prefab = level >= perk.maxLevel
                                  ? perk.maxLvDamagePrefab
                                  : perk.initialDamagePrefab;

            if (prefab == null) return;

#if FUSION2
            var runner = FusionHelpers.GetRunner(characterOwner);
            bool canNetSpawn =
                runner != null &&
                runner.IsRunning &&
                characterOwner != null &&
                characterOwner.Object != null &&
                characterOwner.Object.HasStateAuthority;

            if (canNetSpawn)
            {
                runner.Spawn(
                    prefab.gameObject,
                    launchTransform.position,
                    Quaternion.LookRotation(dir),
                    null,
                    (r, obj) =>
                    {
                        var de = obj.GetComponent<DamageEntity>();
                        de.Init(new SkillPerkDamageProvider(perk, level, isReplica),
                                characterOwner,
                                dir.normalized * perk.speed,
                                isReplica);
                    });
                return;
            }
#endif

            // ─── OFFLINE / local ───
            GameObject go = GameEffectsManager.SpawnEffect(
                prefab.gameObject,
                launchTransform.position,
                Quaternion.LookRotation(dir));

            var deLocal = go.GetComponent<DamageEntity>();
            deLocal.Init(new SkillPerkDamageProvider(perk, level, isReplica),
                         characterOwner,
                         dir.normalized * perk.speed,
                         isReplica);
        }

        /// <summary>
        /// Finds the nearest target with the tags "Monster" or "Box".
        /// </summary>
        /// <returns>The transform of the nearest target, or null if no targets are found.</returns>
        public Transform FindNearestMonster()
        {
            float bestSqr = float.PositiveInfinity;
            Transform best = null;

            foreach (var t in GameplayManager.Singleton.ActiveMonstersList)
            {
                if (!t) continue;
                float d = (t.position - transform.position).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = t; }
            }

            foreach (var box in GameplayManager.Singleton.ActiveBoxesList)
            {
                float d = (box.position - transform.position).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = box; }
            }

            if (best && best.TryGetComponent(out MonsterEntity me))
                EventBus.Publish(new EnemyTargetEvent(me));

            return best;
        }

        private Transform FindNearestTarget()
        {
            var gm = GameplayManager.Singleton;
            if (gm != null && gm.IsPvp)
                return FindNearestEnemyPlayer();
            return FindNearestMonster(); 
        }

        private Transform FindNearestEnemyPlayer()
        {
#if FUSION2
            if (GameplayManager.Singleton.IsPvp &&
                (PvpSync.Instance != null && !PvpSync.Instance.RulesReady))
                return null;
#endif

#if UNITY_6000_0_OR_NEWER
            var all = FindObjectsByType<CharacterEntity>(FindObjectsSortMode.None);
#else
            var all = FindObjectsOfType<CharacterEntity>();
#endif

            var gm = GameplayManager.Singleton;
            bool pvp = gm != null && gm.IsPvp;

            bool pvpReady = false;
#if FUSION2
            pvpReady = pvp && PvpSync.Instance != null && PvpSync.Instance.RulesReady;
#endif

            byte myTeam = 0;
#if FUSION2
            if (characterOwner) myTeam = characterOwner.TeamId;
#endif

            float bestSqr = float.PositiveInfinity;
            Transform best = null;

            foreach (var ce in all)
            {
                if (!ce || ce == characterOwner || ce.IsDead) continue;

                byte otherTeam = 1; // default
#if FUSION2
                otherTeam = ce.TeamId;
#endif
                if (pvpReady && otherTeam == myTeam) continue;

                float d = (ce.transform.position - transform.position).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = ce.transform; }
            }
            return best;
        }

      

        /// <summary>
        /// Spawns multiple damage entities with angle and delay using UniTask.
        /// </summary>
        /// <param name="skillPerk">The skill perk data.</param>
        /// <param name="level">Skill level.</param>
        /// <param name="baseDirection">Base direction for the first shot.</param>
        /// <param name="shots">Number of shots to fire.</param>
        /// <param name="angle">Angle between each shot.</param>
        /// <param name="delay">Delay between each shot in seconds.</param>
        /// <param name="token">Cancellation token (optional).</param>
        private async UniTask MultiShotDamageEntityAsync(
            SkillPerkData skillPerk,
            int level,
            Vector3 baseDirection,
            int shots,
            float angle,
            float delay,
            bool isReplica,
            CancellationToken token = default)
        {
            float halfAngle = (shots - 1) * angle / 2f;

            for (int i = 0; i < shots; i++)
            {
                token.ThrowIfCancellationRequested();

                while (GameplayManager.Singleton.IsPaused())
                    await UniTask.Yield(PlayerLoopTiming.Update, token);

                float shotAngle = -halfAngle + i * angle;
                Quaternion rotation = Quaternion.Euler(0f, shotAngle, 0f);
                Vector3 shotDirection = rotation * baseDirection;

                SpawnDamageEntity(skillPerk, level, shotDirection, isReplica);

                if (delay > 0f && i < shots - 1)
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
            }
        }


        /// <summary>
        /// Applies the skill's characterBuffsComponent and debuffs to this character (heal, shield, speed buff, etc.).
        /// </summary>
        /// <param name="skill">The skill data containing buff/debuff options.</param>
        public void ApplySkillBuffs(SkillData skill)
        {
            if (skill.receiveHeal && skill.healAmount > 0f)
            {
                characterBuffsComponent.ApplyHealHp((int)skill.healAmount);
            }

            if (skill.receiveShield && skill.shieldAmount > 0)
            {
                characterBuffsComponent.ApplyShield(skill.shieldAmount, skill.shieldDuration, true);
            }

            if (skill.receiveMoveSpeed && skill.moveSpeedAmount > 0f)
            {
                characterBuffsComponent.ApplyMoveSpeedBuff(skill.moveSpeedAmount, skill.moveSpeedDuration);
            }

            if (skill.receiveAttackSpeed && skill.attackSpeedAmount > 0f)
            {
                characterBuffsComponent.ApplyAttackSpeedBuff(skill.attackSpeedAmount, skill.attackSpeedDuration);
            }

            if (skill.receiveDefense && skill.defenseAmount > 0f)
            {
                characterBuffsComponent.ApplyDefenseBuff(skill.defenseAmount, skill.defenseDuration);
            }

            if (skill.receiveDamage && skill.damageAmount > 0f)
            {
                characterBuffsComponent.ApplyDamageBuff(skill.damageAmount, skill.damageDuration);
            }

            if (skill.isInvincible && skill.invincibleDuration > 0f)
            {
                characterOwner.ApplyInvincible(skill.invincibleDuration);
            }

            if (skill.receiveSlow && skill.slowDuration > 0f)
            {
                float slow = Mathf.Abs(skill.receiveSlowAmount);
                characterBuffsComponent.ApplyMoveSpeedDebuff(slow, skill.slowDuration);
            }
        }

        /// <summary>
        /// Converts a 2D direction vector to a 3D direction.
        /// </summary>
        /// <param name="direction">The 2D direction vector.</param>
        /// <returns>The 3D direction vector.</returns>
        private Vector3 GetDirection(Vector2 direction)
        {
            return new Vector3(direction.x, 0, direction.y);
        }

        /// <summary>
        /// Determines the skill direction based on the provided AimType and input direction.
        /// </summary>
        /// <param name="aimMode">The aiming mode defined by the skill.</param>
        /// <param name="inputDirection">The input direction vector (e.g., from joystick).</param>
        /// <param name="launchTransform">The transform from which the skill is launched.</param>
        /// <returns>The direction vector in which the skill should be launched.</returns>
        private Vector3 GetSkillDirection(AimType aimMode, Vector2 inputDirection, Transform launchTransform)
        {
            switch (aimMode)
            {
                case AimType.InputDirection:
                    return GetDirection(inputDirection).normalized;

                case AimType.TargetNearestEnemy:
                    Transform nearestTarget = FindNearestTarget();
                    return (nearestTarget != null)
                        ? (nearestTarget.position - launchTransform.position).normalized
                        : transform.forward;

                case AimType.FowardDirection:
                    return transform.forward;

                case AimType.ReverseDirection:
                    return -transform.forward;

                case AimType.RandomDirection:
                    return GetRandomDirection();

                default:
                    return transform.forward;
            }
        }

        /// <summary>
        /// Generates a random direction on the XZ plane.
        /// </summary>
        /// <returns>A normalized Vector3 representing a random direction.</returns>
        private Vector3 GetRandomDirection()
        {
            float randomAngle = UnityEngine.Random.Range(0f, 360f);
            float radian = randomAngle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian)).normalized;
        }

        private async void HandleAdvancedDash(AdvancedDashSettings dashSettings, Vector3 dir, Transform nearestTarget, int skillIndex)
        {
            await HandleAdvancedDashAsync(dashSettings, dir, nearestTarget, skillIndex, this.GetCancellationTokenOnDestroy());
        }

        /// <summary>
        /// Coroutine to handle advanced dash without waiting for the animation to complete.
        /// Each dash wave may trigger an action animation (UseSkillX) if AnimationTriggerEachDash is true.
        /// </summary>
        /// <param name="dashSettings">Advanced dash settings.</param>
        /// <param name="dir">The dash direction input.</param>
        /// <param name="nearestTarget">Nearest target transform (if any).</param>
        /// <param name="skillIndex">Index for the UseSkill animation trigger.</param>
        private async UniTask HandleAdvancedDashAsync(AdvancedDashSettings dashSettings, Vector3 dir, Transform nearestTarget, int skillIndex, CancellationToken cancellationToken)
        {
            for (int i = 0; i < dashSettings.dashWaves; i++)
            {
                Vector3 dashDir = dashSettings.dashMode switch
                {
                    DashMode.InputDirection => dir.normalized,
                    DashMode.ForwardOnly => transform.forward,
                    DashMode.ReverseOnly => -dir.normalized,
                    DashMode.NearestTarget => (nearestTarget != null)
                        ? (nearestTarget.position - transform.position).normalized
                        : transform.forward,
                    DashMode.RandomDirection => GetRandomDirection(),
                    _ => transform.forward
                };

                if (dashSettings.AnimationTriggerEachDash)
                {
                    characterOwner.PlaySkill(skillIndex);
                }

                var dashCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                EventBus.Publish(new PlayerDashEvent(
                    characterOwner,
                    dashDir,
                    dashSettings.dashSpeed,
                    dashSettings.dashDuration,
                    dashCts.Token
                ));

                await UniTask.Delay(TimeSpan.FromSeconds(dashSettings.dashDuration), cancellationToken: cancellationToken);

                if (i < dashSettings.dashWaves - 1 && dashSettings.delayBetweenWaves > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(dashSettings.delayBetweenWaves), cancellationToken: cancellationToken);
                }
            }
        }
        /// <summary>
        /// Plays a skill audio clip after a delay.
        /// </summary>
        /// <param name="skillAudio">The audio clip to play.</param>
        /// <param name="delay">Delay in seconds before playing the clip.</param>
        /// <param name="token">Optional cancellation token.</param>
        private async UniTask PlaySkillAudioAsync(AudioClip skillAudio, float delay, CancellationToken token = default)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            AudioManager.Singleton.PlayAudio(skillAudio, "vfx");
        }

    }
}
