namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a player character receives damage.
    /// Contains information about the amount of damage and the source.
    /// </summary>
    public partial struct AnimationOnReceiveDamageEvent
    {
        /// <summary>
        /// The character who received damage.
        /// </summary>
        public CharacterModel Target;

        public AnimationOnReceiveDamageEvent(CharacterModel target)
        {
            Target = target;
        }
    }
}
