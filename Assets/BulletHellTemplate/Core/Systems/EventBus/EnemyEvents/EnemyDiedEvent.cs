namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when an enemy dies.
    /// </summary>
    public struct EnemyDiedEvent
    {
        public string enemyId;

        public EnemyDiedEvent(string enemyId)
        {
            this.enemyId = enemyId;
        }
    }
}
