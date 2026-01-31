using System.Collections.Generic;
using System.Threading;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using UnityEngine;

#if FUSION2
using Fusion;
#endif

namespace BulletHellTemplate
{
    /// <summary>
    ///     Generic damage projectile: movement, ricochet, boomerang,
    ///     per-target cooldown and optional size-change over time.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public sealed class DamageEntity : MonoBehaviour
    {
        /* ───── Runtime ───── */
        private Rigidbody _rb;
        private CharacterEntity _attacker;
        private IDamageProvider _cfg;

        private float _life;
        private Vector3 _spawnPos;
        private bool _returning;

        private bool _isReplica;
        private bool _inited = false;
        /* ───── Target cooldown ───── */
        private readonly HashSet<Collider> _valid = new(16);
        private readonly Dictionary<Collider, float> _cool = new(32);
        private static readonly List<Collider> _tmpKeys = new(32);

        /* ───── Size-change ───── */
        private CancellationTokenSource _scaleCts;

        /* ───── Non-alloc helpers ───── */
        private static readonly RaycastHit[] _ray = new RaycastHit[1];

#if FUSION2
        private NetworkObject _nob;
#endif
        static ulong _seq;
        public ulong HitId { get; private set; }
        private static long _hitSeq = 0;
        private static ulong NextHitId() =>
            (ulong)System.Threading.Interlocked.Increment(ref _hitSeq);

        #region Unity ---------------------------------------------------------

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            GetComponent<Collider>().isTrigger = true;
#if FUSION2
            _nob = GetComponent<NetworkObject>();
#endif
        }

        private void OnEnable()
        {
            _valid.Clear();
            _cool.Clear();
            _returning = false;
#if FUSION2
            if (_nob && _nob.Runner && _nob.Runner.IsRunning && !_nob.HasStateAuthority)
            {
                _isReplica = true;
            }
#endif
        }
        private void Update()
        {
            if (GameplayManager.Singleton.IsPaused()) return;
            if (!_inited) return;

            if (_isReplica)
            {
                transform.position += _rb.linearVelocity * Time.deltaTime;
                TickLifetimeLocal(Time.deltaTime);
                return;
            }

            SimulateBullet(Time.deltaTime);
        }

        private void SimulateBullet(float dt)
        {
            TickCooldowns();
            TickBoomerang();
            TickLifetimeLocal(dt); 
        }
        #endregion

        #region Public API ----------------------------------------------------

        /// <summary>
        /// Initialises the projectile just before being spawned.
        /// </summary>
        public void Init(IDamageProvider cfg, CharacterEntity attacker, Vector3 velocity, bool isReplica)
        {
#if FUSION2
            ulong a = attacker ? (ulong)attacker.Id.Object.Raw : 0UL;
#else
            ulong a = 0UL;
#endif
            HitId = (a << 32) | ++_seq;

            _cfg = cfg;
            _attacker = attacker;
            _rb.linearVelocity = cfg.IsOrbital ? Vector3.zero : velocity;
            _life = cfg.LifeTime;
            _spawnPos = transform.position;
            _isReplica = isReplica;
            _inited = true;

            


            if (_cfg == null) Debug.LogError("DamageEntity.Init: cfg is null!");
            if (_attacker == null && !isReplica)
                Debug.LogError("DamageEntity.Init: attacker is null on non‐replica!");

        }

        /// <summary>
        /// Starts a size-change routine based on supplied config.
        /// </summary>
        public void SetSizeChange(DamageEntitySizeChange cfg)
        {
            _scaleCts?.Cancel();
            if (cfg == null || !cfg.enableSizeChange) return;

            _scaleCts = new CancellationTokenSource();
             ChangeSizeAsync(cfg, _scaleCts.Token).Forget();
        }

        /// <summary>
        /// Backwards-compat alias kept for older calls.
        /// </summary>
        public void StartSizeChange(DamageEntitySizeChange cfg) => SetSizeChange(cfg);

        #endregion

        #region Tick helpers --------------------------------------------------

        private void TickCooldowns()
        {
            if (_cool.Count == 0) return;

            _tmpKeys.Clear();
            _tmpKeys.AddRange(_cool.Keys);

            foreach (var k in _tmpKeys)
            {
                float t = _cool[k] - Time.deltaTime;
                if (t <= 0) _cool.Remove(k);
                else _cool[k] = t;
            }
        }

        private void TickLifetimeLocal(float dt)
        {
            if ((_life -= dt) > 0) return;
            if (_cfg.ExplodeOnDie) SpawnExplosion(_cfg.ExplodePrefab.gameObject, _cfg.ExplodePrefabSettings);
            ReturnToPool();
        }

