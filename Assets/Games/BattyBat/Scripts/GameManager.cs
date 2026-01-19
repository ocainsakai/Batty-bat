using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Core.EventSystem;
using Games.BattyBat.Events;

namespace Games.BattyBat
{
    public enum GameState
    {
        Screen,
        Playing,
        Paused,
        Lose,
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] BattyBat _battyBat;
        public BattyBat BattyBat => _battyBat;
        [SerializeField] Environment _environment;
        [SerializeField] Spawner _spawner;

        [Header("UI")]
        [SerializeField] GameObject _startScreen;
        [SerializeField] Button _startButton;
        [SerializeField] GameObject _losePopup;
        [SerializeField] Button _restartButton;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] TMP_Text _highScoreText;

        [SerializeField, Range(0.1f, 10f)] private float _gameSpeed = 1f;
        private GameState _gameState;
        public GameState CurrentState => _gameState;
        private float GameTime;
        private int _currentScore;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions when GameManager is destroyed
            // This prevents memory leaks
            EventBus.Clear();
        }

        private void Start()
        {
            InitilizeGame();
        }

        public void InitilizeGame()
        {
            if (_startButton)
            {
                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(StartGame);
            }
            if (_restartButton)
            {
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(StartGame);
            }

            if (_startScreen) _startScreen.SetActive(true);
            if (_losePopup) _losePopup.SetActive(false);

            UpdateScoreUI();

            _gameState = GameState.Screen;
        }

        public void StartGame()
        {
            // If we are in Lose state, reload the scene to restart fresh
            if (_gameState == GameState.Lose)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            GameTime = 0f;
            _currentScore = 0;
            UpdateScoreUI();

            _gameState = GameState.Playing;

            if (_startScreen) _startScreen.SetActive(false);
            if (_losePopup) _losePopup.SetActive(false);

            Rosin.RosinSpeed = 5f * _gameSpeed;

            // Ensure BattyBat is active/reset if needed (though scene reload handles it usually)
            if (_battyBat != null) _battyBat.gameObject.SetActive(true);

            // Publish GameStartedEvent
            EventBus.Publish(new GameStartedEvent(_gameSpeed));
        }

        public void AddScore(int amount)
        {
            int oldScore = _currentScore;
            _currentScore += amount;
            UpdateScoreUI();

            // Publish ScoreChangedEvent
            EventBus.Publish(new ScoreChangedEvent(oldScore, _currentScore));
        }

        private void UpdateScoreUI()
        {
            if (_scoreText) _scoreText.text = _currentScore.ToString();
        }

        public void GameOver()
        {
            if (_gameState == GameState.Lose) return;

            _gameState = GameState.Lose;
            Rosin.RosinSpeed = 0f;

            if (_losePopup) _losePopup.SetActive(true);
            // Ensure start screen is off
            if (_startScreen) _startScreen.SetActive(false);

            CheckHighScore();

            // Publish GameOverEvent
            EventBus.Publish(new GameOverEvent(_currentScore));

            Debug.Log("Game Over!");
        }

        private void CheckHighScore()
        {
            int oldHighScore = PlayerPrefs.GetInt("HighScore", 0);
            if (_currentScore > oldHighScore)
            {
                PlayerPrefs.SetInt("HighScore", _currentScore);
                PlayerPrefs.Save();

                // Publish HighScoreAchievedEvent
                EventBus.Publish(new HighScoreAchievedEvent(oldHighScore, _currentScore));
            }

            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (_highScoreText)
            {
                _highScoreText.text = $"High Score: {highScore}\nScore: {_currentScore}";
            }
        }

        private void Update()
        {
            if (_gameState == GameState.Playing)
            {
                GameTime += Time.deltaTime;
                Rosin.RosinSpeed = (5f * (1 + GameTime / 1000f)) * _gameSpeed;
                Spawner.SpawnerTime = 1.5f / _gameSpeed;
            }
        }
    }
}

