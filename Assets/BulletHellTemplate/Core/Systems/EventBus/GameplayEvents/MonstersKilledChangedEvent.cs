namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the total number of killed monsters changes.
    /// </summary>
    public partial struct MonstersKilledChangedEvent
    {
        public int TotalKilled;

        /// <summary>
        /// Creates a new instance of the MonstersKilledChangedEvent.
        /// </summary>
        /// <param name="totalKilled">The new total number of monsters killed.</param>
        public MonstersKilledChangedEvent(int totalKilled)
        {
            TotalKilled = totalKilled;
        }
    }
}
