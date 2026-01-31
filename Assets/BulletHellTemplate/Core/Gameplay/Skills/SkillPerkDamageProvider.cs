using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    ///     Adapter that exposes <see cref="SkillPerkData"/> values through
    ///     the <see cref="IDamageProvider"/> interface.
    /// </summary>
    public readonly struct SkillPerkDamageProvider : IDamageProvider
    {
        private readonly SkillPerkData _perk;
        private readonly int _level;
        private readonly bool _isReplica;

        public SkillPerkDamageProvider(SkillPerkData perk, int level, bool isReplica)
        {
            _perk = perk;
            _level = Mathf.Clamp(level, 0, perk.maxLevel);
            _isReplica = isReplica;
        }

        /* ───── Core numbers ───── */
        public float BaseDamage => _isReplica ? 0f : _perk.GetDamageForLevel(_level);
        public float AttackerDamageRate => _isReplica ? 0f : _perk.GetAttackerDamageRateForLevel(_level);
        public float LifeTime => _perk.lifeTime;
        public bool CanCrit => !_perk.withoutCooldown;
        public CharacterTypeData ElementalType => _perk.damageType;

        /* ───── Behaviour ───── */
        public bool IsOrbital => _perk.isOrbital;
        public bool IsBoomerang => _perk.isBoomerangSkill;
        public float MaxDistance => _perk.maxDistance;
        public bool IsRicochet => _perk.isRicochet;

        /* ───── Destroy behaviour ───── */
        public bool DestroyOnHit => _perk.destroyOnFirstHit;
        public bool ExplodeOnDie => _perk.explodeOnDestroy;
        public DamageEntity ExplodePrefab => null;
        public SkillLevel ExplodePrefabSettings => null;
        public GameObject HitEffect => _perk.HitEffect;
        public AudioClip HitAudio => _perk.HitAudio;
        /* ───── Extra effects applied to a target ───── */
        public void ApplyExtraEffects(Vector3 origin, MonsterEntity target)
        {
            if (target == null) return;

            if (_perk.applySlow)
                target.Slow(_perk.slowPercent, _perk.slowDuration);

            if (_perk.applyKnockback)
                target.Knockback(origin, _perk.knockbackDistance, _perk.knockbackDuration);

            if (_perk.applyStun)
                target.Stun(_perk.stunDuration);

            if (_perk.applyDOT)
            {
                float dotDmg = GameInstance.Singleton.TotalDamageWithElements(
                                   _perk.damageType,
                                   target.GetCharacterTypeData,
                                   _perk.dotAmount);

                target.ApplyDOT(_perk, (int)dotDmg, _perk.dotDuration);
            }
        }
    }
}
