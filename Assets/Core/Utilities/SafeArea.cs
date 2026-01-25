using UnityEngine;
using UnityEngine.Events;

namespace Core.Utilities
{
    /// <summary>
    /// Adjusts RectTransform to fit within the device's safe area.
    /// Handles notch (tai thỏ), rounded corners (bo góc), and other screen cutouts.
    /// Attach to any UI element that needs to respect safe area boundaries.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [ExecuteAlways] // Run in Edit mode too
    public class SafeArea : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Which edges to apply safe area margins")]
        [SerializeField] private bool applyTop = true;
        [SerializeField] private bool applyBottom = true;
        [SerializeField] private bool applyLeft = true;
        [SerializeField] private bool applyRight = true;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

#if UNITY_EDITOR
        [Header("Editor Simulation")]
        [Tooltip("Enable to simulate device notch in Editor")]
        [SerializeField] private bool simulateInEditor = true;
        [SerializeField] private SimulatedDevice simulatedDevice = SimulatedDevice.iPhoneX;
        
        public enum SimulatedDevice
        {
            None,              // No simulation (use actual Screen.safeArea)
            iPhoneX,           // Top notch + bottom home indicator
            iPhone14Pro,       // Dynamic Island
            iPhoneSE,          // No notch (old style)
            AndroidNotch,      // Top notch
            AndroidPunch,      // Punch hole camera
            iPad,              // Rounded corners only
            Custom             // Use custom values below
        }
        
        [Header("Custom Simulation Values (pixels)")]
        [SerializeField] private float customTop = 44;
        [SerializeField] private float customBottom = 34;
        [SerializeField] private float customLeft = 0;
        [SerializeField] private float customRight = 0;
#endif

        [Header("Events")]
        public UnityEvent<Rect> OnSafeAreaChanged;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            ApplySafeArea();
        }

        private void Update()
        {
            // Check if safe area or screen has changed
            if (HasScreenChanged())
            {
                ApplySafeArea();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Apply in editor when values change
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            
            ApplySafeArea();
        }
#endif

        private bool HasScreenChanged()
        {
            Rect safeArea = GetCurrentSafeArea();
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            ScreenOrientation orientation = Screen.orientation;

            bool changed = safeArea != _lastSafeArea ||
                          screenSize != _lastScreenSize ||
                          orientation != _lastOrientation;

            if (changed)
            {
                _lastSafeArea = safeArea;
                _lastScreenSize = screenSize;
                _lastOrientation = orientation;
            }

            return changed;
        }

        /// <summary>
        /// Get the current safe area (real or simulated)
        /// </summary>
        private Rect GetCurrentSafeArea()
        {
#if UNITY_EDITOR
            if (simulateInEditor && simulatedDevice != SimulatedDevice.None)
            {
                return GetSimulatedSafeArea();
            }
#endif
            return Screen.safeArea;
        }

        /// <summary>
        /// Apply safe area to the RectTransform
        /// </summary>
        public void ApplySafeArea()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (_rectTransform == null) return;

            Rect safeArea = GetCurrentSafeArea();
            int screenWidth = Screen.width > 0 ? Screen.width : 1;
            int screenHeight = Screen.height > 0 ? Screen.height : 1;

            if (debugMode)
            {
                Debug.Log($"[SafeArea] Screen: {screenWidth}x{screenHeight}");
                Debug.Log($"[SafeArea] Safe Area: {safeArea}");
                Debug.Log($"[SafeArea] Orientation: {Screen.orientation}");
            }

            // Convert safe area to anchor values (0-1 range)
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.x /= screenWidth;
            anchorMax.y /= screenHeight;

            // Apply based on settings
            if (!applyLeft) anchorMin.x = 0;
            if (!applyBottom) anchorMin.y = 0;
            if (!applyRight) anchorMax.x = 1;
            if (!applyTop) anchorMax.y = 1;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            
            // Reset offsets
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            if (debugMode)
            {
                Debug.Log($"[SafeArea] Applied anchors: min={anchorMin}, max={anchorMax}");
            }

            OnSafeAreaChanged?.Invoke(safeArea);
        }

        /// <summary>
        /// Force refresh the safe area
        /// </summary>
        public void Refresh()
        {
            _lastSafeArea = Rect.zero;
            ApplySafeArea();
        }

        /// <summary>
        /// Get current safe area margins in pixels
        /// </summary>
        public static SafeAreaMargins GetMargins()
        {
            Rect safeArea = Screen.safeArea;
            
            return new SafeAreaMargins
            {
                Top = Screen.height - (safeArea.y + safeArea.height),
                Bottom = safeArea.y,
                Left = safeArea.x,
                Right = Screen.width - (safeArea.x + safeArea.width)
            };
        }

        /// <summary>
        /// Check if device has a notch or cutout
        /// </summary>
        public static bool HasNotch()
        {
            var margins = GetMargins();
            return margins.Top > 0 || margins.Bottom > 0 || 
                   margins.Left > 0 || margins.Right > 0;
        }

        /// <summary>
        /// Get safe area as normalized rect (0-1 range)
        /// </summary>
        public static Rect GetNormalizedSafeArea()
        {
            Rect safeArea = Screen.safeArea;
            return new Rect(
                safeArea.x / Screen.width,
                safeArea.y / Screen.height,
                safeArea.width / Screen.width,
                safeArea.height / Screen.height
            );
        }

#if UNITY_EDITOR
        private Rect GetSimulatedSafeArea()
        {
            float top = 0, bottom = 0, left = 0, right = 0;
            int screenWidth = Screen.width > 0 ? Screen.width : 1080;
            int screenHeight = Screen.height > 0 ? Screen.height : 1920;

            switch (simulatedDevice)
            {
                case SimulatedDevice.iPhoneX:
                    top = 44;
                    bottom = 34;
                    break;
                case SimulatedDevice.iPhone14Pro:
                    top = 59; // Dynamic Island
                    bottom = 34;
                    break;
                case SimulatedDevice.iPhoneSE:
                    // No safe area needed (old style phone)
                    break;
                case SimulatedDevice.AndroidNotch:
                    top = 30;
                    break;
                case SimulatedDevice.AndroidPunch:
                    top = 25;
                    left = 50;
                    break;
                case SimulatedDevice.iPad:
                    // Minimal safe area for rounded corners
                    top = 24;
                    bottom = 20;
                    break;
                case SimulatedDevice.Custom:
                    top = customTop;
                    bottom = customBottom;
                    left = customLeft;
                    right = customRight;
                    break;
                case SimulatedDevice.None:
                default:
                    return new Rect(0, 0, screenWidth, screenHeight);
            }

            return new Rect(
                left,
                bottom,
                screenWidth - left - right,
                screenHeight - top - bottom
            );
        }
#endif
    }

    /// <summary>
    /// Safe area margins in pixels
    /// </summary>
    [System.Serializable]
    public struct SafeAreaMargins
    {
        public float Top;
        public float Bottom;
        public float Left;
        public float Right;

        public bool HasAnyMargin => Top > 0 || Bottom > 0 || Left > 0 || Right > 0;
        
        public override string ToString()
        {
            return $"SafeAreaMargins(T:{Top}, B:{Bottom}, L:{Left}, R:{Right})";
        }
    }
}
