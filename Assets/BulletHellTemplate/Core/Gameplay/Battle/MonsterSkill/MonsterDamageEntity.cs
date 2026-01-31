using System.Threading;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class MonsterDamageEntity : MonoBehaviour
    {
        /* ────────── Serialized ────────── */
        [Header("Explosion on Die")]
        [SerializeField] private MonsterDamageEntity explosionPrefab;

        /* ────────── Runtime, inject via ConfigureFromSkill ────────── */
        private MonsterSkill _skill;
        private MonsterEntity _owner;
        private CharacterTypeData _damageType;
        private float _damage;
        private float _lifeTime;

        /* special flags */
        private bool _orbital;
        private float _orbitRadius;
        private float _orbitAngle;
        private bool _ricochet;
        private bool _boomerang;
        private float _maxBoomerang;
        private bool _returning;

        /* state */
        private Rigidbody _rb;
        private float _tLife;
        private Vector3 _spawnPos;
        private bool _paused;
        private Vector3 _cachedVelocity;

        private ulong _shotId;
        private static ulong s_nextShotId;

        /* helpers */
        private static readonly RaycastHit[] _ray = new RaycastHit[1];

        #region ───── Init ─────────────────────────────────────────────

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnEnable()
        {
            _paused = false;
            _returning = false;
        }

        public void ConfigureFromSkill(MonsterSkill skill,
                                       MonsterEntity owner,
                                       Vector3 velocity)
        {
            _skill = skill;
            _owner = owner;
            _damageType = skill.damageType;
            _damage = skill.baseDamage;
            _lifeTime = Mathf.Max(.1f, skill.lifeTime);

            _orbital = skill.isOrbital;
            _orbitRadius = Mathf.Max(.1f, skill.orbitalDistance);
            _orbitAngle = 0f;

            _ricochet = skill.isRicochet;

            _boomerang = skill.isBoomerang;
            _maxBoomerang = Mathf.Max(.5f, skill.maxBoomerangDistance);
            _returning = false;

            _shotId = ++s_nextShotId;

#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = _orbital ? Vector3.zero : velocity;
#else
            _rb.velocity       = _orbital ? Vector3.zero : velocity;
#endif
            _spawnPos = transform.position;
            _tLife = _lifeTime;
        }

        #endregion

        #region ───── Update loop ──────────────────────────────────────

        private void Update()
        {
            HandlePause();
            if (_paused) return;

            TickLifetime();
            TickMovementModes();
        }

        private void HandlePause()
        {
            bool gPaused = GameplayManager.Singleton.IsPaused();
            if (gPaused && !_paused)
            {
                _paused = true;
#if UNITY_6000_0_OR_NEWER
                _cachedVelocity = _rb.linearVelocity;
#else
                _cachedVelocity = _rb.velocity;
#endif
                _rb.isKinematic = true;
            }
            else if (!gPaused && _paused)
            {
                _paused = false;
                _rb.isKinematic = false;
#if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = _cachedVelocity;
#else
                _rb.velocity       = _cachedVelocity;
#endif
            }
        }

        private void TickLifetime()
        {
            if ((_tLife -= Time.deltaTime) > 0f) return;
            ExplodeIfNeeded();
            ReturnToPool();
        }

        private void TickMovementModes()
        {
            /* Orbital */
            if (_orbital && _owner)
            {
                _orbitAngle += 180f * Time.deltaTime;
                float rad = _orbitAngle * Mathf.Deg2Rad;

                Vector3 offset = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * _orbitRadius;
                transform.position = _owner.transform.position + offset;
                transform.forward = (_owner.transform.position - transform.position).normalized;
                return;
            }

            /* Boomerang */
            if (_boomerang && !_returning)
            {
                if (Vector3.Distance(_spawnPos, transform.position) >= _maxBoomerang)
                    _returning = true;
            }

            if (_boomerang && _returning && _owner)
            {
                Vector3 dir = (_owner.transform.position - transform.position).normalized;
#if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = dir * _rb.linearVelocity.magnitude;
#else
                _rb.velocity       = dir * _rb.velocity.magnitude;
#endif
                transform.forward = dir;

                if (Vector3.Distance(transform.position, _owner.transform.position) <= 0.25f)
                    ReturnToPool();
            }
        }

        #endregion

        #region ───── Collision ────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Wall") && _ricochet)
            {
                Ricochet();
                return;
            }

            if (!other.CompareTag("Character")) return;

            if (other.TryGetComponent<CharacterEntity>(out var character))
            {
                bool isAuthority = true;

#if FUSION2
                if (_owner && _owner.TryGetComponent<NetworkObject>(out var ownerNob) && _owner.Runner && _owner.Runner.IsRunning)
                    isAuthority = ownerNob.HasStateAuthority;
#endif
                if (isAuthority)
                    ApplyDamage(character); 

                ExplodeIfNeeded();
                ReturnToPool();
            }
        }


        private void Ricochet()
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 v = _rb.linearVelocity;
#else
            Vector3 v = _rb.velocity;
