namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the player's Shield is updated.
    /// </summary>
    public partial struct PlayerShieldChangedEvent
    {
        public CharacterEntity Target;
        public float CurrentShield;
        public float MaxHP;

        public PlayerShieldChangedEvent(CharacterEntity character, float currentShield, float maxHP)
        {
            Target = character;
            CurrentShield = currentShield;
            MaxHP = maxHP;
        }
    }
}

