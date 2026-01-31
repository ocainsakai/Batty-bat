namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a buff is received by a character.
    /// </summary>
    public partial struct BuffReceivedEvent
    {
        public CharacterEntity character;
        public StatType statType;
        public float amount;
        public float duration;
        public int activeBuffsAmount;

        public BuffReceivedEvent(CharacterEntity _character, StatType _statType, float _amount, float _duration, int _activeBuffsAmount)
        {
            character = _character;
            statType = _statType;
            amount = _amount;
            duration = _duration;
            activeBuffsAmount = _activeBuffsAmount;
        }
    }
}
