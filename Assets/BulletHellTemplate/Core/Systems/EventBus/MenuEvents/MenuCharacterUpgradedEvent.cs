namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when character receives an upgrade.
    /// </summary>
    public partial struct MenuCharacterUpgradedEvent
    {
        public CharacterModel Target;
        public MenuCharacterUpgradedEvent(CharacterModel characterModel)
        {
            Target = characterModel;
        }
    }
}
