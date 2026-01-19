using UnityEngine;

namespace Core.Patterns.Examples
{
    /// <summary>
    /// Example of Pure C# Singleton pattern.
    /// This manager handles game data without needing MonoBehaviour.
    /// Thread-safe and can be accessed from any thread.
    /// </summary>
    public class GameDataManager : PureSingleton<GameDataManager>
    {
        // Game data
        public int PlayerLevel { get; set; }
        public int PlayerScore { get; set; }
        public string PlayerName { get; set; }
        public float TotalPlayTime { get; private set; }

        // Constructor is public for PureSingleton (required by new() constraint)
        // But you should not call it directly - use Instance instead
        public GameDataManager()
        {
            // Initialize default values
            PlayerLevel = 1;
            PlayerScore = 0;
            PlayerName = "Player";
            TotalPlayTime = 0f;

            Debug.Log("[GameDataManager] Initialized");
        }

        public void AddScore(int points)
        {
            PlayerScore += points;
            Debug.Log($"[GameDataManager] Score: {PlayerScore}");
        }

        public void LevelUp()
        {
            PlayerLevel++;
            Debug.Log($"[GameDataManager] Level up! Now level {PlayerLevel}");
        }

        public void AddPlayTime(float seconds)
        {
            TotalPlayTime += seconds;
        }

        public void ResetData()
        {
            PlayerLevel = 1;
            PlayerScore = 0;
            PlayerName = "Player";
            TotalPlayTime = 0f;
            Debug.Log("[GameDataManager] Data reset");
        }
    }
}
