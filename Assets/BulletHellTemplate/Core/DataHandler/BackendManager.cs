using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace BulletHellTemplate
{
    /// <summary>
    /// Facade that exposes a single <see cref="IBackendService"/> implementation chosen at runtime.
    /// Register exactly one concrete service (Offline, Firebase, WebSocket-SQL, etc.)
    /// in <c>BackendLifetimeScope</c>; all calls made through this manager are
    /// forwarded to that implementation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BackendManager : MonoBehaviour
    {
        /// <summary>
        /// Globally accessible backend service selected during application boot.
        /// </summary>
        public static IBackendService Service { get; private set; }

        private static bool isInitialized;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        public static void SetBackendInitialized() => isInitialized = true;

        /// <summary>
        /// Waits until the backend is fully initialized.
        /// </summary>
        /// <returns>Returns true when initialized.</returns>
        public static async UniTask<bool> CheckInitialized()
        {
            // Wait until the static flag is set
            await UniTask.WaitUntil(() => isInitialized);
            return true;
        }

        /// <summary>
        /// VContainer injects the concrete implementation bound to <see cref="IBackendService"/>.
        /// </summary>
        /// <param name="backend">Concrete backend implementation.</param>
        [Inject]
        public void Construct(IBackendService backend)
        {
            Service = backend;
            SetBackendInitialized();
            Debug.Log("Backend Injected");
        }      
    }
}
