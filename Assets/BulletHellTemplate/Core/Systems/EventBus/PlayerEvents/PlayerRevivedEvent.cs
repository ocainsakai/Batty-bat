namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when the player is revived.
    /// </summary>
    public partial struct PlayerRevivedEvent
    {
        public CharacterEntity character;

        public PlayerRevivedEvent(CharacterEntity character)
        {
            this.character = character;
        }
    }
}
