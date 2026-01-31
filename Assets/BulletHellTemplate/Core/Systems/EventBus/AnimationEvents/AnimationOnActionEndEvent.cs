namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when a skill or attack animation ends.
    /// Used to return the character to idle or movement animation.
    /// </summary>
    public partial struct AnimationOnActionEndEvent
    {
        /// <summary>
        /// The character who triggered the event.
        /// </summary>
        public CharacterModel Target;

        public AnimationOnActionEndEvent(CharacterModel sender)
        {
            Target = sender;
        }
    }
}
