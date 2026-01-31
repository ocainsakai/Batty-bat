namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when the player dies.
    /// </summary>
    public partial struct PlayerDiedEvent
    {
        public CharacterEntity Target;

        public PlayerDiedEvent(CharacterEntity character)
        {
            Target = character;
        }
    }
}
