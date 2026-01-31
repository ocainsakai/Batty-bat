namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a buff is removed from a character.
    /// </summary>
    public partial struct DebuffRemovedEvent
    {
        public CharacterEntity Target;
        public StatType statType;
        public int RemainingDebuffsCount;

        public DebuffRemovedEvent(CharacterEntity _target, StatType _buffType, int _remainingDebuffsCount)
        {
            Target = _target;
            statType = _buffType;
            RemainingDebuffsCount = _remainingDebuffsCount;
        }
    }

}
