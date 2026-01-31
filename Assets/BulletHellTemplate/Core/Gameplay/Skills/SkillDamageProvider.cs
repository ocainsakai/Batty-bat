using UnityEngine;

namespace BulletHellTemplate
{
    public readonly struct SkillDamageProvider : IDamageProvider
    {
        private readonly SkillData _skill;
        private readonly SkillLevel _lvl;
        private readonly bool _isReplica;
        
        public SkillDamageProvider(SkillData skill, SkillLevel lvl, bool isReplica)
        {
            _skill = skill;
            _lvl = lvl;
            _isReplica = isReplica;
        }

        /* ───── Core ───── */
        public float BaseDamage => _isReplica ? 0f : _lvl.baseDamage;
        public float AttackerDamageRate => _isReplica ? 0f : _lvl.attackerDamageRate;
        public float LifeTime => _lvl.lifeTime;
        public bool CanCrit => _lvl.canCauseCriticalDamage;
        public CharacterTypeData ElementalType => _skill.damageType;

        /* ───── Behaviour ───── */
        public bool IsOrbital => _skill.isOrbital;
        public bool IsBoomerang => _skill.isBoomerangSkill;
        public float MaxDistance => _skill.maxDistance;
        public bool IsRicochet => _skill.isRicochet;

        /* ───── Destroy ───── */
        public bool DestroyOnHit => _skill.destroyOnFirstHit;
        public bool ExplodeOnDie => _skill.explodeOnDestroy;
        public DamageEntity ExplodePrefab => _skill.explodeDamageEntity;
        public SkillLevel ExplodePrefabSettings => _skill.explodeEntitySettings;
        public GameObject HitEffect => _skill.HitEffect;
        public AudioClip HitAudio => _skill.HitAudio;

        /* ───── Extra effects ───── */
        public void ApplyExtraEffects(Vector3 origin, MonsterEntity m)
        {
            if (_skill.applySlow) m.Slow(_skill.slowPercent, _skill.slowDuration);
            if (_skill.applyKnockback) m.Knockback(origin,_skill.knockbackDistance, _skill.knockbackDuration);
            if (_skill.applyStun) m.Stun(_skill.stunDuration);
            if (_skill.applyDOT)
            {
                float dmg = GameInstance.Singleton.TotalDamageWithElements(
                                _skill.damageType, m.GetCharacterTypeData, _skill.dotAmount);
                m.ApplyDOT(_skill, (int)dmg, _skill.dotDuration);
            }
        }
    }
}