#endif
            if (Physics.RaycastNonAlloc(transform.position, v.normalized, _ray, 1f) == 0) return;

            Vector3 refl = Vector3.Reflect(v, _ray[0].normal);
#if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = refl;
#else
            _rb.velocity       = refl;
#endif
            transform.forward = refl;
        }

        #endregion

        #region ───── Damage & effects ─────────────────────────────────

        private void ApplyDamage(CharacterEntity target)
        {
            if (!target) return;

            float dmg = GameInstance.Singleton.TotalDamageWithElements(
                            _damageType,
                            target.GetCharacterType(),
                            _damage);

            dmg = Mathf.Max(
                    dmg - target.GetCurrentDefense(),
                    GameplayManager.Singleton.minDamage);

#if FUSION2
            NetworkObject attackerNob = null;
            if (_owner) _owner.TryGetComponent(out attackerNob);
            target.ApplyDamageToSelfFromHit(attackerNob, _shotId, dmg, false);
#else
            target.ApplyDamageToSelfFromHit(null, _shotId, dmg, false);
#endif

            if (_skill.slow.enable) target.ReceiveMoveSpeedDebuff(_skill.slow.percent, _skill.slow.duration);
            if (_skill.stun.enable) target.ReceiveStun(_skill.stun.duration);
            if (_skill.knockback.enable) target.ReceiveKnockback(_owner.transform.position, _skill.knockback.distance, _skill.knockback.duration);
            if (_skill.dot.enable) target.ReceiveDot(null, _skill.dot.totalDamage, _skill.dot.duration);

            _owner.ApplyHpLeech(dmg);
        }



        #endregion

        #region ───── Size-change opcional ─────────────────────────────

        private CancellationTokenSource _scaleCts;

        public void SetSizeChange(DamageEntitySizeChange cfg)
        {
            _scaleCts?.Cancel();
            if (cfg == null || !cfg.enableSizeChange) return;

            _scaleCts = new();
            ChangeSizeAsync(cfg, _scaleCts.Token).Forget();
        }

        /// <summary>Sets the initial orbital phase in degrees (0-360).</summary>
        public void SetOrbitInitialAngle(float angleDeg)
        {
            _orbitAngle = angleDeg; 
        }

        private async UniTaskVoid ChangeSizeAsync(DamageEntitySizeChange cfg,
                                                  CancellationToken token)
        {
            Vector3 start = new Vector3(cfg.initialSizeX, cfg.initialSizeY, cfg.initialSizeZ);
            Vector3 end = new Vector3(cfg.finalSizeX, cfg.finalSizeY, cfg.finalSizeZ);
            Vector3 dur = new Vector3(cfg.sizeChangeTimeX, cfg.sizeChangeTimeY, cfg.sizeChangeTimeZ);
            Vector3 t = Vector3.zero;

            transform.localScale = start;

            while (t.x < dur.x || t.y < dur.y || t.z < dur.z)
            {
                if (GameplayManager.Singleton.IsPaused())
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    continue;
                }

                float dt = Time.deltaTime;
                if (t.x < dur.x) t.x = Mathf.Min(t.x + dt, dur.x);
                if (t.y < dur.y) t.y = Mathf.Min(t.y + dt, dur.y);
                if (t.z < dur.z) t.z = Mathf.Min(t.z + dt, dur.z);

                float lx = dur.x > 0 ? t.x / dur.x : 1f;
                float ly = dur.y > 0 ? t.y / dur.y : 1f;
                float lz = dur.z > 0 ? t.z / dur.z : 1f;

                transform.localScale = new Vector3(
                    Mathf.Lerp(start.x, end.x, lx),
                    Mathf.Lerp(start.y, end.y, ly),
                    Mathf.Lerp(start.z, end.z, lz));

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        #endregion

        #region ───── Pool helpers ─────────────────────────────────────

        private void ExplodeIfNeeded()
        {
            if (!explosionPrefab) return;

            var go = GameEffectsManager.SpawnEffect(
                         explosionPrefab.gameObject,
                         transform.position,
                         Quaternion.identity);

            if (go.TryGetComponent<MonsterDamageEntity>(out var de))
                de.ConfigureFromSkill(_skill, _owner, Vector3.zero);

            go.AddComponent<ReturnEffectToPool>();
        }

        private void ReturnToPool()
        {
            _scaleCts?.Cancel();
            GameEffectsManager.ReleaseEffect(gameObject);
        }

        #endregion
    }
}
