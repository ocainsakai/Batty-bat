using System;
using UnityEngine;

namespace BulletHellTemplate.Core.FSM
{
    /// <summary>Additional data required to play a skill animation.</summary>
    [Serializable]
    public struct SkillAnimData
    {
        public AnimationClip Clip;
        public bool UseAvatarMask;
        public AvatarMask Mask;
        [Range(0.1f, 3f)] public float Speed;
        [Range(0f, 1f)] public float Transition;
    }
}
