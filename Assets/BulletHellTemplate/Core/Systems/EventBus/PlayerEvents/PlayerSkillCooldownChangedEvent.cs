using BulletHellTemplate;

namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever a skill cooldown changes (one per skill).
    /// </summary>
    public partial struct PlayerSkillCooldownChangedEvent
    {
        public CharacterEntity Target;
        public int SkillIndex;
        public float CurrentCooldown;
        public float MaxCooldown;

        /// <summary>
        /// Creates a new instance of the SkillCooldownChangedEvent.
        /// </summary>
        /// <param name="player">The player entity whose skill cooldown changed.</param>
        /// <param name="skillIndex">The index of the skill affected.</param>
        /// <param name="currentCooldown">The current remaining cooldown time.</param>
        /// <param name="maxCooldown">The maximum cooldown time for this skill.</param>
        public PlayerSkillCooldownChangedEvent(CharacterEntity player, int skillIndex, float currentCooldown, float maxCooldown)
        {
            Target = player;
            SkillIndex = skillIndex;
            CurrentCooldown = currentCooldown;
            MaxCooldown = maxCooldown;
        }
    }
}
