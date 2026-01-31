using System;
using UnityEngine;

namespace BulletHellTemplate.Core.FSM
{
    /// <summary>
    /// Eight-way animation set. Designers only need to assign <b>Forward</b>;
    /// all others are optional. <br/>
    /// Order (reference only): N(Fwd), NE, E(Right), SE, S(Back), SW, W(Left), NW.
    /// </summary>
    [Serializable]
    public struct DirectionalAnimSet
    {
        [Header("Required")]
        public AnimClipData Forward;          // North

        [Header("Optional")]
        public AnimClipData ForwardRight;     // North-East
        public AnimClipData Right;            // East
        public AnimClipData BackRight;        // South-East
        public AnimClipData Back;             // South
        public AnimClipData BackLeft;         // South-West
        public AnimClipData Left;             // West
        public AnimClipData ForwardLeft;      // North-West

        /// <summary>
        /// Returns the appropriate clip for <paramref name="dir"/>.<br/>
        /// Falls back to <see cref="Forward"/> when the specific clip is null.
        /// </summary>
        public AnimClipData GetClip(Vector2 dir)
        {
            // Default to Forward when standing still
            if (dir.sqrMagnitude < 0.01f)
                return Forward;

            float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 360f + 22.5f) % 360f;
            int idx = (int)(angle / 45f);

            return idx switch
            {
                0 => ForwardRight.Clip ? ForwardRight : Forward,
                1 => Right.Clip ? Right : Forward,
                2 => BackRight.Clip ? BackRight : Forward,
                3 => Back.Clip ? Back : Forward,
                4 => BackLeft.Clip ? BackLeft : Forward,
                5 => Left.Clip ? Left : Forward,
                6 => ForwardLeft.Clip ? ForwardLeft : Forward,
                _ => Forward, // 7 == Forward (N)
            };
        }

        public string GetStateName(Vector2 dir, string prefix)
        {
            string suffix = "N"; // default
            if (dir.sqrMagnitude >= 0.01f)
            {
                float angle = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 360f + 22.5f) % 360f;
                suffix = ((int)(angle / 45f)) switch
                {
                    0 => "NE",
                    1 => "E",
                    2 => "SE",
                    3 => "S",
                    4 => "SW",
                    5 => "W",
                    6 => "NW",
                    _ => "N",
                };
            }
            return $"{prefix}_{suffix}";
        }
    }
}
