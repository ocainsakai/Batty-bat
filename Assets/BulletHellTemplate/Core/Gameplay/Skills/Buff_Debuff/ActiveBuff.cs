using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents an active buff or debuff applied to the character.
    /// Tracks its type, value, and duration.
    /// </summary>
    public class ActiveBuff
    {
        public string Id;
        public StatType Type;
        public BuffCategory Category;
        public float Amount;
        public float Duration;

        public ActiveBuff(string id, StatType type, BuffCategory category, float amount, float duration)
        {
            Id = id;
            Type = type;
            Category = category;
            Amount = amount;
            Duration = duration;
        }
    }
}
