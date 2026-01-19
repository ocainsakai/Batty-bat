using UnityEngine;
using Core.EventSystem;

namespace Games.Worm.Events
{
    /// <summary>
    /// Player/Worm-specific events
    /// </summary>

    public class WormGrowthChangedEvent : IEvent
    {
        public float OldRadius { get; set; }
        public float NewRadius { get; set; }
        public int GrowthAmount { get; set; }

        public WormGrowthChangedEvent(float oldRadius, float newRadius, int growthAmount)
        {
            OldRadius = oldRadius;
            NewRadius = newRadius;
            GrowthAmount = growthAmount;
        }
    }

    public class WormMaxSizeReachedEvent : IEvent
    {
        public float MaxRadius { get; set; }

        public WormMaxSizeReachedEvent(float maxRadius)
        {
            MaxRadius = maxRadius;
        }
    }
}
