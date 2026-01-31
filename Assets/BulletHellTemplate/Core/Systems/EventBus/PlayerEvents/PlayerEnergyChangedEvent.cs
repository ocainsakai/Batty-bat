namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the player's MP is updated.
    /// </summary>
    public partial struct PlayerEnergyChangedEvent
    {
        public CharacterEntity Target;
        public float CurrentMP;
        public float MaxMP;

        public PlayerEnergyChangedEvent(CharacterEntity character, float currentMP, float maxMP)
        {
            Target = character;
            CurrentMP = currentMP;
            MaxMP = maxMP;
        }
    }
}

