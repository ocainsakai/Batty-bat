namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the player's HP is updated.
    /// </summary>
    public partial struct PlayerHealthChangedEvent
    {
        public CharacterEntity Target;
        public float CurrentHP;
        public float MaxHP;

        public PlayerHealthChangedEvent(CharacterEntity character, float currentHP, float maxHP)
        {
            Target = character;
            CurrentHP = currentHP;
            MaxHP = maxHP;
        }
    }
}
