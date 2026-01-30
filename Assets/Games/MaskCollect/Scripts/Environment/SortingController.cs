using UnityEngine;

namespace MaskCollect.Environment
{
    /// <summary>
    /// Automatically updates SpriteRenderer sorting order based on Y position.
    /// Objects lower on screen (higher Y value in world = lower on screen) appear in front.
    /// This creates a 2.5D depth effect.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SortingController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private float sortingPrecision = 100f; // Higher = more precise sorting
        [SerializeField] private int baseSortingOrder = 0;
        [SerializeField] private float yOffset = 0f; // Offset from pivot point

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private SpriteRenderer _spriteRenderer;
        private Transform _transform;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _transform = transform;
        }

        private void Start()
        {
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            if (updateEveryFrame)
            {
                UpdateSortingOrder();
            }
        }

        /// <summary>
        /// Update sorting order based on current Y position
        /// </summary>
        public void UpdateSortingOrder()
        {
            if (_spriteRenderer == null) return;

            // Lower Y = higher sorting order (appears in front)
            // We negate Y because Unity's sorting order: higher = in front
            float yPosition = _transform.position.y + yOffset;
            int sortingOrder = baseSortingOrder - Mathf.RoundToInt(yPosition * sortingPrecision);
            
            _spriteRenderer.sortingOrder = sortingOrder;

#if UNITY_EDITOR
            if (showDebugInfo)
            {
                Debug.Log($"[SortingController] {gameObject.name}: Y={yPosition:F2}, Order={sortingOrder}");
            }
#endif
        }

        /// <summary>
        /// Set a new base sorting order
        /// </summary>
        public void SetBaseSortingOrder(int order)
        {
            baseSortingOrder = order;
            UpdateSortingOrder();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (_transform == null)
            {
                _transform = transform;
            }
            UpdateSortingOrder();
        }
#endif
    }

    /// <summary>
    /// Manages multiple SpriteRenderers for complex characters (body parts, accessories, etc.)
    /// </summary>
    public class MultiSortingController : MonoBehaviour
    {
        [System.Serializable]
        public class SpriteLayer
        {
            public SpriteRenderer spriteRenderer;
            public int orderOffset = 0; // Relative to base order
        }

        [Header("Settings")]
        [SerializeField] private bool updateEveryFrame = true;
        [SerializeField] private float sortingPrecision = 100f;
        [SerializeField] private int baseSortingOrder = 0;
        [SerializeField] private float yOffset = 0f;

        [Header("Sprite Layers")]
        [SerializeField] private SpriteLayer[] spriteLayers;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void Start()
        {
            UpdateSortingOrder();
        }

        private void LateUpdate()
        {
            if (updateEveryFrame)
            {
                UpdateSortingOrder();
            }
        }

        public void UpdateSortingOrder()
        {
            float yPosition = _transform.position.y + yOffset;
            int baseOrder = baseSortingOrder - Mathf.RoundToInt(yPosition * sortingPrecision);

            foreach (var layer in spriteLayers)
            {
                if (layer.spriteRenderer != null)
                {
                    layer.spriteRenderer.sortingOrder = baseOrder + layer.orderOffset;
                }
            }
        }

        /// <summary>
        /// Auto-find all child SpriteRenderers
        /// </summary>
        [ContextMenu("Auto-populate Sprite Layers")]
        private void AutoPopulateLayers()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            spriteLayers = new SpriteLayer[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                spriteLayers[i] = new SpriteLayer
                {
                    spriteRenderer = renderers[i],
                    orderOffset = i // Stack them in order found
                };
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
