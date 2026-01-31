using System.Collections;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// World-space trap spawned by a <see cref="MonsterSkill"/>.<br/>
    /// • **Damage** the first <u>N</u> characters that touch it (or infinite when
    ///   <see cref="maxActivations"/> is 0).<br/>
    /// • Optionally applies <b>Stun</b> and/or <b>DOT</b> to the target.<br/>
    /// • Self-destructs after <see cref="lifeTime"/> seconds or when the
    ///   activation limit is reached.<br/><br/>
    /// All runtime values are injected through
    /// <see cref="ConfigureFromSkill(MonsterSkill,MonsterEntity)"/>; the
    /// serialized fields exist only as safe defaults for isolated testing.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public sealed class MonsterTrapEntity : MonoBehaviour
    {
        // ─────────────── Serialized fallbacks (optional) ───────────────
        [Header("Default Values (editor tests)")]
        [SerializeField] private int defaultDamage = 10;
        [SerializeField] private CharacterTypeData defaultDamageType;
        [SerializeField] private float defaultLifeTime = 6f;
        [SerializeField] private int defaultMaxUses = 0;

        // Status-effect defaults
        [SerializeField] private bool defaultApplyStun = false;
        [SerializeField] private float defaultStunTime = 1f;
        [SerializeField] private bool defaultApplyDot = false;
        [SerializeField] private int defaultDotDamage = 5;
        [SerializeField] private float defaultDotTime = 3f;

        // ─────────────── Runtime state ───────────────
        private int damage;
        private CharacterTypeData damageType;
        private bool applyStun;
        private float stunDuration;
        private bool applyDot;
        private int dotTotalDamage;
        private float dotDuration;
        private float lifeTime;
        private int maxActivations;

        private MonsterEntity owner;      // who spawned the trap
        private int activations;

        #region Setup / Lifetime ------------------------------------------------

        /// <summary>
        /// Injects all values coming from the <see cref="MonsterSkill"/> that
        /// created this trap. Call immediately after <c>Instantiate()</c>.
        /// </summary>
        public void ConfigureFromSkill(MonsterSkill sk, MonsterEntity spawner)
        {
            owner = spawner;
            damage = Mathf.RoundToInt(sk.baseDamage);
            damageType = sk.damageType;

            applyStun = sk.stun.enable;
            stunDuration = sk.stun.duration;

            applyDot = sk.dot.enable;
            dotTotalDamage = sk.dot.totalDamage;
            dotDuration = sk.dot.duration;

            lifeTime = sk.trapLifetime > 0 ? sk.trapLifetime : defaultLifeTime;
            maxActivations = 0; // infinite by design; change if needed
        }

        private void OnEnable()
        {
            // Fallback to serialized defaults if ConfigureFromSkill was skipped
            if (damage == 0 && defaultDamage > 0) damage = defaultDamage;
            if (damageType == null) damageType = defaultDamageType;
            if (lifeTime <= 0f) lifeTime = defaultLifeTime;
            if (maxActivations == 0) maxActivations = defaultMaxUses;
            if (!applyStun && defaultApplyStun) { applyStun = true; stunDuration = defaultStunTime; }
            if (!applyDot && defaultApplyDot) { applyDot = true; dotTotalDamage = defaultDotDamage; dotDuration = defaultDotTime; }

            StartCoroutine(LifetimeRoutine());
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }

        #endregion

        #region Collision / Effects --------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Character")) return;

            CharacterEntity target = other.GetComponent<CharacterEntity>();
            if (target == null) return;

            ApplyDamageAndEffects(target);

            activations++;
            if (maxActivations > 0 && activations >= maxActivations)
                Destroy(gameObject);
        }

        private void ApplyDamageAndEffects(CharacterEntity target)
        {
            float dmg = damage;

            if (damageType != null)
                dmg = Mathf.RoundToInt(
                        GameInstance.Singleton.TotalDamageWithElements(
                            damageType, target.GetCharacterType(), dmg));

            dmg = Mathf.Max(
                    dmg - target.GetCurrentDefense(),
                    GameplayManager.Singleton.minDamage);

            target.ReceiveDamage(dmg);

            if (applyStun && stunDuration > 0f)
                target.ReceiveStun(stunDuration);

            if (applyDot && dotTotalDamage > 0 && dotDuration > 0f)
                target.ReceiveDot(null, dotTotalDamage, dotDuration);

            owner?.ApplyHpLeech(dmg);
        }


        #endregion
    }
}
