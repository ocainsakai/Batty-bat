using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// MonoBehaviour Singleton pattern.
    /// Regular singleton that exists only in the current scene.
    /// Use this for scene-specific managers (e.g., LevelManager, UIManager).
    /// </summary>
    /// <typeparam name="T">The type of the singleton MonoBehaviour</typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// Gets the singleton instance.
        /// Auto-creates a GameObject with the component if it doesn't exist.
        /// Returns null if application is quitting to prevent object creation during shutdown.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Try to find existing instance in scene
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            // Create new GameObject with the component
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name} (Singleton)";

                            Debug.Log($"[MonoSingleton] Created new instance of {typeof(T)}");
                        }
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Check if the singleton instance exists.
        /// </summary>
        public static bool HasInstance => _instance != null;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Ensures only one instance exists.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T)} detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Called when the application is quitting.
        /// Prevents creating new instances during shutdown.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// Called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
