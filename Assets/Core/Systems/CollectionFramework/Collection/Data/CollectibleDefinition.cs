using UnityEngine;

namespace Core.Systems.CollectableSystem
{
    /// <summary>
    /// ScriptableObject defining a collectable item type.
    /// Data-driven approach for easy balancing and expansion.
    /// Can be used for resources, coins, powerups, pickups, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "Collectable", menuName = "Core/Collectable Definition", order = 1)]
    public class CollectibleDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the collectable")]
        public string CollectibleName = "Collectable";
        
        [Tooltip("Unique identifier for this collectable type")]
        public string CollectibleID = "collectable_default";

        [Header("Visuals")]
        [Tooltip("Color of the collectable")]
        public Color CollectibleColor = Color.white;
        
        [Tooltip("Sprite for this collectable")]
        public Sprite CollectibleSprite;
        
        [Tooltip("Size of the collectable in world units")]
        [Range(0.1f, 5f)]
        public float Size = 0.5f;

        [Header("Gameplay")]
        [Tooltip("Generic value of this collectable (points, growth, currency, etc.)")]
        [Range(1, 1000)]
        public int Value = 1;
        
        [Tooltip("Type of this collectable (for game-specific logic)")]
        public CollectibleType Type = CollectibleType.Resource;
        
        [Tooltip("Time in seconds before this collectable respawns (0 = no respawn)")]
        [Range(0f, 300f)]
        public float RespawnTime = 5f;

        [Header("Spawning")]
        [Tooltip("Maximum number of this collectable type in the world")]
        [Range(1, 500)]
        public int MaxSpawnCount = 10;
        
        [Tooltip("Spawn distribution pattern")]
        public SpawnPattern Pattern = SpawnPattern.Random;
        
        [Tooltip("Minimum spawn area bounds")]
        public Vector2 SpawnAreaMin = new Vector2(-10, -10);
        
        [Tooltip("Maximum spawn area bounds")]
        public Vector2 SpawnAreaMax = new Vector2(10, 10);

        [Header("Rarity")]
        [Tooltip("Rarity tier for this collectable")]
        public CollectibleRarity Rarity = CollectibleRarity.Common;

        [Header("Effects (Optional)")]
        [Tooltip("Particle effect when collected")]
        public GameObject CollectionEffect;
        
        [Tooltip("Sound effect when collected")]
        public AudioClip CollectionSound;
    }

    public enum SpawnPattern
    {
        Random,      // Spawn randomly in area
        Grid,        // Spawn in grid pattern
        Cluster,     // Spawn in clusters
        Circle,      // Spawn in circle pattern
        Line,        // Spawn in line
        Custom       // Custom spawn logic
    }

    public enum CollectibleType
    {
        Resource,    // Generic resource
        Coin,        // Currency
        PowerUp,     // Temporary power-up
        Pickup,      // One-time pickup
        Consumable,  // Consumable item
        Custom       // Custom type
    }

    public enum CollectibleRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
