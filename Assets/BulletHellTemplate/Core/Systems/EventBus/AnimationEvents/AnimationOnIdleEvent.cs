namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when the character stops moving and becomes idle.
    /// Used to play idle animations.
    /// </summary>
    public partial struct AnimationOnIdleEvent
    {
        /// <summary>
        /// The character who triggered the event.
        /// </summary>
        public CharacterModel Target;

        public AnimationOnIdleEvent(CharacterModel sender)
        {
            Target = sender;
        }
    }
}
