using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>Animation data for skills or attack.</summary>
    [System.Serializable]
    public struct SkillAnim
    {
        [Tooltip("Index used by gameplay to trigger this skill.")]
        public int skillIndex;

        public AnimationClip clip;

        [Min(0.1f)] public float speed;
        [Range(0f, 1f)] public float transition;
       
        public bool useAvatarMask;
    }
}
