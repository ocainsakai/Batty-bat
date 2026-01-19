using Core.EventSystem;

namespace Games.Worm.Events
{
    /// <summary>
    /// Game-related events for Worm game
    /// </summary>

    public class GameStartedEvent : IEvent
    {
        public float GameSpeed { get; set; }

        public GameStartedEvent(float gameSpeed = 1f)
        {
            GameSpeed = gameSpeed;
        }
    }

    public class GameOverEvent : IEvent
    {
        public int FinalScore { get; set; }
        public string Reason { get; set; }

        public GameOverEvent(int finalScore, string reason = "Player died")
        {
            FinalScore = finalScore;
            Reason = reason;
        }
    }

    public class ScoreChangedEvent : IEvent
    {
        public int OldScore { get; set; }
        public int NewScore { get; set; }
        public int Delta { get; set; }

        public ScoreChangedEvent(int oldScore, int newScore)
        {
            OldScore = oldScore;
            NewScore = newScore;
            Delta = newScore - oldScore;
        }
    }
}
