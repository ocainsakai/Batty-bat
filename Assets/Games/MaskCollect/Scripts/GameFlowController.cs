using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MaskCollect.Data;

namespace MaskCollect
{
    public enum GameState
    {
        Loading,
        MainMenu,
        WorldMap,
        Tutorial,
        Gameplay,
        Paused,
        CollectionBook
    }

    /// <summary>
    /// Controls the overall game flow and state transitions.
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskDatabase maskDatabase;
        [SerializeField] private BiomeDatabase biomeDatabase;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private BiomeManager biomeManager;

        [Header("UI Screens")]
        [SerializeField] private GameObject loadingScreen;
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject worldMapScreen;
        [SerializeField] private GameObject gameplayScreen;
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject collectionBookScreen;
        [SerializeField] private GameObject tutorialScreen;

        [Header("Settings")]
        [SerializeField] private float minimumLoadTime = 2f;
        [SerializeField] private bool skipToGameplay = false; // For testing

        private GameState _currentState = GameState.Loading;
        private GameState _previousState;
        private bool _hasSeenTutorial = false;

        public GameState CurrentState => _currentState;
        public event Action<GameState, GameState> OnStateChanged;

        private const string TUTORIAL_KEY = "MaskCollect_HasSeenTutorial";
        private const string FIRST_PLAY_KEY = "MaskCollect_FirstPlay";

