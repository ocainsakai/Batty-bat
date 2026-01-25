using UnityEngine;
using TMPro;
using Core.Systems.CollectableSystem;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// Popup for displaying inventory contents.
    /// Can be used for viewing collected items, choosing items, etc.
    /// </summary>
    public class InventoryPopup : UIPopup
    {
        [Header("Inventory References")]
        [SerializeField] private InventoryGridUI inventoryGrid;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private CollectableInventory targetInventory;

        [Header("Detail Panel")]
        [SerializeField] private GameObject detailPanel;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private UnityEngine.UI.Image detailIconImage;

        private System.Action<string> _onItemSelected;

        protected override void Awake()
        {
            base.Awake();

            if (inventoryGrid != null)
            {
                inventoryGrid.OnSlotSelected += HandleSlotSelected;
                inventoryGrid.OnItemClicked += HandleItemClicked;
            }

            // Hide detail panel initially
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        protected override void OnAfterShow()
        {
            base.OnAfterShow();
            RefreshInventory();
        }

        /// <summary>
        /// Set the inventory to display
        /// </summary>
        public void SetInventory(CollectableInventory inventory)
        {
            targetInventory = inventory;
            
            if (inventoryGrid != null)
            {
                // Use reflection or direct assignment to set targetInventory
                // For now, just refresh
                RefreshInventory();
            }
        }

        /// <summary>
        /// Set callback for item selection
        /// </summary>
        public void SetOnItemSelected(System.Action<string> callback)
        {
            _onItemSelected = callback;
        }

        /// <summary>
        /// Refresh inventory display
        /// </summary>
        public void RefreshInventory()
        {
            if (targetInventory == null || inventoryGrid == null) return;

            inventoryGrid.ClearAllSlots();
            
            var items = targetInventory.GetAll();
            int slotIndex = 0;

            foreach (var kvp in items)
            {
                var definition = targetInventory.GetDefinition(kvp.Key);
                if (definition != null)
                {
                    inventoryGrid.SetSlotItem(slotIndex, definition, kvp.Value);
                    slotIndex++;
                }
            }

            UpdateItemCount();
        }

        private void UpdateItemCount()
        {
            if (itemCountText != null && targetInventory != null)
            {
                itemCountText.text = $"{inventoryGrid.UsedSlotCount}/{inventoryGrid.SlotCount}";
            }
        }

        private void HandleSlotSelected(InventorySlotUI slot)
        {
            if (detailPanel == null) return;

            if (slot == null || slot.IsEmpty)
            {
                detailPanel.SetActive(false);
                return;
            }

            // Show detail panel
            detailPanel.SetActive(true);

            // Update detail info
            if (targetInventory != null)
            {
                var definition = targetInventory.GetDefinition(slot.ItemId);
                if (definition != null)
                {
                    if (detailNameText != null)
                        detailNameText.text = definition.CollectibleName;

                    if (detailIconImage != null)
                        detailIconImage.sprite = definition.CollectibleSprite;

                    // Description - you might want to add a description field to CollectibleDefinition
                    if (detailDescriptionText != null)
                        detailDescriptionText.text = $"Count: {slot.Count}";
                }
            }
        }

        private void HandleItemClicked(string itemId, int count)
        {
            _onItemSelected?.Invoke(itemId);
        }

        /// <summary>
        /// Set title text
        /// </summary>
        public void SetTitle(string title)
        {
            if (titleText != null)
            {
                titleText.text = title;
            }
        }
    }
}
