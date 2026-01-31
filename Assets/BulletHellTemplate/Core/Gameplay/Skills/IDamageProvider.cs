using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    ///     Supplies runtime parameters for a <see cref="DamageEntity"/>.
    /// </summary>
    public interface IDamageProvider
    {
        /* ───── Core numbers ───── */
        float BaseDamage { get; }
        float AttackerDamageRate { get; }
        float LifeTime { get; }
        bool CanCrit { get; }
        CharacterTypeData ElementalType { get; }

        /* ───── Movement & behaviour ───── */
        bool IsOrbital { get; }
        bool IsBoomerang { get; }
        float MaxDistance { get; }
        bool IsRicochet { get; }

        /* ───── Destroy behaviour ───── */
        bool DestroyOnHit { get; }
        bool ExplodeOnDie { get; }
        DamageEntity ExplodePrefab { get; }
        SkillLevel ExplodePrefabSettings { get; }
        GameObject HitEffect { get; }
        public AudioClip HitAudio { get; }
        /* ───── Extra effects (DOT, slow, etc.) ───── */
        void ApplyExtraEffects(Vector3 origin ,MonsterEntity target);
    }
}
