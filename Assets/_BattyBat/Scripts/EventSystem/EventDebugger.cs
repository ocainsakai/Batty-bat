using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Debug tool for monitoring Event Bus activity.
/// Attach this to a GameObject in your scene to see event flow in real-time.
/// </summary>
public class EventDebugger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _enableLogging = true;
    [SerializeField] private bool _showInConsole = true;
    [SerializeField] private int _maxHistorySize = 100;

    private Queue<EventLogEntry> _eventHistory = new Queue<EventLogEntry>();

    private class EventLogEntry
    {
        public string EventType;
        public string EventData;
        public float Timestamp;

        public EventLogEntry(string eventType, string eventData)
        {
            EventType = eventType;
            EventData = eventData;
            Timestamp = Time.time;
        }

        public override string ToString()
        {
            return $"[{Timestamp:F2}s] {EventType}: {EventData}";
        }
    }

    private void OnEnable()
    {
        if (!_enableLogging) return;

        // Subscribe to all common events for debugging
        EventBus.Instance.Subscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Instance.Subscribe<GameOverEvent>(OnGameOver);
        EventBus.Instance.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Subscribe<HighScoreAchievedEvent>(OnHighScoreAchieved);
        EventBus.Instance.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Instance.Subscribe<PlayerJumpedEvent>(OnPlayerJumped);
        EventBus.Instance.Subscribe<ButtonClickedEvent>(OnButtonClicked);
    }

    private void OnDisable()
    {
        if (!_enableLogging) return;

        EventBus.Instance.Unsubscribe<GameStartedEvent>(OnGameStarted);
        EventBus.Instance.Unsubscribe<GameOverEvent>(OnGameOver);
        EventBus.Instance.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance.Unsubscribe<HighScoreAchievedEvent>(OnHighScoreAchieved);
        EventBus.Instance.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
        EventBus.Instance.Unsubscribe<PlayerJumpedEvent>(OnPlayerJumped);
        EventBus.Instance.Unsubscribe<ButtonClickedEvent>(OnButtonClicked);
    }

    // Event Handlers
    private void OnGameStarted(GameStartedEvent evt)
    {
        LogEvent("GameStartedEvent", $"Speed: {evt.GameSpeed}");
    }

    private void OnGameOver(GameOverEvent evt)
    {
        LogEvent("GameOverEvent", $"Score: {evt.FinalScore}, Reason: {evt.Reason}");
    }

    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        LogEvent("ScoreChangedEvent", $"{evt.OldScore} → {evt.NewScore} (+{evt.Delta})");
    }

    private void OnHighScoreAchieved(HighScoreAchievedEvent evt)
    {
        LogEvent("HighScoreAchievedEvent", $"Old: {evt.OldHighScore}, New: {evt.NewHighScore}");
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        LogEvent("PlayerDiedEvent", $"Position: {evt.DeathPosition}, Cause: {evt.CauseOfDeath}");
    }

    private void OnPlayerJumped(PlayerJumpedEvent evt)
    {
        LogEvent("PlayerJumpedEvent", $"Position: {evt.JumpPosition}, Force: {evt.JumpForce}");
    }

    private void OnButtonClicked(ButtonClickedEvent evt)
    {
        LogEvent("ButtonClickedEvent", $"Button: {evt.ButtonName}, Action: {evt.Action}");
    }

    private void LogEvent(string eventType, string eventData)
    {
        var entry = new EventLogEntry(eventType, eventData);
        
        _eventHistory.Enqueue(entry);
        
        // Maintain max history size
        while (_eventHistory.Count > _maxHistorySize)
        {
            _eventHistory.Dequeue();
        }

        if (_showInConsole)
        {
            Debug.Log($"[EventDebugger] {entry}");
        }
    }

    /// <summary>
    /// Get the event history as a formatted string
    /// </summary>
    public string GetEventHistory()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== Event History ===");
        
        foreach (var entry in _eventHistory)
        {
            sb.AppendLine(entry.ToString());
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Clear the event history
    /// </summary>
    public void ClearHistory()
    {
        _eventHistory.Clear();
    }

    /// <summary>
    /// Get the number of events in history
    /// </summary>
    public int GetEventCount()
    {
        return _eventHistory.Count;
    }
}
