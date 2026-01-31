using System;
using UnityEngine;

namespace BulletHellTemplate.Core.FSM
{
    /// <summary>Generic clip configuration used by most states.</summary>
    [Serializable]
    public struct AnimClipData
    {
        public AnimationClip Clip;
        [Range(0.1f, 3f)] public float Speed;
        [Range(0f, 1f)] public float Transition;
    }
}
