using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// UI component representing a single inventory slot.
    /// Displays item icon, count, and handles selection/interaction.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectionFrame;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Button button;

        [Header("Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(1f, 0.9f, 0.5f);
        [SerializeField] private Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.15f;
        [SerializeField] private Ease animationEase = Ease.OutBack;

        private string _itemId;
        private int _count;
        private bool _isEmpty = true;
        private bool _isSelected = false;
        private Tween _currentTween;

        public string ItemId => _itemId;
        public int Count => _count;
        public bool IsEmpty => _isEmpty;
        public bool IsSelected => _isSelected;

        public System.Action<InventorySlotUI> OnSlotClicked;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(HandleClick);
            }

            SetEmpty();
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
        }

        /// <summary>
        /// Set slot data
        /// </summary>
        public void SetItem(string itemId, Sprite icon, int count, string displayName = null)
        {
            _itemId = itemId;
            _count = count;
            _isEmpty = false;

            // Update visuals
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
                iconImage.color = Color.white;
            }

            if (countText != null)
            {
                countText.text = count > 1 ? count.ToString() : "";
                countText.enabled = count > 1;
            }

            if (nameText != null)
            {
                nameText.text = displayName ?? "";
                nameText.enabled = !string.IsNullOrEmpty(displayName);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }

            if (emptyState != null)
            {
                emptyState.SetActive(false);
            }
        }

        /// <summary>
        /// Update count only
        /// </summary>
        public void UpdateCount(int newCount)
        {
            _count = newCount;

            if (countText != null)
            {
                countText.text = newCount > 1 ? newCount.ToString() : "";
                countText.enabled = newCount > 1;
            }

            if (newCount <= 0)
            {
                SetEmpty();
            }
        }

        /// <summary>
        /// Set slot as empty
        /// </summary>
        public void SetEmpty()
        {
            _itemId = null;
            _count = 0;
            _isEmpty = true;
            _isSelected = false;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (countText != null)
            {
                countText.text = "";
                countText.enabled = false;
            }

            if (nameText != null)
            {
                nameText.text = "";
                nameText.enabled = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = emptyColor;
            }

            if (selectionFrame != null)
            {
                selectionFrame.enabled = false;
            }

            if (emptyState != null)
            {
                emptyState.SetActive(true);
            }
        }

        /// <summary>
        /// Set selected state
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;

            if (selectionFrame != null)
            {
                selectionFrame.enabled = selected;
            }

            if (backgroundImage != null && !_isEmpty)
            {
                backgroundImage.DOColor(selected ? selectedColor : normalColor, animationDuration);
            }
        }

        private void HandleClick()
        {
            OnSlotClicked?.Invoke(this);
        }

        /// <summary>
        /// Play add animation
        /// </summary>
        public void PlayAddAnimation()
        {
            if (iconImage == null) return;

            _currentTween?.Kill();
            
            var rect = iconImage.rectTransform;
            rect.localScale = Vector3.one;
            
            _currentTween = DOTween.Sequence()
                .Append(rect.DOScale(1.2f, animationDuration * 0.6f).SetEase(Ease.OutQuad))
                .Append(rect.DOScale(1f, animationDuration * 0.4f).SetEase(Ease.InQuad));
        }

        /// <summary>
        /// Play remove animation
        /// </summary>
        public void PlayRemoveAnimation()
        {
            if (iconImage == null) return;

            _currentTween?.Kill();
            
            var rect = iconImage.rectTransform;
            
            _currentTween = DOTween.Sequence()
                .Append(rect.DOScale(0.8f, animationDuration * 0.4f).SetEase(Ease.InQuad))
                .Append(rect.DOScale(1f, animationDuration * 0.6f).SetEase(Ease.OutQuad));
        }

        /// <summary>
        /// Play punch animation (for emphasis)
        /// </summary>
        public void PlayPunchAnimation()
        {
            if (iconImage == null) return;

            _currentTween?.Kill();
            
            var rect = iconImage.rectTransform;
            _currentTween = rect.DOPunchScale(Vector3.one * 0.2f, animationDuration, 1, 0.5f);
        }
    }
}
