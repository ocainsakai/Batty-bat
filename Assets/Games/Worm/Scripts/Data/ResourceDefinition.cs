using Core.Systems.CollectableSystem;
using UnityEngine;

namespace Games.Worm.Data
{
    /// <summary>
    /// ScriptableObject defining a resource type that the worm can collect.
    /// Extends CollectibleDefinition from Core with Worm-specific properties.
    /// </summary>
    [CreateAssetMenu(fileName = "Resource", menuName = "Worm/Resource Definition", order = 1)]
    public class ResourceDefinition : CollectibleDefinition
    {
        // Convenience aliases for backward compatibility
        public string ResourceName => CollectibleName;
        public string ResourceID => CollectibleID;
        public Color ResourceColor => CollectibleColor;
        public Sprite ResourceSprite => CollectibleSprite;

        [Header("Worm-Specific Gameplay")]
        [Tooltip("How much the worm grows when collecting this resource")]
        [Range(1, 100)]
        public int GrowthValue = 1;
    }
}
