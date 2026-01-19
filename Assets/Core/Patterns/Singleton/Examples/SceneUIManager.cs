using UnityEngine;

namespace Core.Patterns.Examples
{
    /// <summary>
    /// Example of Regular MonoBehaviour Singleton pattern.
    /// This manager exists only in the current scene and is destroyed when scene changes.
    /// Perfect for scene-specific functionality like UI management.
    /// </summary>
    public class SceneUIManager : MonoSingleton<SceneUIManager>
    {
        [Header("UI References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject pauseMenu;

        private bool _isPaused = false;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("[SceneUIManager] Initialized for current scene");
        }

        private void Start()
        {
            // Initialize UI
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }

        public void ShowPauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
                _isPaused = true;
                Time.timeScale = 0f;
                Debug.Log("[SceneUIManager] Game paused");
            }
        }

        public void HidePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
                _isPaused = false;
                Time.timeScale = 1f;
                Debug.Log("[SceneUIManager] Game resumed");
            }
        }

        public void TogglePause()
        {
            if (_isPaused)
            {
                HidePauseMenu();
            }
            else
            {
                ShowPauseMenu();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Reset time scale when destroyed
            Time.timeScale = 1f;
            Debug.Log("[SceneUIManager] Destroyed");
        }
    }
}
