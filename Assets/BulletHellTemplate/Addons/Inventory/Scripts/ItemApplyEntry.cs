using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a UI entry in the item/rune popup. Shows item details and equips or unequips when clicked.
    /// Differentiates between normal items and runes using a boolean flag.
    /// </summary>
    public class ItemApplyEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Displays the icon of the item or rune.")]
        public Image itemIcon;

        [Tooltip("Displays the name of the item or rune.")]
        public TextMeshProUGUI itemNameText;

        [Tooltip("Displays the rarity overlay sprite.")]
        public Image raritySprite;

        [Tooltip("Button used to equip or unequip this item/rune.")]
        public Button applyButton;

        [Tooltip("Indicator to show if the item/rune is already equipped.")]
        public Image equippedIndicator;

        [Tooltip("Displays the upgrade level of the item or rune.")]
        public TextMeshProUGUI itemLevelText;

        [Header("Sprites for Rarity Levels")]
        [Tooltip("Sprite for Common rarity.")]
        public Sprite commonRaritySprite;

        [Tooltip("Sprite for Uncommon rarity.")]
        public Sprite uncommonRaritySprite;

        [Tooltip("Sprite for Rare rarity.")]
        public Sprite rareRaritySprite;

        [Tooltip("Sprite for Epic rarity.")]
        public Sprite epicRaritySprite;

        [Tooltip("Sprite for Legendary rarity.")]
        public Sprite legendaryRaritySprite;

        private InventoryItem baseItem;
        private string uniqueItemGuid;
        private string slotName;
        private UIItemsApply itemsApply;
        private bool isRune;

        /// <summary>
        /// Initializes the entry with the given InventoryItem, purchased GUID, slot name, 
        /// reference to UIItemsApply, and a flag indicating if it's a rune.
        /// </summary>
        public void Setup(InventoryItem scriptableItem, string guid, string slot, UIItemsApply itemsApplyMenu, bool isRuneType)
        {
            baseItem = scriptableItem;
            uniqueItemGuid = guid;
            slotName = slot;
            itemsApply = itemsApplyMenu;
            isRune = isRuneType;

            if (itemIcon != null)
                itemIcon.sprite = scriptableItem.itemIcon;

            if (itemNameText != null)
                itemNameText.text = scriptableItem.title;

            SetRaritySprite(scriptableItem.rarity);

            if (applyButton != null)
            {
                applyButton.onClick.RemoveAllListeners();
                applyButton.onClick.AddListener(OnClickApply);
            }

            // We removed the selectedIndicator logic as requested

            // Display item level if item is found in MonetizationManager
            int lvl = PlayerSave.GetItemUpgradeLevel(guid);
            if (itemLevelText != null)
                itemLevelText.text = $"Lv. {lvl}";
        }

        /// <summary>
        /// Shows or hides the 'equipped' indicator.
        /// </summary>
        public void ShowAsEquipped(bool isEquipped)
        {
            if (equippedIndicator != null)
                equippedIndicator.gameObject.SetActive(isEquipped);
        }

        /// <summary>
        /// Called when the user clicks the apply button.
        /// Equips or unequips the item/rune using UIItemsApply.
        /// </summary>
        private void OnClickApply()
        {
            itemsApply.ApplyItemToSlot(uniqueItemGuid, slotName, isRune);
        }

        /// <summary>
        /// Sets the rarity overlay sprite based on the given rarity enum.
        /// </summary>
        private void SetRaritySprite(Rarity rarity)
        {
            if (raritySprite == null) return;

            switch (rarity)
            {
                case Rarity.Common:
                    raritySprite.sprite = commonRaritySprite;
                    break;
                case Rarity.Uncommon:
                    raritySprite.sprite = uncommonRaritySprite;
                    break;
                case Rarity.Rare:
                    raritySprite.sprite = rareRaritySprite;
                    break;
                case Rarity.Epic:
                    raritySprite.sprite = epicRaritySprite;
                    break;
                case Rarity.Legendary:
                    raritySprite.sprite = legendaryRaritySprite;
                    break;
                default:
                    raritySprite.sprite = commonRaritySprite;
                    break;
            }
            raritySprite.color = Color.white;
        }
    }
}
