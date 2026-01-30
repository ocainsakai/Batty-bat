using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MaskCollect.Data;

namespace MaskCollect.UI
{
    /// <summary>
    /// Individual slot in the collection book grid.
    /// </summary>
    public class CollectionSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button button;
        [SerializeField] private Image maskImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Image rarityGlow;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject newBadge;
        [SerializeField] private TextMeshProUGUI nameText;

        private MaskData _maskData;
        private bool _isOwned;
        private Color _rarityColor;
        private Color _lockedColor;

        public MaskData MaskData => _maskData;
        public bool IsOwned => _isOwned;

        public event Action<MaskData, bool> OnSlotClicked;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }
        }

        public void Initialize(MaskData mask, bool isOwned, Color rarityColor, Color lockedColor)
        {
            _maskData = mask;
            _isOwned = isOwned;
            _rarityColor = rarityColor;
            _lockedColor = lockedColor;

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_maskData == null) return;

            // Set mask image
            if (maskImage != null)
            {
                maskImage.sprite = _isOwned ? _maskData.MaskSprite : (_maskData.SilhouetteSprite ?? _maskData.MaskSprite);
                maskImage.color = _isOwned ? Color.white : _lockedColor;
            }

            // Set border based on rarity
            if (borderImage != null)
            {
                borderImage.color = _isOwned ? _rarityColor : Color.gray;
            }

            // Rarity glow effect
            if (rarityGlow != null)
            {
                rarityGlow.gameObject.SetActive(_isOwned);
                if (_isOwned)
                {
                    rarityGlow.color = new Color(_rarityColor.r, _rarityColor.g, _rarityColor.b, 0.3f);
                }
            }

            // Lock icon
            if (lockIcon != null)
            {
                lockIcon.SetActive(!_isOwned);
            }

            // Name text
            if (nameText != null)
            {
                nameText.text = _isOwned ? _maskData.MaskName : "???";
                nameText.color = _isOwned ? Color.white : Color.gray;
            }

            // Hide new badge by default
            if (newBadge != null)
            {
                newBadge.SetActive(false);
            }
        }

        private void HandleClick()
        {
            // Play click animation
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1);

            OnSlotClicked?.Invoke(_maskData, _isOwned);
        }

        /// <summary>
        /// Call when player newly acquires this mask
        /// </summary>
        public void SetNewlyObtained()
        {
            _isOwned = true;
            UpdateVisuals();

            // Play unlock animation
            PlayUnlockAnimation();

            // Show new badge
            if (newBadge != null)
            {
                newBadge.SetActive(true);
            }
        }

        private void PlayUnlockAnimation()
        {
            // Scale pop
            transform.DOPunchScale(Vector3.one * 0.3f, 0.5f, 2);

            // Fade in color
            if (maskImage != null)
            {
                maskImage.color = _lockedColor;
                maskImage.DOColor(Color.white, 0.5f);
            }

            // Lock icon shrink away
            if (lockIcon != null)
            {
                lockIcon.transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => lockIcon.SetActive(false));
            }

            // Border glow
            if (borderImage != null)
            {
                borderImage.DOColor(_rarityColor, 0.3f);
            }
        }

        public void ClearNewBadge()
        {
            if (newBadge != null)
            {
                newBadge.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            OnSlotClicked = null;
        }
    }
}
