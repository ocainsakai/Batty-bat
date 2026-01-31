namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered whenever the total number of gold changes.
    /// </summary>
    public partial struct GoldChangedEvent
    {
        public int TotalGold;

        /// <summary>
        /// Creates a new instance of the GoldChangedEvent.
        /// </summary>
        /// <param name="totalKilled">The new total number of gold.</param>
        public GoldChangedEvent(int _totalGold)
        {
            TotalGold = _totalGold;
        }
    }
}

