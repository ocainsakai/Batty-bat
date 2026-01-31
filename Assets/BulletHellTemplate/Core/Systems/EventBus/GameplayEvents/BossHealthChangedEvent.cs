namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the Boss HP is updated.
    /// </summary>
    public partial struct BossHealthChangedEvent
    {
        public MonsterEntity Boss;
        public float CurrentHP;
        public float MaxHP;

        public BossHealthChangedEvent(MonsterEntity character, float currentHP, float maxHP)
        {
            Boss = character;
            CurrentHP = currentHP;
            MaxHP = maxHP;
        }
    }
}
