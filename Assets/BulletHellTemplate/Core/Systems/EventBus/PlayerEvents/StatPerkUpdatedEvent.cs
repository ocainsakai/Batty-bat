namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when any stat perk is updated or added.
    /// </summary>
    public partial struct StatPerkUpdatedEvent
    {
        public CharacterEntity Target;
        public StatType statType;
        public float NewValue;

        public StatPerkUpdatedEvent(CharacterEntity character, StatType _statType, float newValue)
        {
            Target = character;
            statType = _statType;
            NewValue = newValue;
        }
    }
}
