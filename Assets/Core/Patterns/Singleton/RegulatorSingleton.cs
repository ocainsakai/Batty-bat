using UnityEngine;

namespace Core.Patterns
{
    /// <summary>
    /// Regulator Singleton pattern.
    /// Allows newer instances to replace older ones based on initialization time.
    /// Commonly used for Audio Managers where you want the newest instance to take control.
    /// 
    /// Unlike regular singleton that destroys duplicates, this keeps the newest instance
    /// and destroys the older one.
    /// </summary>
    /// <typeparam name="T">The type of the singleton MonoBehaviour</typeparam>
    public class RegulatorSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static float _instanceInitTime;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        private float _myInitTime;

        /// <summary>
        /// Gets the singleton instance.
        /// Returns the most recently initialized instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[RegulatorSingleton] Instance of {typeof(T)} already destroyed. Returning null.");
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
                            singletonObject.name = $"{typeof(T).Name} (Regulator Singleton)";

                            Debug.Log($"[RegulatorSingleton] Created new instance of {typeof(T)}");
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
        /// Compares initialization time and keeps the newer instance.
        /// </summary>
        protected virtual void Awake()
        {
            _myInitTime = Time.time;

            lock (_lock)
            {
                if (_instance == null)
                {
                    // First instance - keep it
                    _instance = this as T;
                    _instanceInitTime = _myInitTime;
                    Debug.Log($"[RegulatorSingleton] {typeof(T)} initialized at time {_myInitTime:F3}");
                }
                else if (_instance == this)
                {
                    // Already the instance - do nothing
                    return;
                }
                else
                {
                    // Compare initialization times
                    if (_myInitTime > _instanceInitTime)
                    {
                        // This instance is newer - replace the old one
                        Debug.Log($"[RegulatorSingleton] Replacing old {typeof(T)} (time {_instanceInitTime:F3}) with newer instance (time {_myInitTime:F3})");
                        
                        GameObject oldObject = _instance.gameObject;
                        _instance = this as T;
                        _instanceInitTime = _myInitTime;
                        
                        // Destroy the old instance
                        Destroy(oldObject);
                    }
                    else
                    {
                        // This instance is older - destroy it
                        Debug.Log($"[RegulatorSingleton] Destroying older {typeof(T)} instance (time {_myInitTime:F3}). Current instance time: {_instanceInitTime:F3}");
                        Destroy(gameObject);
                    }
                }
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
                _instanceInitTime = 0f;
            }
        }

        /// <summary>
        /// Check if this instance is the current singleton instance.
        /// </summary>
        protected bool IsCurrentInstance => _instance == this;
    }
}
