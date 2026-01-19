using System;

namespace Core.Patterns
{
    /// <summary>
    /// Pure C# Singleton pattern (non-MonoBehaviour).
    /// Thread-safe implementation with lazy initialization.
    /// Use this for data managers, services, and other non-Unity classes.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class</typeparam>
    public class PureSingleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the singleton instance.
        /// Creates the instance on first access (lazy initialization).
        /// Thread-safe using double-check locking pattern.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Explicitly destroy the singleton instance.
        /// Use with caution - mainly for testing purposes.
        /// </summary>
        public static void DestroyInstance()
        {
            lock (_lock)
            {
                if (_instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _instance = null;
            }
        }

        /// <summary>
        /// Check if the singleton instance exists.
        /// </summary>
        public static bool HasInstance => _instance != null;
    }
}
