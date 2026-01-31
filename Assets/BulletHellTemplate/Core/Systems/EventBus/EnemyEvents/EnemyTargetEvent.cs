namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a new target is chosen by a character.
    /// </summary>
    public partial struct EnemyTargetEvent
    {
        public MonsterEntity Target;

        public EnemyTargetEvent(MonsterEntity _monster)
        {
            Target = _monster;
        }
    }
}