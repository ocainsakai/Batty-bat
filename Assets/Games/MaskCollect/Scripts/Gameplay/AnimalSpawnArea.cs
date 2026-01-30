using UnityEngine;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Marks an area where animals can spawn.
    /// Uses child transforms as spawn points.
    /// </summary>
    public class AnimalSpawnArea : MonoBehaviour
    {
        [Tooltip("Spawn points - auto-collected from children if empty")]
        public Transform[] spawnPoints;

        [Tooltip("Random offset range for spawn positions")]
        public Vector2 randomOffset = new Vector2(0.5f, 0.3f);

        private void Awake()
        {
            CollectSpawnPoints();
        }

        private void CollectSpawnPoints()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                spawnPoints = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    spawnPoints[i] = transform.GetChild(i);
                }
            }
        }

        /// <summary>
        /// Get a random spawn position from available spawn points.
        /// </summary>
        public Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return transform.position;

            Vector3 basePos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            
            // Add random offset
            float xOffset = Random.Range(-randomOffset.x, randomOffset.x);
            float yOffset = Random.Range(-randomOffset.y, randomOffset.y);
            
            return basePos + new Vector3(xOffset, yOffset, 0);
        }

        /// <summary>
        /// Get spawn position at specific index.
        /// </summary>
        public Vector3 GetSpawnPosition(int index)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return transform.position;

            index = Mathf.Clamp(index, 0, spawnPoints.Length - 1);
            return spawnPoints[index].position;
        }

        /// <summary>
        /// Get all spawn positions.
        /// </summary>
        public Vector3[] GetAllSpawnPositions()
        {
            CollectSpawnPoints();
            
            Vector3[] positions = new Vector3[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                positions[i] = spawnPoints[i].position;
            }
            return positions;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw spawn points in editor
            Gizmos.color = Color.green;
            
            if (spawnPoints != null)
            {
                foreach (var point in spawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.3f);
                        Gizmos.DrawWireCube(point.position, new Vector3(randomOffset.x * 2, randomOffset.y * 2, 0));
                    }
                }
            }
            else
            {
                // Draw children as spawn points
                foreach (Transform child in transform)
                {
                    Gizmos.DrawWireSphere(child.position, 0.3f);
                }
            }
        }
    }
}
