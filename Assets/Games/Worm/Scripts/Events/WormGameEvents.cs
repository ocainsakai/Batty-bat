using UnityEngine;
using Core.EventSystem;
using Games.Worm.Data;

namespace Games.Worm.Events
{
    /// <summary>
    /// Game-level events for Worm game
    /// </summary>

    public class GameStartedEvent : IEvent
    {
        public float StartTime { get; set; }

        public GameStartedEvent(float startTime)
        {
            StartTime = startTime;
        }
    }

    public class GameOverEvent : IEvent
    {
        public float FinalSize { get; set; }
        public int TotalResourcesCollected { get; set; }
        public float PlayTime { get; set; }

        public GameOverEvent(float finalSize, int totalResources, float playTime)
        {
            FinalSize = finalSize;
            TotalResourcesCollected = totalResources;
            PlayTime = playTime;
        }
    }

    public class GamePausedEvent : IEvent
    {
        public bool IsPaused { get; set; }

        public GamePausedEvent(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}
