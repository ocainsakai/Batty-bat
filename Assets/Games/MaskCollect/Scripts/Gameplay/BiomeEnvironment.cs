using UnityEngine;
using MaskCollect.Data;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Root component for a biome environment prefab.
    /// Manages all layers and spawning for a biome.
    /// </summary>
    public class BiomeEnvironment : MonoBehaviour
    {
        [Header("Biome Data")]
        [SerializeField] private BiomeData biomeData;

        [Header("Layers")]
        [SerializeField] private Transform backgroundLayer;
        [SerializeField] private Transform groundLayer;
        [SerializeField] private Transform decorationLayer;
        [SerializeField] private Transform effectsLayer;

        [Header("Spawning")]
        [SerializeField] private AnimalSpawnArea animalSpawnArea;

        [Header("Settings")]
        [SerializeField] private bool autoSetupOnAwake = true;

        public BiomeData BiomeData => biomeData;
        public BiomeType BiomeType => biomeData != null ? biomeData.Type : BiomeType.PeacefulMeadow;

        private void Awake()
        {
            if (autoSetupOnAwake)
            {
                SetupEnvironment();
            }
        }

        /// <summary>
        /// Initialize the biome environment.
        /// </summary>
        public void SetupEnvironment()
        {
            if (biomeData == null)
            {
                Debug.LogWarning($"[BiomeEnvironment] No BiomeData assigned to {gameObject.name}");
                return;
            }

            // Apply biome-specific settings
            ApplyBiomeVisuals();

            Debug.Log($"[BiomeEnvironment] {biomeData.BiomeName} environment setup complete");
        }

        /// <summary>
        /// Apply visual settings from BiomeData.
        /// </summary>
        private void ApplyBiomeVisuals()
        {
            // Set background sprites if available in BiomeData
            if (biomeData.BackgroundSprite != null && backgroundLayer != null)
            {
                var bgRenderer = backgroundLayer.GetComponentInChildren<SpriteRenderer>();
                if (bgRenderer != null)
                {
                    bgRenderer.sprite = biomeData.BackgroundSprite;
                }
            }

            // Additional biome-specific setup can go here
        }

        /// <summary>
        /// Get a random spawn position for animals.
        /// </summary>
        public Vector3 GetAnimalSpawnPosition()
        {
            if (animalSpawnArea != null)
            {
                return animalSpawnArea.GetRandomSpawnPosition();
            }
            return transform.position;
        }

        /// <summary>
        /// Get all spawn positions.
        /// </summary>
        public Vector3[] GetAllSpawnPositions()
        {
            if (animalSpawnArea != null)
            {
                return animalSpawnArea.GetAllSpawnPositions();
            }
            return new Vector3[] { transform.position };
        }

        /// <summary>
        /// Enable/disable effects layer.
        /// </summary>
        public void SetEffectsEnabled(bool enabled)
        {
            if (effectsLayer != null)
            {
                effectsLayer.gameObject.SetActive(enabled);
            }
        }

        /// <summary>
        /// Reset all parallax layers to original positions.
        /// </summary>
        public void ResetParallax()
        {
            var parallaxLayers = GetComponentsInChildren<ParallaxLayer>();
            foreach (var layer in parallaxLayers)
            {
                layer.ResetPosition();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Auto-Setup References")]
        private void AutoSetupReferences()
        {
            backgroundLayer = transform.Find("BackgroundLayer");
            groundLayer = transform.Find("GroundLayer");
            decorationLayer = transform.Find("DecorationLayer");
            effectsLayer = transform.Find("EffectsLayer");
            animalSpawnArea = GetComponentInChildren<AnimalSpawnArea>();

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[BiomeEnvironment] References auto-setup complete");
        }
#endif
    }
}
