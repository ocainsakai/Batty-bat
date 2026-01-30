using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace MaskCollect.VFX
{
    /// <summary>
    /// Visual effect for mask flying into the player's inventory.
    /// </summary>
    public class MaskFlyToInventory : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float flyDuration = 1f;
        [SerializeField] private float startScale = 1f;
        [SerializeField] private float endScale = 0.3f;
        [SerializeField] private float arcHeight = 2f;
        [SerializeField] private Ease flyEase = Ease.InOutQuad;

        [Header("Target")]
        [SerializeField] private RectTransform inventoryTarget; // UI position to fly to

        private SpriteRenderer _spriteRenderer;
        private Transform _targetTransform;
        private Camera _mainCamera;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null)
            {
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Initialize and play the fly animation
        /// </summary>
        public async UniTask PlayFlyAnimation(Sprite maskSprite, Vector3 startPosition, RectTransform targetUI)
        {
            transform.position = startPosition;
            transform.localScale = Vector3.one * startScale;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = maskSprite;
                _spriteRenderer.sortingOrder = 100; // Above everything
            }

            inventoryTarget = targetUI;

            // Calculate target world position from UI
            Vector3 targetWorldPos;
            if (targetUI != null && _mainCamera != null)
            {
                // Convert UI position to world position
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, targetUI.position);
                targetWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            }
            else
            {
                // Default: fly to top-right corner
                targetWorldPos = _mainCamera != null 
                    ? _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width - 100, Screen.height - 100, 10f))
                    : startPosition + Vector3.up * 5f;
            }

            // Create arc path
            Vector3[] path = CreateArcPath(startPosition, targetWorldPos);

            // Animate
            Sequence sequence = DOTween.Sequence();

            // Fly along path
            sequence.Append(transform.DOPath(path, flyDuration, PathType.CatmullRom)
                .SetEase(flyEase));

            // Scale down during flight
            sequence.Join(transform.DOScale(Vector3.one * endScale, flyDuration)
                .SetEase(Ease.InQuad));

            // Spin during flight
            sequence.Join(transform.DORotate(new Vector3(0, 0, 360f), flyDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear));

            // Fade out at end
            if (_spriteRenderer != null)
            {
                sequence.Join(_spriteRenderer.DOFade(0f, flyDuration * 0.3f)
                    .SetDelay(flyDuration * 0.7f));
            }

            await sequence.AsyncWaitForCompletion();

            Destroy(gameObject);
        }

        private Vector3[] CreateArcPath(Vector3 start, Vector3 end)
        {
            Vector3 midPoint = (start + end) / 2f;
            midPoint.y += arcHeight;

            // Create smooth arc with 5 points
            return new Vector3[]
            {
                start,
                Vector3.Lerp(start, midPoint, 0.5f),
                midPoint,
                Vector3.Lerp(midPoint, end, 0.5f),
                end
            };
        }

        /// <summary>
        /// Spawn and play mask fly effect
        /// </summary>
        public static async UniTask SpawnAndPlay(Sprite maskSprite, Vector3 startPosition, RectTransform targetUI)
        {
            var go = new GameObject("MaskFlyEffect");
            var effect = go.AddComponent<MaskFlyToInventory>();
            await effect.PlayFlyAnimation(maskSprite, startPosition, targetUI);
        }
    }
}
