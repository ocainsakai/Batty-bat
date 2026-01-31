using UnityEngine;
using UnityEngine.AI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents spawn points for enemies, including a radius for spawn area and avoidance layer for obstacles.
    /// </summary>
    public class SpawnPoints : MonoBehaviour
    {
        [Header("Spawn Area Settings")]
        [Tooltip("Radius of the spawn area.")]
        public float radius = 10f; // Radius for random spawn area

        [Tooltip("Layer to avoid when spawning (default: Wall).")]
        public LayerMask layerAvoid; // Layer to avoid (initialized in Awake)

        /// <summary>
        /// Initializes the layerAvoid with the default "Wall" layer.
        /// </summary>
        private void Awake()
        {
            // Initialize layerAvoid in Awake to avoid calling LayerMask.GetMask in the constructor
            layerAvoid = LayerMask.GetMask("Wall");
        }

        /// <summary>
        /// Draws a gizmo in the editor to visualize the spawn area radius.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        /// <summary>
        /// Gets a random spawn point within the defined radius, ensuring the point is on the NavMesh.
        /// </summary>
        /// <returns>A random Vector3 position within the radius that is valid on the NavMesh.</returns>
        public Vector3 GetRandomSpawnPoint()
        {
            Vector3 randomPoint;
            int safetyCounter = 0; // Safety counter to avoid infinite loops

            do
            {
                // Generate a random point within the radius
                Vector3 randomDirection = Random.insideUnitSphere * radius;
                randomDirection.y = 0; // Keep the spawn point on the same y level
                Vector3 randomPosition = transform.position + randomDirection;

                // Sample the NavMesh to find a valid position near the generated point
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPosition, out hit, radius, NavMesh.AllAreas))
                {
                    randomPoint = hit.position; // Use the sampled position on the NavMesh
                    break; // Exit loop if a valid position is found
                }

                safetyCounter++;
                // Avoid getting stuck in an infinite loop by limiting checks
                if (safetyCounter > 30)
                {
                    Debug.LogWarning("Could not find a valid spawn point within the radius.");
                    randomPoint = transform.position; // Default to the center point if no valid point is found
                    break;
                }

            } while (true);

            return randomPoint;
        }
    }
}
