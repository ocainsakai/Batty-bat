using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Individual mask slot in the collection grid.
    /// Shows mask sprite with owned/unowned visual state.
    /// </summary>
    public class MaskSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image maskImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private TextMeshProUGUI maskNameText;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private GameObject lockIcon;

        [Header("Rarity Colors")]
        [SerializeField] private Color commonColor = Color.gray;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color legendaryColor = Color.yellow;

        private MaskData _maskData;
        private bool _isOwned;
        private bool _useSilhouette;
        private Color _ownedColor;
        private Color _unownedColor;

        public MaskData MaskData => _maskData;
        public bool IsOwned => _isOwned;

        /// <summary>
        /// Initialize the slot with mask data
        /// </summary>
        public void Initialize(MaskData mask, bool useSilhouette, Color ownedColor, Color unownedColor)
        {
            _maskData = mask;
            _useSilhouette = useSilhouette;
            _ownedColor = ownedColor;
            _unownedColor = unownedColor;

            UpdateRarityBorder();
            SetOwned(false); // Default to unowned

            if (maskNameText != null)
            {
                maskNameText.text = mask.MaskName;
            }
        }

        /// <summary>
        /// Set the ownership state and update visuals
        /// </summary>
        public void SetOwned(bool isOwned)
        {
            _isOwned = isOwned;

            if (maskImage != null && _maskData != null)
            {
                if (isOwned)
                {
                    maskImage.sprite = _maskData.MaskSprite;
                    maskImage.color = _ownedColor;
                }
                else
                {
                    // Tint the normal sprite for unowned
                    maskImage.sprite = _maskData.SilhouetteSprite ?? _maskData.MaskSprite;
                    maskImage.color = _unownedColor;
                }
            }

            if (lockIcon != null)
            {
                lockIcon.SetActive(!isOwned);
            }

            if (maskNameText != null)
            {
                maskNameText.gameObject.SetActive(isOwned);
            }

            // Hide new badge by default
            if (newBadge != null)
            {
                newBadge.SetActive(false);
            }
        }

        private void UpdateRarityBorder()
        {
            if (rarityBorder == null || _maskData == null) return;

            rarityBorder.color = _maskData.Rarity switch
            {
                MaskRarity.Common => commonColor,
                MaskRarity.Uncommon => uncommonColor,
                MaskRarity.Rare => rareColor,
                MaskRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }

        /// <summary>
        /// Play unlock animation when mask is newly collected
        /// </summary>
        public void PlayUnlockAnimation()
        {
            // Show new badge temporarily
            if (newBadge != null)
            {
                newBadge.SetActive(true);
            }

            // Scale punch animation
            DOTween.Kill(gameObject);
            transform.localScale = Vector3.one * 0.5f;
            transform.DOScale(Vector3.one * 1.2f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    transform.DOScale(Vector3.one, 0.2f)
                        .SetEase(Ease.InOutQuad);
                });

            // Optional: Shake or glow effect
            if (maskImage != null)
            {
                // Flash white
                maskImage.color = Color.white;
                maskImage.DOColor(_ownedColor, 0.5f);
            }
        }

        /// <summary>
        /// Show mask details (for click interaction)
        /// </summary>
        public void OnClick()
        {
            if (!_isOwned)
            {
                // Could show hint about how to get this mask
                Debug.Log($"[MaskSlotUI] {_maskData.MaskName} is not yet collected");
                return;
            }

            // Could open detail view
            Debug.Log($"[MaskSlotUI] Viewing: {_maskData.MaskName} - {_maskData.Description}");
        }
    }
}