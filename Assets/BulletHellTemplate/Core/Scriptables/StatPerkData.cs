using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    [CreateAssetMenu(fileName = "NewStatPeakData", menuName = "Stats/StatPeakData")]
    public class StatPerkData : ScriptableObject
    {
        [Header("General Icon for StatPeakData")]
        public Sprite icon; // Icon for the entire StatPeakData

        [Header("Icon for Max Level")]
        public Sprite maxLevelIcon; // Icon representing the stat at max level

        [Header("Stat Type")]
        public StatType statType; // Selected stat type

        [Header("Fixed Stat: Total value added to the character.")]
        public FixedStat fixedStat;

        [Header("Rate Stat: Increases the character's stat by a percentage.")]
        public RateStat rateStat;

        [System.Serializable]
        public class FixedStat
        {
            [Header("Statistics for each level from 1 to Max Level")]
            public List<float> values = new List<float> { 0, 0, 0, 0, 0 };
        }

        [System.Serializable]
        public class RateStat
        {
            [Header("Percentage increase for each level from 1 to Max Level")]
            public List<float> rates = new List<float> { 0, 0, 0, 0, 0 };
        }
    }
}
