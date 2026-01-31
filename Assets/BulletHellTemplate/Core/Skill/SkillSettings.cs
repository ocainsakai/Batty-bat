using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Enumeration for range indicator types.
    /// </summary>
    public enum RangeIndicatorType
    {
        None,
        Radial,
        RadialAoE,
        Arrow,
        Cone
    }

    /// <summary>
    /// Enumeration for launch types.
    /// </summary>
    public enum LaunchType
    {
        Projectile,
        TargetedAoE,
        TargetedAirStrike,
    }

    /// <summary>
    /// Defines the modes for Advanced Dash, such as input direction or targeting.
    /// </summary>
    public enum DashMode
    {
        InputDirection,
        ForwardOnly,
        ReverseOnly,
        NearestTarget,
        RandomDirection
    }

    /// <summary>
    /// Enumeration for aim types.
    /// </summary>
    public enum AimType
    {
        InputDirection,
        TargetNearestEnemy,
        FowardDirection,
        ReverseDirection,
        RandomDirection,
    }

    /// <summary>
    /// Represents settings for a radial indicator.
    /// </summary>
    [System.Serializable]
    public class RadialIndicatorSettings
    {
        [Tooltip("Radius area of the indicator.")]
        public float radiusArea;

        [Tooltip("Determines whether to use a cast indicator.")]
        public bool useCastIndicator;
    }

    /// <summary>
    /// Represents settings for a radial AoE indicator.
    /// </summary>
    [System.Serializable]
    public class RadialAoEIndicatorSettings
    {
        [Tooltip("Maximum range for the indicator radius.")]
        public float radiusMaxRange;

        [Tooltip("Area radius for the indicator.")]
        public float radiusArea;

        [Tooltip("Determines whether to use a cast indicator.")]
        public bool useCastIndicator;
    }

    /// <summary>
    /// Represents settings for an arrow indicator.
    /// </summary>
    [System.Serializable]
    public class ArrowIndicatorSettings
    {
        [Tooltip("Size of the arrow.")]
        public Vector3 arrowSize;

        [Tooltip("Determines whether to use a cast indicator.")]
        public bool useCastIndicator;
    }

    /// <summary>
    /// Represents settings for a cone indicator.
    /// </summary>
    [System.Serializable]
    public class ConeIndicatorSettings
    {
        [Tooltip("Size of the cone arrow.")]
        public Vector3 ConeSize;

        [Tooltip("Determines whether to use a cast indicator.")]
        public bool useCastIndicator;
    }

    /// <summary>
    /// Represents advanced dash configuration with waves and different dash modes.
    /// </summary>
    [System.Serializable]
    public class AdvancedDashSettings
    {
        [Tooltip("Enable Advanced Dash feature.")]
        public bool enableAdvancedDash = false;

        [Tooltip("Activates the skill animation trigger on each dash.")]
        public bool AnimationTriggerEachDash = false;

        [Tooltip("Number of dash waves to perform.")]
        public int dashWaves = 1;

        [Tooltip("Delay (in seconds) between each dash wave.")]
        public float delayBetweenWaves = 0f;

        [Tooltip("Dash mode for advanced dash.")]
        public DashMode dashMode = DashMode.InputDirection;

        [Tooltip("Speed of the dash.")]
        public float dashSpeed = 10f;

        [Tooltip("Duration of the dash.")]
        public float dashDuration = 0.3f;
    }

    /// <summary>
    /// Represents a configuration to change the size of DamageEntity prefabs over time for each axis.
    /// </summary>
    [System.Serializable]
    public class DamageEntitySizeChange
    {
        [Tooltip("Enable or disable the size change feature for DamageEntity.")]
        public bool enableSizeChange = false;

        [Tooltip("Initial size of the DamageEntity on the X-axis.")]
        public float initialSizeX = 1.0f;

        [Tooltip("Final size of the DamageEntity on the X-axis.")]
        public float finalSizeX = 1.0f;

        [Tooltip("Time (in seconds) it takes to transition the size on the X-axis.")]
        public float sizeChangeTimeX = 0;

        [Tooltip("Initial size of the DamageEntity on the Y-axis.")]
        public float initialSizeY = 1.0f;

        [Tooltip("Final size of the DamageEntity on the Y-axis.")]
        public float finalSizeY = 1.0f;

        [Tooltip("Time (in seconds) it takes to transition the size on the Y-axis.")]
        public float sizeChangeTimeY = 0;

        [Tooltip("Initial size of the DamageEntity on the Z-axis.")]
        public float initialSizeZ = 1.0f;

        [Tooltip("Final size of the DamageEntity on the Z-axis.")]
        public float finalSizeZ = 1.0f;

        [Tooltip("Time (in seconds) it takes to transition the size on the Z-axis.")]
        public float sizeChangeTimeZ = 0;
    }

    /// <summary>
    /// Represents the data for a skill level.
    /// </summary>
    [System.Serializable]
    public class SkillLevel
    {
        [Tooltip("Base damage of the skill at this level.")]
        public float baseDamage;

        [Tooltip("Damage rate based on the attacker's characterStatsComponent.")]
        public float attackerDamageRate;

        [Tooltip("Can this skill cause critical damage?")]
        public bool canCauseCriticalDamage;

        [Tooltip("Speed of the skill's projectile.")]
        public float speed;

        [Tooltip("Lifetime of the skill's projectile.")]
        public float lifeTime;

        [Tooltip("Is this level the evolved form of the skill?")]
        public bool isEvolved;
    }

    /// <summary>
    /// Represents a single wave of shots in an advanced multi-shot skill.
    /// </summary>
    [System.Serializable]
    public class ShotWave
    {
        [Tooltip("Number of shots in this wave.")]
        public int shotCount = 1;

        [Tooltip("List of angles for this wave. If more than one, each shot uses the corresponding angle.")]
        public List<float> shotAngles = new List<float>();

        [Tooltip("Use the initial angle for the first shot instead of the default behavior.")]
        public bool useInitialAngle = false;

        [Tooltip("Initial angle to use if useInitialAngle is true.")]
        public float initialAngle = 0f;

        [Tooltip("Delay (s) between EACH shot inside this wave.")]
        public float delayBetweenShots = 0f;  

        [Tooltip("Delay (s) before the NEXT wave starts.")]
        public float delayBeforeNextWave = 0f;
    }
}