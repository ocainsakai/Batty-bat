using UnityEngine;
using DG.Tweening;

namespace MaskCollect.VFX
{
    /// <summary>
    /// Confetti particle effect for celebrations.
    /// Spawns colorful particles when player receives a mask.
    /// </summary>
    public class ConfettiEffect : MonoBehaviour
    {
        [Header("Particle System")]
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private ParticleSystem sparkleParticles;

        [Header("Settings")]
        [SerializeField] private int burstCount = 50;
        [SerializeField] private float duration = 2f;
        [SerializeField] private bool autoDestroy = true;

        [Header("Colors")]
        [SerializeField] private Color[] confettiColors = new Color[]
        {
            new Color(1f, 0.3f, 0.3f),      // Red
            new Color(1f, 0.7f, 0.2f),      // Orange
            new Color(1f, 1f, 0.3f),        // Yellow
            new Color(0.3f, 1f, 0.3f),      // Green
            new Color(0.3f, 0.7f, 1f),      // Blue
            new Color(0.8f, 0.3f, 1f),      // Purple
            new Color(1f, 0.4f, 0.7f)       // Pink
        };

        private static ConfettiEffect _instance;
        public static ConfettiEffect Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }

        private void Start()
        {
            if (confettiParticles == null)
            {
                CreateDefaultParticleSystem();
            }
        }

        private void CreateDefaultParticleSystem()
        {
            var go = new GameObject("ConfettiParticles");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            confettiParticles = go.AddComponent<ParticleSystem>();

            var main = confettiParticles.main;
            main.duration = duration;
            main.loop = false;
            main.startLifetime = 2f;
            main.startSpeed = 10f;
            main.startSize = 0.2f;
            main.gravityModifier = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = confettiParticles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0f, burstCount)
            });

            var shape = confettiParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.5f;

            var colorOverLifetime = confettiParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        /// <summary>
        /// Play confetti effect at position
        /// </summary>
        public void Play(Vector3 position)
        {
            transform.position = position;

            if (confettiParticles != null)
            {
                confettiParticles.Play();
            }

            if (sparkleParticles != null)
            {
                sparkleParticles.Play();
            }

            if (autoDestroy)
            {
                Destroy(gameObject, duration + 0.5f);
            }
        }

        /// <summary>
        /// Play confetti with custom burst count
        /// </summary>
        public void Play(Vector3 position, int customBurstCount)
        {
            if (confettiParticles != null)
            {
                var emission = confettiParticles.emission;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                    new ParticleSystem.Burst(0f, customBurstCount)
                });
            }

            Play(position);
        }

        /// <summary>
        /// Spawn a confetti instance at position
        /// </summary>
        public static ConfettiEffect SpawnAt(Vector3 position, ConfettiEffect prefab = null)
        {
            ConfettiEffect instance;

            if (prefab != null)
            {
                instance = Instantiate(prefab, position, Quaternion.identity);
            }
            else
            {
                var go = new GameObject("Confetti");
                go.transform.position = position;
                instance = go.AddComponent<ConfettiEffect>();
            }

            instance.Play(position);
            return instance;
        }
    }
}
