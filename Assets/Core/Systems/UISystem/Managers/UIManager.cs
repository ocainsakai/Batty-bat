using System.Collections.Generic;
using UnityEngine;
using Core.Patterns;
using Core.EventSystem;
using Cysharp.Threading.Tasks;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// Central manager for UI screens and popups.
    /// Handles navigation, screen history, and popup stacking.
    /// </summary>
    public class UIManager : PersistentSingleton<UIManager>
    {
        [Header("Settings")]
        [SerializeField] private bool handleBackButton = true;
        [SerializeField] private bool logNavigation = false;

        [Header("References")]
        [SerializeField] private Transform screensContainer;
        [SerializeField] private Transform popupsContainer;

        // Screen management
        private Dictionary<string, UIScreen> _registeredScreens = new Dictionary<string, UIScreen>();
        private Stack<UIScreen> _screenHistory = new Stack<UIScreen>();
        private UIScreen _activeScreen;

        // Popup management
        private List<UIPopup> _activePopups = new List<UIPopup>();
        private Dictionary<string, UIPopup> _registeredPopups = new Dictionary<string, UIPopup>();

        public UIScreen ActiveScreen => _activeScreen;
        public int ActivePopupCount => _activePopups.Count;
        public bool HasActivePopup => _activePopups.Count > 0;

        private void Update()
        {
            // Handle back button (Android/Escape key)
            if (handleBackButton && Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }

        #region Screen Management

        /// <summary>
        /// Register a screen with the UIManager
        /// </summary>
        public void RegisterScreen(UIScreen screen)
        {
            if (screen == null) return;
            
            string id = screen.ViewId;
            if (!_registeredScreens.ContainsKey(id))
            {
                _registeredScreens[id] = screen;
                Log($"Registered screen: {id}");
            }
        }

        /// <summary>
        /// Unregister a screen
        /// </summary>
        public void UnregisterScreen(UIScreen screen)
        {
            if (screen == null) return;
            
            string id = screen.ViewId;
            if (_registeredScreens.ContainsKey(id))
            {
                _registeredScreens.Remove(id);
                Log($"Unregistered screen: {id}");
            }
        }

        /// <summary>
        /// Show a screen by ID
        /// </summary>
        public async UniTask ShowScreen(string screenId)
        {
            if (_registeredScreens.TryGetValue(screenId, out var screen))
            {
                await screen.Show();
            }
            else
            {
                Debug.LogWarning($"[UIManager] Screen not found: {screenId}");
            }
        }

        /// <summary>
        /// Show a screen directly
        /// </summary>
        public async UniTask ShowScreen(UIScreen screen)
        {
            if (screen != null)
            {
                await screen.Show();
            }
        }

        /// <summary>
        /// Get a registered screen by ID
        /// </summary>
        public UIScreen GetScreen(string screenId)
        {
            return _registeredScreens.TryGetValue(screenId, out var screen) ? screen : null;
        }

        /// <summary>
        /// Called when a screen is about to show
        /// </summary>
        public void OnScreenShowing(UIScreen screen)
        {
            // Hide other screens if needed
            if (screen.HideOthersOnShow && _activeScreen != null && _activeScreen != screen)
            {
                // Add to history before hiding
                if (_activeScreen.CanGoBack)
                {
                    _screenHistory.Push(_activeScreen);
                }
                
                _activeScreen.HideImmediate();
            }
        }

        /// <summary>
        /// Set the active screen
        /// </summary>
        public void SetActiveScreen(UIScreen screen)
        {
            _activeScreen = screen;
            EventBus.Publish(new ScreenChangedEvent(screen));
            Log($"Active screen: {screen.ViewId}");
        }

        /// <summary>
        /// Called when a screen is hidden
        /// </summary>
        public void OnScreenHidden(UIScreen screen)
        {
            if (_activeScreen == screen)
            {
                _activeScreen = null;
            }
        }

        /// <summary>
        /// Go back to previous screen
        /// </summary>
        public async UniTask<bool> GoBack()
        {
            // First check popups
            if (HasActivePopup)
            {
                var topPopup = _activePopups[_activePopups.Count - 1];
                topPopup.OnBackPressed();
                return true;
            }

            // Then check screen history
            if (_screenHistory.Count > 0)
            {
                var previousScreen = _screenHistory.Pop();
                await previousScreen.Show();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear navigation history
        /// </summary>
        public void ClearHistory()
        {
            _screenHistory.Clear();
        }

        #endregion

        #region Popup Management

        /// <summary>
        /// Register a popup with the UIManager
        /// </summary>
        public void RegisterPopup(UIPopup popup)
        {
            if (popup == null) return;
            
            if (!_activePopups.Contains(popup))
            {
                _activePopups.Add(popup);
                Log($"Popup opened: {popup.ViewId}");
            }

            string id = popup.ViewId;
            if (!_registeredPopups.ContainsKey(id))
            {
                _registeredPopups[id] = popup;
            }
        }

        /// <summary>
        /// Unregister a popup
        /// </summary>
        public void UnregisterPopup(UIPopup popup)
        {
            if (popup == null) return;
            
            _activePopups.Remove(popup);
            Log($"Popup closed: {popup.ViewId}");
        }

        /// <summary>
        /// Show a popup by ID
        /// </summary>
        public async UniTask ShowPopup(string popupId)
        {
            if (_registeredPopups.TryGetValue(popupId, out var popup))
            {
                await popup.Show();
            }
            else
            {
                Debug.LogWarning($"[UIManager] Popup not found: {popupId}");
            }
        }

        /// <summary>
        /// Show a popup directly
        /// </summary>
        public async UniTask ShowPopup(UIPopup popup)
        {
            if (popup != null)
            {
                await popup.Show();
            }
        }

        /// <summary>
        /// Get a registered popup by ID
        /// </summary>
        public UIPopup GetPopup(string popupId)
        {
            return _registeredPopups.TryGetValue(popupId, out var popup) ? popup : null;
        }

        /// <summary>
        /// Close all active popups
        /// </summary>
        public async UniTask CloseAllPopups()
        {
            var popupsToClose = new List<UIPopup>(_activePopups);
            foreach (var popup in popupsToClose)
            {
                await popup.Hide();
            }
        }

        /// <summary>
        /// Close top popup
        /// </summary>
        public async UniTask CloseTopPopup()
        {
            if (HasActivePopup)
            {
                await _activePopups[_activePopups.Count - 1].Hide();
            }
        }

        #endregion

        #region Back Button

        private void HandleBackButton()
        {
            GoBack().Forget();
        }

        #endregion

        private void Log(string message)
        {
            if (logNavigation)
            {
                Debug.Log($"[UIManager] {message}");
            }
        }
    }

    #region Events

    public class ScreenChangedEvent : IEvent
    {
        public UIScreen NewScreen { get; set; }

        public ScreenChangedEvent(UIScreen newScreen)
        {
            NewScreen = newScreen;
        }
    }

    public class PopupOpenedEvent : IEvent
    {
        public UIPopup Popup { get; set; }

        public PopupOpenedEvent(UIPopup popup)
        {
            Popup = popup;
        }
    }

    public class PopupClosedEvent : IEvent
    {
        public UIPopup Popup { get; set; }

        public PopupClosedEvent(UIPopup popup)
        {
            Popup = popup;
        }
    }

    #endregion
}
