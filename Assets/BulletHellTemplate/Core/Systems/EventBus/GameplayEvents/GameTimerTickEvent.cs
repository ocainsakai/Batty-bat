namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Event triggered once per second by the GameplayManager to notify the current elapsed game time.
    /// </summary>
    public partial struct GameTimerTickEvent
    {
        public int SecondsElapsed;

        /// <summary>
        /// Creates a new instance of the GameTimerTickEvent.
        /// </summary>
        /// <param name="secondsElapsed">The total seconds elapsed since the game started.</param>
        public GameTimerTickEvent(int secondsElapsed)
        {
            SecondsElapsed = secondsElapsed;
        }
    }
}
