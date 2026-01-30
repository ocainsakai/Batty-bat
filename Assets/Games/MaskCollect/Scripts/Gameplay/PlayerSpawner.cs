using UnityEngine;

namespace MaskCollect.Gameplay
{
    /// <summary>
    /// Spawns player at the start of gameplay.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool spawnOnStart = true;

        private PlayerController _currentPlayer;

        public PlayerController CurrentPlayer => _currentPlayer;

        private static PlayerSpawner _instance;
        public static PlayerSpawner Instance => _instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnPlayer();
            }
        }

        /// <summary>
        /// Spawn the player at the spawn point.
        /// </summary>
        public PlayerController SpawnPlayer()
        {
            if (_currentPlayer != null)
            {
                Debug.Log("[PlayerSpawner] Player already exists");
                return _currentPlayer;
            }

            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

            if (playerPrefab != null)
            {
                GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                playerObj.name = "Player";
                _currentPlayer = playerObj.GetComponent<PlayerController>();

                if (_currentPlayer == null)
                {
                    _currentPlayer = playerObj.AddComponent<PlayerController>();
                }

                Debug.Log($"[PlayerSpawner] Player spawned at {spawnPos}");
            }
            else
            {
                // Create simple player if no prefab
                _currentPlayer = CreateSimplePlayer(spawnPos);
            }

            return _currentPlayer;
        }

        /// <summary>
        /// Create a simple player without prefab (for testing).
        /// </summary>
        private PlayerController CreateSimplePlayer(Vector3 position)
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.transform.position = position;

            // Add sprite renderer
            var sr = playerObj.AddComponent<SpriteRenderer>();
            sr.color = Color.white;
            sr.sortingOrder = 100;

            // Add rigidbody
            var rb = playerObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Add collider
            var col = playerObj.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 1.2f);

            // Add controller
            var controller = playerObj.AddComponent<PlayerController>();

            Debug.Log("[PlayerSpawner] Created simple player (no prefab assigned)");

            return controller;
        }

        /// <summary>
        /// Respawn player at spawn point.
        /// </summary>
        public void RespawnPlayer()
        {
            if (_currentPlayer != null)
            {
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                _currentPlayer.transform.position = spawnPos;
                _currentPlayer.CanMove = true;
                Debug.Log("[PlayerSpawner] Player respawned");
            }
            else
            {
                SpawnPlayer();
            }
        }

        /// <summary>
        /// Destroy current player.
        /// </summary>
        public void DestroyPlayer()
        {
            if (_currentPlayer != null)
            {
                Destroy(_currentPlayer.gameObject);
                _currentPlayer = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Gizmos.DrawWireSphere(pos, 0.5f);
            Gizmos.DrawIcon(pos, "d_Animation.Record", true);
        }
    }
}
