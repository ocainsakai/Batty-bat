/// <summary>
/// Game-related events
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

public class GamePausedEvent : IEvent
{
    public float GameTime { get; set; }

    public GamePausedEvent(float gameTime)
    {
        GameTime = gameTime;
    }
}

public class GameResumedEvent : IEvent
{
    public float GameTime { get; set; }

    public GameResumedEvent(float gameTime)
    {
        GameTime = gameTime;
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

public class GameStateChangedEvent : IEvent
{
    public GameState OldState { get; set; }
    public GameState NewState { get; set; }

    public GameStateChangedEvent(GameState oldState, GameState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}

public class HighScoreAchievedEvent : IEvent
{
    public int OldHighScore { get; set; }
    public int NewHighScore { get; set; }

    public HighScoreAchievedEvent(int oldHighScore, int newHighScore)
    {
        OldHighScore = oldHighScore;
        NewHighScore = newHighScore;
    }
}
