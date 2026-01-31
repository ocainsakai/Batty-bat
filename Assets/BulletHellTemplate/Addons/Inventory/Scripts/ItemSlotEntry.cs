using BulletHellTemplate;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a UI entry for a single slot (e.g. Weapon, Armor, or a Rune slot).
    /// Displays the slot name and the icon/rarity of the equipped item/rune if any.
    /// Calls UIItemsApply to open the appropriate popup.
    /// </summary>
    public class ItemSlotEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Displays the slot name (e.g., 'WeaponSlot' or 'RuneSlot').")]
        public TextMeshProUGUI slotNameText;

        [Tooltip("Displays the icon of the equipped item or rune.")]
        public Image itemIcon;

        [Tooltip("Displays the rarity sprite overlay (optional).")]
        public Image raritySprite;

        [Tooltip("Button to open the popup and select an item or rune.")]
        public Button slotButton;

        [Tooltip("Visual indicator that this slot is currently selected.")]
        public GameObject selected;

        [Tooltip("Default icon to use when the slot is empty.")]
        public Sprite defaultEmptySlotIcon;

        [Header("Sprites for Rarity Levels")]
        [Tooltip("Sprite used to indicate Common rarity.")]
        public Sprite commonRaritySprite;

        [Tooltip("Sprite used to indicate Uncommon rarity.")]
        public Sprite uncommonRaritySprite;

        [Tooltip("Sprite used to indicate Rare rarity.")]
        public Sprite rareRaritySprite;

        [Tooltip("Sprite used to indicate Epic rarity.")]
        public Sprite epicRaritySprite;

        [Tooltip("Sprite used to indicate Legendary rarity.")]
        public Sprite legendaryRaritySprite;

        private string slotName;
        private UIItemsApply itemsApply;
        private bool isRuneSlot;

        /// <summary>
        /// Initializes the slot with its name, configures the button to open the correct popup,
        /// and sets whether this is a rune slot or a normal item slot.
        /// </summary>
        /// <param name="slot">The slot name (e.g., 'WeaponSlot' or 'DefenseRune').</param>
        /// <param name="itemsApplyMenu">Reference to the UIItemsApply manager.</param>
        /// <param name="isRune">True if this is a rune slot, false if it's a normal item slot.</param>
        public void Setup(string slot, UIItemsApply itemsApplyMenu, bool isRune)
        {
            slotName = slot;
            isRuneSlot = isRune;
            itemsApply = itemsApplyMenu;

            if (slotNameText != null)
                slotNameText.text = slot;

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(OnClickOpenSlotPopup);
            }

            // Initialize as empty
            SetItemIcon(null);

            // Ensure the 'selected' indicator is hidden by default
            if (selected != null)
                selected.SetActive(false);
        }

        /// <summary>
        /// Sets the icon and rarity sprite for the item/rune currently equipped in this slot.
        /// Pass null for the icon to clear the slot visually.
        /// </summary>
        /// <param name="icon">The item/rune icon sprite or null.</param>
        /// <param name="rarity">Optional rarity enum of the item/rune.</param>
        public void SetItemIcon(Sprite icon, Rarity? rarity = null)
        {
            if (icon == null)
            {
                // Empty slot
                if (itemIcon != null)
                {
                    itemIcon.sprite = defaultEmptySlotIcon;
                    itemIcon.color = new Color(1, 1, 1, 0.5f);
                }
                if (raritySprite != null)
                {
                    raritySprite.sprite = null;
                    raritySprite.color = new Color(1, 1, 1, 0);
                }
            }
            else
            {
                if (itemIcon != null)
                {
                    itemIcon.sprite = icon;
                    itemIcon.color = Color.white;
                }
                if (rarity.HasValue && raritySprite != null)
                {
                    SetRaritySprite(rarity.Value);
                }
            }
        }

        /// <summary>
        /// Gets the name of this slot.
        /// </summary>
        public string GetSlotName()
        {
            return slotName;
        }

        /// <summary>
        /// Called when the user clicks the slot button. Opens the appropriate popup (item or rune)
        /// and sets the 'selected' indicator to true.
        /// </summary>
        private void OnClickOpenSlotPopup()
        {
            if (selected != null)
                selected.SetActive(true);

            itemsApply.OpenSlotPopup(slotName, isRuneSlot, this);
        }

        /// <summary>
        /// Deactivates the 'selected' indicator when the popup is closed.
        /// </summary>
        public void DeselectSlot()
        {
            if (selected != null)
                selected.SetActive(false);
        }

        /// <summary>
        /// Assigns the appropriate rarity overlay based on the specified rarity.
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
