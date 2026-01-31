using System;
using System.Collections.Generic;

namespace BulletHellTemplate.Core.Events
{
    /// <summary>
    /// Simple global event bus that allows publishing and subscribing to strongly typed events.
    /// </summary>
    public static partial class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        /// <summary>
        /// Subscribe to a specific event type.
        /// </summary>
        public static void Subscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();

            _subscribers[type].Add(callback);
        }

        /// <summary>
        /// Unsubscribe from a specific event type.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0)
                    _subscribers.Remove(type);
            }
        }

        /// <summary>
        /// Publish an event to all listeners of the event type.
        /// </summary>
        public static void Publish<T>(T publishedEvent)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                foreach (var callback in list)
                {
                    if (callback is Action<T> action)
                        action.Invoke(publishedEvent);
                }
            }
        }

        /// <summary>
        /// Clears all subscribers from the bus. Use carefully!
        /// </summary>
        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }
}
