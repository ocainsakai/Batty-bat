namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered when the player exp changes.
    /// </summary>
    public partial struct PlayerEXPChangeEvent
    {
        public CharacterEntity Target;
        public int NewLevel;
        public int CurrentXP;
        public int NextLevelXP;

        public PlayerEXPChangeEvent(CharacterEntity _character, int newLevel, int currentXP, int nextlevelXP)
        {
            Target = _character;
            NewLevel = newLevel;
            CurrentXP = currentXP;
            NextLevelXP = nextlevelXP;
        }
    }
}
