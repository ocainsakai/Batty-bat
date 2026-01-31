using UnityEngine;

namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when the character starts moving with direction input.
    /// Used to update movement animations.
    /// </summary>
    public partial struct AnimationOnRunEvent
    {
        /// <summary>
        /// The character who triggered the event.
        /// </summary>
        public CharacterModel Target;

        /// <summary>
        /// The movement direction.
        /// </summary>
        public Vector2 dir;

        public AnimationOnRunEvent(CharacterModel sender, Vector2 _dir)
        {
            Target = sender;
            dir = _dir;
        }
    }
}
