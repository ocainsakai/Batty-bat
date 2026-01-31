using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using DentedPixel;

namespace BulletHellTemplate.VFX
{
    [AddComponentMenu("Bullet Hell Template/VFX/Damage Popup")]
    public sealed class DamagePopup : MonoBehaviour
    {
        static DamagePopup()
        {
            // Auto‑clean when scene changes to avoid MissingReferenceException
            SceneManager.activeSceneChanged += (_, __) =>
            {
                Pool.Clear();
                _root = null;
            };
        }
    
        /* ───── Inspector ───── */
        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Timing (seconds)")]
        [SerializeField, Range(0.1f, 3f)] private float lifetime = 0.8f;
        [SerializeField] private float riseDistance = 1.2f;

        [Header("Critical Hit Style")]
        [SerializeField] private float criticalScaleBoost = 1.3f;
        [SerializeField] private Color criticalColor = new(1f, 0.4f, 0f);

        [Header("Reference")]
        [SerializeField] private TextMeshProUGUI tmp;

        /* ───── Static Pool ───── */
        private static readonly Stack<DamagePopup> Pool = new(32);
        private static Transform _root; // recreated per scene          // parent folder (active)
        private static GameObject _templatePrefab;

        /* ───── Instance Cache ───── */
        private RectTransform _rect;
        private Color _baseColor;
        private Vector3 _originalScale;
        private Vector3 _initialWorldPos;
        private readonly LTDescr[] _tween = new LTDescr[3];

        /* ───── Public API ───── */
        public static void Configure(GameObject prefab, int warmup = 32)
        {
            _templatePrefab = prefab;
            WarmUp(warmup);
        }

        public static void Show(int amount, Vector3 worldPos, bool critical = false)
        {
            Camera cam = Camera.main;
            if (!cam) return;
            EnsureRoot();
            DamagePopup pop = null;
            // Discard destroyed refs
            while (Pool.Count > 0 && pop == null)
            {
                pop = Pool.Pop();
            }
            if (pop == null) pop = CreateInstance();
            pop.gameObject.SetActive(true);
            pop.Play(amount, worldPos, critical);
        }

        /* ───── Mono ───── */
        private void Awake() => Cache();
        private void OnValidate() { if (!Application.isPlaying) Cache(); }

        private void Cache()
        {
            _rect ??= GetComponent<RectTransform>();
            tmp ??= GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) _baseColor = tmp.color;
            _originalScale = _rect ? _rect.localScale : Vector3.one;
        }

        /* ───── Root & Pool ───── */
        private static void EnsureRoot()
        {
            if (_root) return;
            _root = new GameObject("__DamagePopupPool").transform;
        }

        private static DamagePopup CreateInstance()
        {
            GameObject go = _templatePrefab ? Object.Instantiate(_templatePrefab, _root) : BuildDefault();
            return go.TryGetComponent(out DamagePopup dp) ? dp : go.AddComponent<DamagePopup>();
        }

        private static GameObject BuildDefault()
        {
            GameObject go = new("DamagePopup", typeof(RectTransform));
            go.transform.SetParent(_root, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 36;
            text.raycastTarget = false;
            return go;
        }

        private static void WarmUp(int n)
        {
            EnsureRoot();
            while (Pool.Count < n)
            {
                DamagePopup p = CreateInstance();
                p.gameObject.SetActive(false);
                Pool.Push(p);
            }
        }

        /* ───── Animation ───── */
        private void Play(int amount, Vector3 worldPos, bool critical)
        {
            if (!_rect || tmp == null) { Return(); return; }

            // Position & appearance
            _rect.position = worldPos;
            _initialWorldPos = worldPos;
            tmp.SetText("- {0}", amount);
            tmp.color = critical ? criticalColor : _baseColor;

            LeanTween.cancel(gameObject);
            float scaleMultiplier = critical ? criticalScaleBoost : 1f;

            // Scale punch multiplied by original passEntryPrefab scale
            _tween[0] = LeanTween.value(gameObject, 0f, scaleMultiplier, lifetime * 0.25f)
                             .setEase(scaleCurve)
                             .setOnUpdate(v => _rect.localScale = _originalScale * v);

            // Rise motion (world‑space)
            _tween[1] = LeanTween.value(gameObject, 0f, riseDistance, lifetime)
                             .setOnUpdate(v => _rect.position = _initialWorldPos + Vector3.up * v);

            // Fade
            _tween[2] = LeanTween.value(gameObject, 1f, 0f, lifetime)
                             .setEase(alphaCurve)
                             .setOnUpdate(a => tmp.alpha = a)
                             .setOnComplete(Return);
        }

        private void Return()
        {
            gameObject.SetActive(false);
            if (_rect) _rect.localScale = _originalScale;
            if (tmp) tmp.alpha = 1f;
            transform.SetParent(_root, false);
            Pool.Push(this);
        }
    }
}
