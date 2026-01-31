namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when the character starts using a skill or performing an attack.
    /// Used to play action animations.
    /// </summary>
    public partial struct AnimationOnActionEvent
    {
        /// <summary>
        /// The character who triggered the event.
        /// </summary>
        public CharacterModel Target;

        /// <summary>
        /// Whether the action is an attack (true) or a skill (false).
        /// </summary>
        public bool isAttack;

        /// <summary>
        /// The skill index (ignored if isAttack is true).
        /// </summary>
        public int skillIndex;

        public AnimationOnActionEvent(CharacterModel sender, bool _isAttack, int _skillIndex = 0)
        {
            Target = sender;
            isAttack = _isAttack;
            skillIndex = _skillIndex;
        }
    }
}
