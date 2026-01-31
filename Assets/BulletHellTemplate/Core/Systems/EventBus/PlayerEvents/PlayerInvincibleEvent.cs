namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a character becomes invincible temporarily.
    /// </summary>
    public partial struct PlayerInvincibleEvent
    {
        public CharacterEntity character;
        public float duration;

        public PlayerInvincibleEvent(CharacterEntity character, float duration)
        {
            this.character = character;
            this.duration = duration;
        }
    }
}
