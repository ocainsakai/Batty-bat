using UnityEngine;

namespace Games.Worm.Data
{
    /// <summary>
    /// ScriptableObject defining a resource type that the worm can collect.
    /// Data-driven approach for easy balancing and expansion.
    /// </summary>
    [CreateAssetMenu(fileName = "Resource", menuName = "Worm/Resource Definition", order = 1)]
    public class ResourceDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the resource")]
        public string ResourceName = "Resource";
        
        [Tooltip("Unique identifier for this resource type")]
        public string ResourceID = "resource_default";

        [Header("Visuals")]
        [Tooltip("Color of the resource dot")]
        public Color ResourceColor = Color.white;
        
        [Tooltip("Optional sprite (if null, uses circle)")]
        public Sprite ResourceSprite;
        
        [Tooltip("Size of the resource in world units")]
        [Range(0.1f, 2f)]
        public float Size = 0.5f;

        [Header("Gameplay")]
        [Tooltip("How much the worm grows when collecting this resource")]
        [Range(1, 100)]
        public int GrowthValue = 1;
        
        [Tooltip("Time in seconds before this resource respawns")]
        [Range(0f, 120f)]
        public float RespawnTime = 5f;

        [Header("Spawning")]
        [Tooltip("Maximum number of this resource type in the world")]
        [Range(1, 100)]
        public int MaxSpawnCount = 10;
        
        [Tooltip("Spawn distribution pattern")]
        public SpawnPattern Pattern = SpawnPattern.Random;
        
        [Tooltip("Minimum spawn area bounds")]
        public Vector2 SpawnAreaMin = new Vector2(-10, -10);
        
        [Tooltip("Maximum spawn area bounds")]
        public Vector2 SpawnAreaMax = new Vector2(10, 10);

        [Header("Rarity (Future)")]
        [Tooltip("Rarity tier for this resource")]
        public ResourceRarity Rarity = ResourceRarity.Common;
    }

    public enum SpawnPattern
    {
        Random,      // Spawn randomly in area
        Grid,        // Spawn in grid pattern
        Cluster,     // Spawn in clusters
        Circle,      // Spawn in circle pattern
        Custom       // Custom spawn logic
    }

    public enum ResourceRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
