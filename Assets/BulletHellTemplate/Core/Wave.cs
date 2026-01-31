using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a wave of monsters with specific configurations.
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        public List<MonsterConfig> monsters; // List of monster configurations in this wave
        public float waveDuration; // Duration of the wave before the next one starts
        public bool spawnBossAfterWave; // Indicates if a boss should spawn after this wave

        /// <summary>
        /// Constructor to initialize a new wave with the specified parameters.
        /// </summary>
        /// <param name="monsters">List of monster configurations to spawn in the wave.</param>
        /// <param name="waveDuration">Duration of the wave.</param>
        /// <param name="spawnBossAfterWave">Indicates if a boss should spawn after this wave.</param>
        public Wave(List<MonsterConfig> monsters, float waveDuration, bool spawnBossAfterWave = false)
        {
            this.monsters = monsters;
            this.waveDuration = waveDuration;
            this.spawnBossAfterWave = spawnBossAfterWave;
        }
    }

    /// <summary>
    /// Represents the configuration for a specific type of monster in a wave.
    /// </summary>
    [System.Serializable]
    public class MonsterConfig
    {
        public MonsterEntity monsterPrefab; // The monster passEntryPrefab to spawn
        public float spawnInterval; // Time interval between monster spawns
        public int goldPerMonster; // Gold gained per monster
        public int xpPerMonster; // XP gained per monster

        /// <summary>
        /// Constructor to initialize a new monster configuration with the specified parameters.
        /// </summary>
        /// <param name="monsterPrefab">The monster passEntryPrefab to spawn.</param>
        /// <param name="monsterCount">Number of monsters to spawn.</param>
        /// <param name="spawnInterval">Time interval between spawns.</param>
        /// <param name="goldPerMonster">Gold gained per monster.</param>
        /// <param name="xpPerMonster">XP gained per monster.</param>
        public MonsterConfig(MonsterEntity monsterPrefab, float spawnInterval, int goldPerMonster, int xpPerMonster)
        {
            this.monsterPrefab = monsterPrefab;
            this.spawnInterval = spawnInterval;
            this.goldPerMonster = goldPerMonster;
            this.xpPerMonster = xpPerMonster;
        }
    }
}