using UnityEngine;

namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a character receives Knockback.
    /// </summary>
    public partial struct PlayerKnockbackReceivedEvent
    {

        public CharacterEntity Target;

        public MonsterEntity Sender;

        public Vector3 SenderDirection;

        public float Distance;

        public float Duration;

        public PlayerKnockbackReceivedEvent(CharacterEntity target, MonsterEntity sender, Vector3 senderDirection, float distance, float duration)
        {
            Target = target;
            Sender = sender;
            SenderDirection = senderDirection;
            Distance = distance;
            Duration = duration;
        }
    }
}
