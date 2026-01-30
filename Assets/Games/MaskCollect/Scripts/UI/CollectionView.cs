using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Displays the mask collection grid.
    /// Shows owned masks in full color and unowned masks as silhouettes.
    /// </summary>
    public class CollectionView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MaskDatabase maskDatabase;
        [SerializeField] private InventoryManager inventory;

        [Header("UI Elements")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private GameObject maskSlotPrefab;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Slider progressBar;

        [Header("Visual Settings")]
        [SerializeField] private Color ownedColor = Color.white;
        [SerializeField] private Color unownedColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private bool useSilhouetteForUnowned = true;

        private List<MaskSlotUI> _slots = new();

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnMaskCollected += HandleMaskCollected;
                inventory.OnInventoryLoaded += RefreshView;
            }

            RefreshView();
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnMaskCollected -= HandleMaskCollected;
                inventory.OnInventoryLoaded -= RefreshView;
            }
        }

        private void Start()
        {
            if (inventory == null)
            {
                inventory = InventoryManager.Instance;
            }
            
            InitializeGrid();
            RefreshView();
        }

        /// <summary>
        /// Create all mask slots based on database
        /// </summary>
        private void InitializeGrid()
        {
            if (maskDatabase == null || maskSlotPrefab == null || gridContainer == null)
            {
                Debug.LogError("[CollectionView] Missing references!");
                return;
            }

            // Clear existing slots
            foreach (var slot in _slots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _slots.Clear();

            // Create slots for each mask
            foreach (var mask in maskDatabase.AllMasks)
            {
                var slotObj = Instantiate(maskSlotPrefab, gridContainer);
                var slot = slotObj.GetComponent<MaskSlotUI>();
                
                if (slot != null)
                {
                    slot.Initialize(mask, useSilhouetteForUnowned, ownedColor, unownedColor);
                    _slots.Add(slot);
                }
            }

            Debug.Log($"[CollectionView] Created {_slots.Count} mask slots");
        }

        /// <summary>
        /// Refresh all slots to show current ownership status
        /// </summary>
        public void RefreshView()
        {
            if (inventory == null) return;

            foreach (var slot in _slots)
            {
                if (slot != null && slot.MaskData != null)
                {
                    bool isOwned = inventory.HasMask(slot.MaskData);
                    slot.SetOwned(isOwned);
                }
            }

            UpdateProgressDisplay();
        }

        private void UpdateProgressDisplay()
        {
            if (inventory == null) return;

            int collected = inventory.CollectedCount;
            int total = inventory.TotalMaskCount;
            float progress = inventory.GetCollectionProgress();

            if (progressText != null)
            {
                progressText.text = $"{collected} / {total}";
            }

            if (progressBar != null)
            {
                progressBar.value = progress;
            }
        }

        private void HandleMaskCollected(MaskData mask)
        {
            // Find and update the specific slot
            foreach (var slot in _slots)
            {
                if (slot != null && slot.MaskData == mask)
                {
                    slot.SetOwned(true);
                    slot.PlayUnlockAnimation();
                    break;
                }
            }

            UpdateProgressDisplay();
        }

        /// <summary>
        /// Show the collection view (for UI navigation)
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshView();
        }

        /// <summary>
        /// Hide the collection view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}
