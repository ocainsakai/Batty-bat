namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a character receives damage.
    /// </summary>
    public partial struct PlayerDamagedEvent
    {
        /// <summary>
        /// The amount of damage taken after all calculations (e.g. defense, shield).
        /// </summary>
        public float Damage;

        /// <summary>
        /// Optional identifier for the source of damage (e.g., monster ID, trap, projectile).
        /// </summary>
        public string SourceId;

        /// <summary>
        /// The target that received the damage.
        /// </summary>
        public CharacterEntity Target;

        /// <summary>
        /// Initializes a new damage event.
        /// </summary>
        /// <param name="target">The character who received the damage.</param>
        /// <param name="damage">The final damage value applied.</param>
        /// <param name="sourceId">The ID of the damage source.</param>
        public PlayerDamagedEvent(CharacterEntity target, float damage, string sourceId = "")
        {
            Target = target;
            Damage = damage;
            SourceId = sourceId;
        }
    }
}
