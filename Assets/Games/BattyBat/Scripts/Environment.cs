using UnityEngine;
using System.Threading;           // Dùng cho CancellationToken
using Cysharp.Threading.Tasks;    // Dùng cho UniTask

namespace Games.BattyBat
{
    public enum VisionMode
    {
        BlindMode,
        EchoMode,
    }

    public class Environment : MonoBehaviour
    {
        public static Environment Instance { get; private set; }

        [SerializeField] private Transform _startPoint;

        [Header("Settings")]
        [SerializeField] private SpriteRenderer darkness;
        [SerializeField] private float _fadeDuration = 0.5f;

        private VisionMode _visionMode;
        private CancellationTokenSource _fadeCts;

        public VisionMode VisionMode
        {
            get => _visionMode;
            set
            {
                if (_visionMode != value)
                {
                    _visionMode = value;
                    ChangeVisionMode(value);
                }
            }
        }

        public Transform StartPoint => _startPoint;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            var color = darkness.color;
            color.a = 1f;
            darkness.color = color;
            darkness.gameObject.SetActive(true);

            _visionMode = VisionMode.BlindMode;
        }

        private void OnDestroy()
        {
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
        }

        private void ChangeVisionMode(VisionMode visionMode)
        {
            if (_fadeCts != null)
            {
                _fadeCts.Cancel();
                _fadeCts.Dispose();
            }

            _fadeCts = new CancellationTokenSource();
            float targetAlpha = (visionMode == VisionMode.BlindMode) ? 1f : 0f;
            FadeDarknessAsync(targetAlpha, _fadeCts.Token).Forget();
        }

        private async UniTaskVoid FadeDarknessAsync(float targetAlpha, CancellationToken token)
        {
            if (targetAlpha > 0) darkness.gameObject.SetActive(true);

            Color startColor = darkness.color;
            float startAlpha = startColor.a;
            float time = 0;

            while (time < _fadeDuration)
            {
                time += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / _fadeDuration);

                startColor.a = newAlpha;
                darkness.color = startColor;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }

            startColor.a = targetAlpha;
            darkness.color = startColor;
            if (targetAlpha == 0) darkness.gameObject.SetActive(false);
        }
    }
}