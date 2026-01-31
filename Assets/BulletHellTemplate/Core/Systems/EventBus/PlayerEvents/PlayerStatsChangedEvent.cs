namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the player's stats change.
    /// </summary>
    public partial struct PlayerStatsChangedEvent
    {
        public CharacterEntity Target;
        public CharacterStats playerStats;

        public PlayerStatsChangedEvent(CharacterEntity character, CharacterStats _stats)
        {
            Target = character;
            playerStats = _stats;
        }
    }
}
