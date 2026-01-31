namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event animation triggered when a player character dies.
    /// Used for handling death logic such as playing VFX or triggering game over.
    /// </summary>
    public partial struct AnimationOnDiedEvent
    {
        /// <summary>
        /// The character who died.
        /// </summary>
        public CharacterModel Target;

        public AnimationOnDiedEvent(CharacterModel target)
        {
            Target = target;
        }
    }
}
