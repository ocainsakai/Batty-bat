using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    [System.Serializable]
    public class MonsterShotWave
    {
        [Tooltip("How many projectiles in this wave.")] 
        public int shotCount = 3;
        [Tooltip("Custom angle list (optional).")]
        public List<float> shotAngles = new();
        [Tooltip("Use an initial offset angle?")] 
        public bool useInitialAngle = false;
        public float initialAngle = 0;
        [Tooltip("Delay before the next wave.")] 
        public float delayBeforeNextWave = 0.1f;
    }

    // ── Effect blocks ────────────────────────────────────────────────
    [System.Serializable] public struct SlowEffect { public bool enable; public float percent; public float duration; }
    [System.Serializable] public struct StunEffect { public bool enable; public float duration; }
    [System.Serializable] public struct KnockbackEffect { public bool enable; public float distance; public float duration; }
    [System.Serializable] public struct DotEffect { public bool enable; public int totalDamage; public float duration; }
    [System.Serializable] public struct SelfHealEffect { public bool enable; public float amount; }
    [System.Serializable] public struct TrapHealEffect { public bool enable; public int healPerTick; public float tickInterval; public float lifeTime; }
}
