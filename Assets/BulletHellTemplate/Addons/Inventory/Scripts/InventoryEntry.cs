using BulletHellTemplate;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents one entry in the Inventory UI, displaying basic info
    /// and providing an option to open detailed item info.
    /// </summary>
    public class InventoryEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Icon representing the item.")]
        public Image icon;

        [Tooltip("Highlight indicating selection.")]
        public Image selected;

        [Tooltip("Icon or overlay indicating the item is equipped.")]
        public GameObject equipped;

        [Tooltip("Frame background based on rarity.")]
        public Image frame;

        [Tooltip("Text to display item title.")]
        public TextMeshProUGUI title;

        [Tooltip("Text to display item level.")]
        public TextMeshProUGUI level;

        // Rarity frames
        [Header("Rarity Frames")]
        public Sprite commonFrame;
        public Sprite uncommonFrame;
        public Sprite rareFrame;
        public Sprite epicFrame;
        public Sprite legendaryFrame;

        private string uniqueItemGuid;      // Each purchased item is identified by a unique GUID
        private InventoryItem baseScriptable;

        /// <summary>
        /// Sets up the inventory entry with the provided item details.
        /// </summary>
        /// <param name="guid">Unique GUID of the purchased item instance.</param>
        /// <param name="scriptableItem">Scriptable object for item data.</param>
        /// <param name="itemLevel">Current upgrade level of this item instance.</param>
        public void Setup(string guid, InventoryItem scriptableItem, int itemLevel)
        {
            uniqueItemGuid = guid;
            baseScriptable = scriptableItem;

            if (icon != null)
                icon.sprite = scriptableItem.itemIcon;

            if (title != null)
                title.text = scriptableItem.title;

            if (level != null)
                level.text = itemLevel.ToString();

            SetFrameByRarity(scriptableItem.rarity);
        }

        /// <summary>
        /// Called when the user clicks this entry to open the detailed item info.
        /// </summary>
        public void OnClickOpenItemInfo()
        {
            UIInventoryMenu.Singleton.DeselectAllEntries();
            Select();

            if (baseScriptable != null)
            {
                // Activate and pass the uniqueItemGuid to the item info panel
                var itemInfoPanel = UIInventoryMenu.Singleton.inventoryItemInfoPrefab;
                itemInfoPanel.gameObject.SetActive(true);
                itemInfoPanel.OpenItemInfo(baseScriptable, uniqueItemGuid);
            }
        }

        /// <summary>
        /// Highlights this entry as selected.
        /// </summary>
        public void Select() => selected.gameObject.SetActive(true);

        /// <summary>
        /// Removes selection highlight.
        /// </summary>
        public void Deselect() => selected.gameObject.SetActive(false);

        /// <summary>
        /// Sets the frame sprite according to the item's rarity.
        /// </summary>
        public void SetFrameByRarity(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:
                    frame.sprite = commonFrame;
                    break;
                case Rarity.Uncommon:
                    frame.sprite = uncommonFrame;
                    break;
                case Rarity.Rare:
                    frame.sprite = rareFrame;
                    break;
                case Rarity.Epic:
                    frame.sprite = epicFrame;
                    break;
                case Rarity.Legendary:
                    frame.sprite = legendaryFrame;
                    break;
                default:
                    frame.sprite = commonFrame;
                    break;
            }
        }

        /// <summary>
        /// Returns an integer representing the rarity, for sorting.
        /// </summary>
        public int RarityValue()
        {
            return (int)baseScriptable.rarity;
        }

        /// <summary>
        /// Returns the slot name of the item, for sorting.
        /// </summary>
        public string SlotValue()
        {
            return baseScriptable.slot;
        }
    }
}
