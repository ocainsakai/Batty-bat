using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Core.EventSystem
{
    /// <summary>
    /// Debug tool for monitoring Event Bus activity.
    /// Attach this to a GameObject in your scene to see event flow in real-time.
    /// Note: This debugger needs to be updated to subscribe to game-specific events.
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

        /// <summary>
        /// Log a custom event. Call this method from your event handlers.
        /// </summary>
        public void LogEvent(string eventType, string eventData)
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
}
