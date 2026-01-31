namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the player's Upgrade Points change.
    /// </summary>
    public partial struct UpgradePointsChangedEvent
    {
        public CharacterEntity Target;
        public int Amount;

        public UpgradePointsChangedEvent(CharacterEntity character, int _upgradePoints)
        {
            Target = character;
            Amount = _upgradePoints;
        }
    }
}
