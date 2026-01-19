using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.EventSystem
{
    /// <summary>
    /// Central Event Bus for managing game-wide events.
    /// Provides a decoupled communication system between components.
    /// </summary>
    public class EventBus
    {
        private static EventBus _instance;
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventBus();
                }
                return _instance;
            }
        }

        // Dictionary to store event type -> list of listeners
        private readonly Dictionary<Type, List<Delegate>> _eventListeners = new Dictionary<Type, List<Delegate>>();

        // Lock object for thread safety (if needed in the future)
        private readonly object _lock = new object();

        /// <summary>
        /// Subscribe to an event of type T
        /// </summary>
        /// <typeparam name="T">Event type that implements IEvent</typeparam>
        /// <param name="listener">Callback action to invoke when event is raised</param>
        public void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);

            lock (_lock)
            {
                if (!_eventListeners.ContainsKey(eventType))
                {
                    _eventListeners[eventType] = new List<Delegate>();
                }

                if (!_eventListeners[eventType].Contains(listener))
                {
                    _eventListeners[eventType].Add(listener);
                }
                else
                {
                    Debug.LogWarning($"[EventBus] Listener already subscribed to {eventType.Name}");
                }
            }
        }

        /// <summary>
        /// Unsubscribe from an event of type T
        /// </summary>
        /// <typeparam name="T">Event type that implements IEvent</typeparam>
        /// <param name="listener">Callback action to remove</param>
        public void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);

            lock (_lock)
            {
                if (_eventListeners.ContainsKey(eventType))
                {
                    _eventListeners[eventType].Remove(listener);

                    // Clean up empty lists
                    if (_eventListeners[eventType].Count == 0)
                    {
                        _eventListeners.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// Publish/Raise an event to all subscribed listeners
        /// </summary>
        /// <typeparam name="T">Event type that implements IEvent</typeparam>
        /// <param name="eventData">The event data to send to listeners</param>
        public void Publish<T>(T eventData) where T : IEvent
        {
            Type eventType = typeof(T);

            List<Delegate> listeners = null;

            lock (_lock)
            {
                if (_eventListeners.ContainsKey(eventType))
                {
                    // Create a copy to avoid modification during iteration
                    listeners = new List<Delegate>(_eventListeners[eventType]);
                }
            }

            if (listeners != null)
            {
                foreach (var listener in listeners)
                {
                    try
                    {
                        (listener as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[EventBus] Error invoking listener for {eventType.Name}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _eventListeners.Clear();
            }
        }

        /// <summary>
        /// Clear all subscriptions for a specific event type
        /// </summary>
        /// <typeparam name="T">Event type to clear</typeparam>
        public void Clear<T>() where T : IEvent
        {
            Type eventType = typeof(T);

            lock (_lock)
            {
                if (_eventListeners.ContainsKey(eventType))
                {
                    _eventListeners.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// Get the number of listeners for a specific event type
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <returns>Number of listeners</returns>
        public int GetListenerCount<T>() where T : IEvent
        {
            Type eventType = typeof(T);

            lock (_lock)
            {
                if (_eventListeners.ContainsKey(eventType))
                {
                    return _eventListeners[eventType].Count;
                }
            }

            return 0;
        }

        /// <summary>
        /// Check if there are any listeners for a specific event type
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <returns>True if there are listeners</returns>
        public bool HasListeners<T>() where T : IEvent
        {
            return GetListenerCount<T>() > 0;
        }
    }
}