        private void TickBoomerang()
        {
            if (!_cfg.IsBoomerang) return;

            if (!_returning)
            {
                if (Vector3.Distance(_spawnPos, transform.position) >= _cfg.MaxDistance)
                    _returning = true;
            }
            else
            {
                Vector3 dir = (_attacker.transform.position - transform.position).normalized;
                _rb.linearVelocity = dir * _rb.linearVelocity.magnitude;
                transform.forward = dir;

                if (Vector3.Distance(transform.position, _attacker.transform.position) <= 0.3f)
                    ReturnToPool();
            }
        }

        #endregion

        #region Collision -----------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (_isReplica || !_inited) return;
            if (_attacker && other.gameObject == _attacker.gameObject) return;

            if (other.TryGetComponent<MonsterEntity>(out _))
            {
                _valid.Add(other);
                return;
            }
            if (GameInstance.Singleton && GameplayManager.Singleton.IsPvp &&
                other.TryGetComponent<CharacterEntity>(out var otherPlayer))
            {
                if (_attacker != null && otherPlayer != _attacker &&
                    otherPlayer.IsDead == false &&
                    otherPlayerTeamDifferent(otherPlayer, _attacker))
                {
                    _valid.Add(other);
                    return;
                }
            }
            if (other.CompareTag("Wall") && _cfg.IsRicochet)
                HandleRicochet();
        }

        private void OnTriggerStay(Collider other)
        {
            if (_isReplica || !_inited) return;
            if (!_valid.Contains(other) || _cool.ContainsKey(other)) return;

            //Monster
            if (other.TryGetComponent<MonsterEntity>(out var monster))
                ApplyDamageMonster(monster);
            // PLAYER (PVP)
            else if (GameInstance.Singleton && GameplayManager.Singleton.IsPvp &&
                     other.TryGetComponent<CharacterEntity>(out var player))
                ApplyDamagePlayerPvp(player);

            _cool[other] = 0.3f;

            if (_cfg.DestroyOnHit)
            {
                if (_cfg.ExplodeOnDie) SpawnExplosion(_cfg.ExplodePrefab.gameObject, _cfg.ExplodePrefabSettings);
                ReturnToPool();
            }
        }

        private static bool otherPlayerTeamDifferent(CharacterEntity a, CharacterEntity b)
        {
#if FUSION2
            return a.TeamId != b.TeamId;
#else
            return a.Team != b.Team;
#endif
        }

        private void HandleRicochet()
        {
            var v = _rb.linearVelocity;
            if (Physics.RaycastNonAlloc(transform.position, v.normalized, _ray, 1f) == 0) return;

            Vector3 refl = Vector3.Reflect(v, _ray[0].normal);
            _rb.linearVelocity = refl;
            transform.forward = refl;
        }

        #endregion

        #region Damage --------------------------------------------------------

        /// <summary>
        /// Applies damage (with crit, extra effects) and spawns a VFX hit-spark.
        /// </summary>
        private void ApplyDamageMonster(MonsterEntity target)
        {
            if (!target) return;

            if (_cfg is SkillDamageProvider sdp)
            {
                var anchor = target.effectTransform ? target.effectTransform : target.transform;
                if (sdp.HitEffect)
                {
                    var vfx = GameEffectsManager.SpawnEffect(sdp.HitEffect, anchor.position, Quaternion.identity);
                    var auto = vfx.GetComponent<ReturnEffectToPool>() ?? vfx.AddComponent<ReturnEffectToPool>();
                }
                if (sdp.HitAudio)
                    AudioManager.Singleton.PlayAudio(sdp.HitAudio, "vfx", anchor.position);
            }

            _cfg.ApplyExtraEffects(_attacker ? _attacker.transform.position : transform.position, target);
            bool crit = _cfg.CanCrit && _attacker && Random.value < _attacker.GetCurrentCriticalRate();
            float dmg = _cfg.BaseDamage + (_attacker ? _attacker.GetCurrentDamage() * _cfg.AttackerDamageRate : 0f);

            if (_cfg is SkillDamageProvider sdpe)
                dmg = GameInstance.Singleton.TotalDamageWithElements(sdpe.ElementalType, target.GetCharacterTypeData, dmg);

            if (crit && _attacker) dmg *= _attacker.GetCurrentCriticalDamageMultiplier();

            var mh = target.GetComponent<MonsterHealth>();
            if (mh)
            {
#if FUSION2
                var attackerNob = _attacker ? _attacker.GetComponent<NetworkObject>() : null;
                if (attackerNob)
                    mh.ApplyDamageFromHit(attackerNob, NextHitId(), dmg, crit);
                else
                    mh.ApplyDamageRequest(dmg, crit); 
#else
                mh.ReceiveDamage(dmg, crit);
#endif
            }
        }

        private void ApplyDamagePlayerPvp(CharacterEntity target)
        {
            if (!target || target == _attacker) return;
            if (!GameInstance.Singleton || !GameplayManager.Singleton.IsPvp) return;
            if (!otherPlayerTeamDifferent(target, _attacker)) return;

            bool crit = _cfg.CanCrit && _attacker && Random.value < _attacker.GetCurrentCriticalRate();
            float dmg = _cfg.BaseDamage + (_attacker ? _attacker.GetCurrentDamage() * _cfg.AttackerDamageRate : 0f);
            if (crit && _attacker) dmg *= _attacker.GetCurrentCriticalDamageMultiplier();

#if FUSION2
            var attackerNob = _attacker ? _attacker.GetComponent<NetworkObject>() : null;
            if (attackerNob)
                target.ApplyDamageToSelfFromHit(attackerNob, NextHitId(), dmg, crit);
            else
                target.ReceiveDamage(dmg); // offline fallback
#else
            target.ReceiveDamage(dmg);
#endif
            // if (target.IsDead) GameInstance.Singleton.AddTeamScore(_attacker.TeamId, 1);
        }


        #endregion

        #region Size-change (UniTask) -----------------------------------------

        private async UniTaskVoid ChangeSizeAsync(
            DamageEntitySizeChange cfg,
            CancellationToken token)
        {
            Vector3 start = new(cfg.initialSizeX, cfg.initialSizeY, cfg.initialSizeZ);
            Vector3 end = new(cfg.finalSizeX, cfg.finalSizeY, cfg.finalSizeZ);
            Vector3 dur = new(cfg.sizeChangeTimeX, cfg.sizeChangeTimeY, cfg.sizeChangeTimeZ);

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

                float lx = dur.x > 0 ? t.x / dur.x : 1;
                float ly = dur.y > 0 ? t.y / dur.y : 1;
                float lz = dur.z > 0 ? t.z / dur.z : 1;

                transform.localScale = new Vector3(
                    Mathf.Lerp(start.x, end.x, lx),
                    Mathf.Lerp(start.y, end.y, ly),
                    Mathf.Lerp(start.z, end.z, lz));

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        #endregion

        #region Pool helpers --------------------------------------------------

        private void SpawnExplosion(GameObject explosionPrefab, SkillLevel cfg)
        {
            if (explosionPrefab == null) return;

            var exp = GameEffectsManager.SpawnEffect(explosionPrefab, transform.position, Quaternion.identity);

            if (exp.TryGetComponent<DamageEntity>(out var de))
            {
                var baseProv = _cfg;
                var safeProv = new NoChainProvider(baseProv, cfg);
                de.Init(safeProv, _attacker, Vector3.zero, _isReplica);
            }

            var auto = exp.GetComponent<ReturnEffectToPool>() ?? exp.AddComponent<ReturnEffectToPool>();
            auto.Delay = cfg.lifeTime;
        }

        private void ReturnToPool()
        {
            _scaleCts?.Cancel();
            GameEffectsManager.ReleaseEffect(gameObject);     
        }


        private readonly struct NoChainProvider : IDamageProvider
        {
            private readonly IDamageProvider _base;
            private readonly float _life;

            public NoChainProvider(IDamageProvider @base, SkillLevel lvl)
            {
                _base = @base;
                _life = lvl.lifeTime;
            }
            public float BaseDamage => _base.BaseDamage;
            public float AttackerDamageRate => _base.AttackerDamageRate;
            public float LifeTime => _life;                    
            public bool CanCrit => _base.CanCrit;
            public CharacterTypeData ElementalType => _base.ElementalType;
            public bool IsOrbital => _base.IsOrbital;
            public bool IsBoomerang => _base.IsBoomerang;
            public float MaxDistance => _base.MaxDistance;
            public bool IsRicochet => _base.IsRicochet;

            public bool DestroyOnHit => _base.DestroyOnHit;
            public bool ExplodeOnDie => false;                   
            public DamageEntity ExplodePrefab => null;
            public SkillLevel ExplodePrefabSettings => null;

            public GameObject HitEffect => _base.HitEffect;
            public AudioClip HitAudio => _base.HitAudio;

            public void ApplyExtraEffects(Vector3 origin, MonsterEntity m)
                => _base.ApplyExtraEffects(origin, m);
        }
        #endregion
    }
}
