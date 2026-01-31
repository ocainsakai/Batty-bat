using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>Basic clip data with speed & transition.</summary>
    [System.Serializable]
    public struct SimpleAnim
    {
        public AnimationClip clip;
        [Min(0.1f)] public float speed;
        [Range(0f, 1f)] public float transition; // 0.25 = 25 % cross-fade
    }   
}