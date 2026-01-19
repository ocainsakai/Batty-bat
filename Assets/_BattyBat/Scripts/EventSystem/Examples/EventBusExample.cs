using UnityEngine;

/// <summary>
/// Example demonstrating how to use the Event Bus system.
/// This script shows best practices for subscribing, unsubscribing, and handling events.
/// </summary>
public class EventBusExample : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to events when the component is enabled
        EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Instance.Subscribe<GameOverEvent>(OnGameOver);
        EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Subscribe<HighScoreAchievedEvent>(OnHighScoreAchieved);
    }

    private void OnDisable()
    {
        // IMPORTANT: Always unsubscribe when disabled to prevent memory leaks
        EventBus.Instance.Unsubscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Instance.Unsubscribe<GameOverEvent>(OnGameOver);
        EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Unsubscribe<HighScoreAchievedEvent>(OnHighScoreAchieved);
    }

    // Event Handlers
    private void OnGameStarted(GameStartedEvent evt)
    {
        Debug.Log($"[EventBusExample] Game started with speed: {evt.GameSpeed}");
        // Add your logic here (e.g., start music, show tutorial, etc.)
    }

    private void OnGameOver(GameOverEvent evt)
    {
        Debug.Log($"[EventBusExample] Game over! Final score: {evt.FinalScore}, Reason: {evt.Reason}");
        // Add your logic here (e.g., show game over screen, save stats, etc.)
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        Debug.Log($"[EventBusExample] Score changed from {evt.OldScore} to {evt.NewScore} (Delta: {evt.Delta})");
        // Add your logic here (e.g., update UI, play sound effect, etc.)
    }

    private void OnHighScoreAchieved(HighScoreAchievedEvent evt)
    {
        Debug.Log($"[EventBusExample] NEW HIGH SCORE! Old: {evt.OldHighScore}, New: {evt.NewHighScore}");
        // Add your logic here (e.g., show celebration animation, unlock achievement, etc.)
    }

    // Example: How to publish a custom event
    private void Update()
    {
        // Example: Publish a button click event when space is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EventBus.Instance.Publish(new ButtonClickedEvent("SpaceButton", "Jump"));
        }
    }
}
