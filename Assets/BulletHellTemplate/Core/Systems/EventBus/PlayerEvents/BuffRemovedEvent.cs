namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a buff is removed from a character.
    /// </summary>
    public partial struct BuffRemovedEvent
    {
        public CharacterEntity Target;
        public StatType statType;
        public int RemainingBuffsCount;

        public BuffRemovedEvent(CharacterEntity _target, StatType _buffType, int _remainingBuffsCount)
        {
            Target = _target;
            statType = _buffType;
            RemainingBuffsCount = _remainingBuffsCount;
        }
    }

}
