using UnityEngine;
using Core.EventSystem;
using Games.Worm.Events;

namespace Games.Worm
{
    /// <summary>
    /// Game Manager for Worm game.
    /// This is a template - implement your game logic here.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

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
            // Clean up event subscriptions
            EventBus.Instance.Clear();
        }

        private void Start()
        {
            // TODO: Initialize your game
            Debug.Log("[Worm] Game Manager initialized");
        }

        public void StartGame()
        {
            // TODO: Implement game start logic
            EventBus.Instance.Publish(new GameStartedEvent(1f));
        }

        public void GameOver()
        {
            // TODO: Implement game over logic
            EventBus.Instance.Publish(new GameOverEvent(0, "Game Over"));
        }
    }
}