        private static GameFlowController _instance;
        public static GameFlowController Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _hasSeenTutorial = PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1;
        }

        private async void Start()
        {
            if (skipToGameplay)
            {
                await TransitionToState(GameState.Gameplay);
                return;
            }

            await StartLoadingSequence();
        }

        #region Loading

        private async UniTask StartLoadingSequence()
        {
            SetState(GameState.Loading);
            ShowScreen(loadingScreen);

            float startTime = Time.time;

            // Load game data
            await LoadGameData();

            // Ensure minimum load time for UX
            float elapsed = Time.time - startTime;
            if (elapsed < minimumLoadTime)
            {
                await UniTask.WaitForSeconds(minimumLoadTime - elapsed);
            }

            // Transition to main menu
            await TransitionToState(GameState.MainMenu);
        }

        private async UniTask LoadGameData()
        {
            // Simulate loading steps (replace with actual loading if needed)
            await UniTask.Yield();

            // Initialize inventory
            if (inventoryManager != null)
            {
                inventoryManager.LoadInventory();
            }

            await UniTask.Yield();

            Debug.Log("[GameFlow] Game data loaded");
        }

        #endregion

        #region State Management

        public async UniTask TransitionToState(GameState newState)
        {
            if (_currentState == newState) 
                return;    

            _previousState = _currentState;
            
            // Hide current screen
            HideAllScreens();

            // Handle state exit
            await OnStateExit(_currentState);

            // Update state
            SetState(newState);

            // Handle state enter
            await OnStateEnter(newState);

            Debug.Log($"[GameFlow] State changed: {_previousState} -> {newState}");
        }

        private void SetState(GameState state)
        {
            var oldState = _currentState;
            _currentState = state;
            OnStateChanged?.Invoke(oldState, state);
        }

        private async UniTask OnStateExit(GameState state)
        {
            switch (state)
            {
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 1f;
                    break;
            }
            await UniTask.Yield();
        }

        private async UniTask OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.Loading:
                    ShowScreen(loadingScreen);
                    break;

                case GameState.MainMenu:
                    ShowScreen(mainMenuScreen);
                    break;

                case GameState.WorldMap:
                    ShowScreen(worldMapScreen);
                    break;

                case GameState.Tutorial:
                    ShowScreen(tutorialScreen);
                    break;

                case GameState.Gameplay:
                    ShowScreen(gameplayScreen);
                    // Check if should show tutorial
                    if (!_hasSeenTutorial && !skipToGameplay)
                    {
                        await ShowTutorial();
                    }
                    break;

                case GameState.Paused:
                    ShowScreen(pauseScreen);
                    Time.timeScale = 0f;
                    break;

                case GameState.CollectionBook:
                    ShowScreen(collectionBookScreen);
                    break;
            }
            await UniTask.Yield();
        }

        private void ShowScreen(GameObject screen)
        {
            if (screen != null)
            {
                screen.SetActive(true);
            }
        }

        private void HideAllScreens()
        {
            if (loadingScreen != null) loadingScreen.SetActive(false);
            if (mainMenuScreen != null) mainMenuScreen.SetActive(false);
            if (worldMapScreen != null) worldMapScreen.SetActive(false);
            if (gameplayScreen != null) gameplayScreen.SetActive(false);
            if (pauseScreen != null) pauseScreen.SetActive(false);
            if (collectionBookScreen != null) collectionBookScreen.SetActive(false);
            if (tutorialScreen != null) tutorialScreen.SetActive(false);
        }

        #endregion

        #region Public Actions (Called by UI)

        /// <summary>
        /// Called when player clicks Play button
        /// </summary>
        public async void OnPlayButtonClicked()
        {
            bool isFirstPlay = PlayerPrefs.GetInt(FIRST_PLAY_KEY, 1) == 1;

            if (isFirstPlay)
            {
                // First time: go straight to gameplay (Meadow)
                PlayerPrefs.SetInt(FIRST_PLAY_KEY, 0);
                PlayerPrefs.Save();
                await TransitionToState(GameState.Gameplay);
            }
            else
            {
                // Returning player: show world map
                await TransitionToState(GameState.WorldMap);
            }
        }

        /// <summary>
        /// Called when player selects a biome from world map
        /// </summary>
        public async void OnBiomeSelected(BiomeType biomeType)
        {
            if (biomeManager != null)
            {
                await biomeManager.EnterBiome(biomeType);
            }
            await TransitionToState(GameState.Gameplay);
        }

        /// <summary>
        /// Called when player clicks Collection button
        /// </summary>
        public async void OnCollectionButtonClicked()
        {
            await TransitionToState(GameState.CollectionBook);
        }

        /// <summary>
        /// Called when player clicks Settings button
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            // TODO: Open settings panel
            Debug.Log("[GameFlow] Settings clicked");
        }

        /// <summary>
        /// Called when player presses pause
        /// </summary>
        public async void OnPauseButtonClicked()
        {
            if (_currentState == GameState.Gameplay)
            {
                await TransitionToState(GameState.Paused);
            }
        }

        /// <summary>
        /// Called when player resumes from pause
        /// </summary>
        public async void OnResumeButtonClicked()
        {
            if (_currentState == GameState.Paused)
            {
                await TransitionToState(GameState.Gameplay);
            }
        }

        /// <summary>
        /// Go back to previous state or main menu
        /// </summary>
        public async void OnBackButtonClicked()
        {
            switch (_currentState)
            {
                case GameState.WorldMap:
                case GameState.CollectionBook:
                    await TransitionToState(GameState.MainMenu);
                    break;
                case GameState.Paused:
                    await TransitionToState(GameState.Gameplay);
                    break;
                case GameState.Gameplay:
                    await TransitionToState(GameState.WorldMap);
                    break;
            }
        }

        /// <summary>
        /// Return to main menu from anywhere
        /// </summary>
        public async void OnHomeButtonClicked()
        {
            Time.timeScale = 1f;
            await TransitionToState(GameState.MainMenu);
        }

        #endregion

        #region Tutorial

        private async UniTask ShowTutorial()
        {
            if (tutorialScreen != null)
            {
                tutorialScreen.SetActive(true);
                // Wait for tutorial to complete (handled by TutorialController)
                await UniTask.WaitUntil(() => !tutorialScreen.activeSelf);
            }

            _hasSeenTutorial = true;
            PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
            PlayerPrefs.Save();
        }

        public void CompleteTutorial()
        {
            _hasSeenTutorial = true;
            PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
            PlayerPrefs.Save();
            
            if (tutorialScreen != null)
            {
                tutorialScreen.SetActive(false);
            }
        }

        public void ResetTutorial()
        {
            _hasSeenTutorial = false;
            PlayerPrefs.SetInt(TUTORIAL_KEY, 0);
            PlayerPrefs.Save();
        }

        #endregion

        #region Save Data

        public bool HasSaveData()
        {
            return inventoryManager != null && inventoryManager.CollectedCount > 0;
        }

        public void DeleteSaveData()
        {
            if (inventoryManager != null)
            {
                inventoryManager.ClearInventory();
            }
            if (biomeManager != null)
            {
                biomeManager.ClearUnlockProgress();
            }
            PlayerPrefs.DeleteKey(TUTORIAL_KEY);
            PlayerPrefs.DeleteKey(FIRST_PLAY_KEY);
            PlayerPrefs.Save();
            Debug.Log("[GameFlow] Save data deleted");
        }

        #endregion
    }
}
