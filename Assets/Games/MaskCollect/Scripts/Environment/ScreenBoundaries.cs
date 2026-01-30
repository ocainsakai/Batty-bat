using UnityEngine;

namespace MaskCollect.Environment
{
    /// <summary>
    /// Automatically creates invisible colliders at screen edges to prevent
    /// objects from leaving the visible area.
    /// Attach this to an empty GameObject in the scene.
    /// </summary>
    public class ScreenBoundaries : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float colliderThickness = 1f;
        [SerializeField] private float padding = 0f; // Extra space inside screen edges
        [SerializeField] private bool generateOnStart = true;
        [SerializeField] private bool useWorldSpace = false; // If true, uses worldBounds instead of camera

        [Header("World Space Bounds (if useWorldSpace)")]
        [SerializeField] private Vector2 worldBoundsMin = new(-10f, -10f);
        [SerializeField] private Vector2 worldBoundsMax = new(10f, 10f);

        [Header("Layer Settings")]
        [SerializeField] private string boundaryLayerName = "Boundary";

        [Header("References")]
        [SerializeField] private Camera targetCamera;

        private GameObject _boundaryContainer;
        private BoxCollider2D _leftWall;
        private BoxCollider2D _rightWall;
        private BoxCollider2D _topWall;
        private BoxCollider2D _bottomWall;

        public Bounds ScreenBounds { get; private set; }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (generateOnStart)
            {
                GenerateBoundaries();
            }
        }

        /// <summary>
        /// Generate boundary colliders based on camera or world bounds
        /// </summary>
        [ContextMenu("Generate Boundaries")]
        public void GenerateBoundaries()
        {
            // Clean up existing boundaries
            if (_boundaryContainer != null)
            {
                DestroyImmediate(_boundaryContainer);
            }

            _boundaryContainer = new GameObject("Screen Boundaries");
            _boundaryContainer.transform.SetParent(transform);
            _boundaryContainer.transform.localPosition = Vector3.zero;

            // Calculate bounds
            Vector2 min, max;
            if (useWorldSpace)
            {
                min = worldBoundsMin;
                max = worldBoundsMax;
            }
            else
            {
                if (targetCamera == null)
                {
                    Debug.LogError("[ScreenBoundaries] No camera assigned!");
                    return;
                }

                // Get camera bounds
                float camHeight = targetCamera.orthographicSize * 2f;
                float camWidth = camHeight * targetCamera.aspect;
                Vector3 camPos = targetCamera.transform.position;

                min = new Vector2(camPos.x - camWidth / 2f + padding, camPos.y - camHeight / 2f + padding);
                max = new Vector2(camPos.x + camWidth / 2f - padding, camPos.y + camHeight / 2f - padding);
            }

            ScreenBounds = new Bounds(
                new Vector3((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0),
                new Vector3(max.x - min.x, max.y - min.y, 0)
            );

            float width = max.x - min.x;
            float height = max.y - min.y;
            float centerX = (min.x + max.x) / 2f;
            float centerY = (min.y + max.y) / 2f;

            // Try to get or create boundary layer
            int boundaryLayer = LayerMask.NameToLayer(boundaryLayerName);
            if (boundaryLayer < 0) boundaryLayer = 0; // Default layer if not found

            // Create walls
            _leftWall = CreateWall("Left Wall",
                new Vector2(min.x - colliderThickness / 2f, centerY),
                new Vector2(colliderThickness, height + colliderThickness * 2f),
                boundaryLayer);

            _rightWall = CreateWall("Right Wall",
                new Vector2(max.x + colliderThickness / 2f, centerY),
                new Vector2(colliderThickness, height + colliderThickness * 2f),
                boundaryLayer);

            _topWall = CreateWall("Top Wall",
                new Vector2(centerX, max.y + colliderThickness / 2f),
                new Vector2(width + colliderThickness * 2f, colliderThickness),
                boundaryLayer);

            _bottomWall = CreateWall("Bottom Wall",
                new Vector2(centerX, min.y - colliderThickness / 2f),
                new Vector2(width + colliderThickness * 2f, colliderThickness),
                boundaryLayer);

            Debug.Log($"[ScreenBoundaries] Generated boundaries: {min} to {max}");
        }

        private BoxCollider2D CreateWall(string name, Vector2 position, Vector2 size, int layer)
        {
            var wall = new GameObject(name);
            wall.transform.SetParent(_boundaryContainer.transform);
            wall.transform.position = position;
            wall.layer = layer;

            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;

            // Add Rigidbody2D to make it a proper physics object
            var rb = wall.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            return collider;
        }

        /// <summary>
        /// Check if a position is within screen bounds
        /// </summary>
        public bool IsInsideBounds(Vector2 position)
        {
            return ScreenBounds.Contains(position);
        }

        /// <summary>
        /// Clamp a position to stay within screen bounds
        /// </summary>
        public Vector2 ClampToBounds(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, ScreenBounds.min.x, ScreenBounds.max.x),
                Mathf.Clamp(position.y, ScreenBounds.min.y, ScreenBounds.max.y)
            );
        }

        /// <summary>
        /// Get a random position within bounds
        /// </summary>
        public Vector2 GetRandomPositionInBounds()
        {
            return new Vector2(
                Random.Range(ScreenBounds.min.x, ScreenBounds.max.x),
                Random.Range(ScreenBounds.min.y, ScreenBounds.max.y)
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector2 min, max;
            
            if (useWorldSpace)
            {
                min = worldBoundsMin;
                max = worldBoundsMax;
            }
            else
            {
                var cam = targetCamera != null ? targetCamera : Camera.main;
                if (cam == null) return;

                float camHeight = cam.orthographicSize * 2f;
                float camWidth = camHeight * cam.aspect;
                Vector3 camPos = cam.transform.position;

                min = new Vector2(camPos.x - camWidth / 2f + padding, camPos.y - camHeight / 2f + padding);
                max = new Vector2(camPos.x + camWidth / 2f - padding, camPos.y + camHeight / 2f - padding);
            }

            Gizmos.color = Color.red;
            Vector3 center = new Vector3((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0);
            Vector3 size = new Vector3(max.x - min.x, max.y - min.y, 0);
            Gizmos.DrawWireCube(center, size);

            // Draw walls
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            float w = max.x - min.x;
            float h = max.y - min.y;
            float t = colliderThickness;

            // Left
            Gizmos.DrawCube(new Vector3(min.x - t / 2f, center.y, 0), new Vector3(t, h + t * 2, 0.1f));
            // Right
            Gizmos.DrawCube(new Vector3(max.x + t / 2f, center.y, 0), new Vector3(t, h + t * 2, 0.1f));
            // Top
            Gizmos.DrawCube(new Vector3(center.x, max.y + t / 2f, 0), new Vector3(w + t * 2, t, 0.1f));
            // Bottom
            Gizmos.DrawCube(new Vector3(center.x, min.y - t / 2f, 0), new Vector3(w + t * 2, t, 0.1f));
        }
#endif
    }
}
