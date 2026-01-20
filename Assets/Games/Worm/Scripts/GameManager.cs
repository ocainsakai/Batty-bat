using UnityEngine;
using Core.EventSystem;
using Core.Patterns;
using Games.Worm.Events;

namespace Games.Worm
{
    /// <summary>
    /// Game Manager for Worm game.
    /// This is a template - implement your game logic here.
    /// </summary>
    public class GameManager : MonoSingleton<GameManager>
    {
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Clean up event subscriptions
            EventBus.Clear();
        }

        private void Start()
        {
            // TODO: Initialize your game
            Debug.Log("[Worm] Game Manager initialized");
        }

        public void StartGame()
        {
            // TODO: Implement game start logic
            EventBus.Publish(new GameStartedEvent(1f));
        }

        public void GameOver()
        {
            
        }
    }
}
