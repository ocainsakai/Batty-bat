using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the loading screen during scene transitions.
    /// Displays a "Loading" text with animated dots and ensures a minimum loading time.
    /// Shows loading progress percentage and handles pooling progress.
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Singleton;

        [Header("UI Elements")]
        [Tooltip("Text component displaying the animated 'Loading' message.")]
        public TextMeshProUGUI loadingText;

        [Tooltip("UI containerPassItems holding the loading screen.")]
        public GameObject UILoading;

        [Tooltip("Slider displaying loading progress.")]
        public Slider loadingProgressBar;

        [Header("Loading Settings")]
        [Tooltip("Minimum duration (in seconds) the loading screen should be displayed.")]
        public float minimumLoadingTime = 2f;

        [Header("Loading AudioClip")]
        public AudioClip loadingAudioClip;
        public string Loadingtag = "master";
        [HideInInspector]
        public bool isLoading = false;

        private bool _running;
        private float _minTime;
        private float dotTimer = 0f;
        private int dotCount = 0;

        private void Awake()
        {
            // Singleton pattern to ensure only one instance exists
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads a scene asynchronously with a loading screen.
        /// Now includes pooling progress and updates the progress bar.
        /// Handles scenes without `GameplayManager`.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <returns>An IEnumerator for coroutine execution.</returns>
        public IEnumerator LoadSceneWithLoadingScreen(string sceneName)
        {
            isLoading = true;
            AudioManager.Singleton.PlayLoadingMenu(loadingAudioClip, Loadingtag);
            UILoading.SetActive(true);

            float startTime = Time.time;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            float simulatedProgress = 0f;
            float simulatedProgressRate = 1f / minimumLoadingTime;
            float totalProgress = 0f;

            while (!asyncLoad.isDone)
            {
                float sceneProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                simulatedProgress += simulatedProgressRate * Time.deltaTime;
                totalProgress = Mathf.Min(sceneProgress, simulatedProgress);

                if (loadingProgressBar != null)
                {
                    loadingProgressBar.value = totalProgress;
                }

                if (loadingText != null)
                {
                    int progressPercentage = Mathf.RoundToInt(totalProgress * 100f);
                    loadingText.text = $"{LanguageManager.LanguageManager.Instance.GetTextEntryByID("loading_loading")}{GetAnimatedDots()}\n{progressPercentage}%";
                }

                if (asyncLoad.progress >= 0.9f && (Time.time - startTime >= minimumLoadingTime))
                {
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            if (asyncLoad.isDone)
            {
                if (UILoading.activeSelf)
                {
                    UILoading.SetActive(false);
                }
                AudioManager.Singleton.StopLoadingAudioPlay();
            }

            isLoading = false;
        }

        /// <summary>
        /// Called to open the loading screen. Used during scene loading with Photon Fusion.
        /// </summary>
        /// <param name="minTime">Minimum duration the loading screen should remain visible.</param>
        public void Open(float minTime = 2f)
        {
            // Prevent reopening if already running
            if (_running) return;

            isLoading = true;
            _minTime = minTime;

            // Start the loading coroutine (used during Fusion scene transitions)
            StartCoroutine(SimpleLoader());
        }

        /// <summary>
        /// Called to close the loading screen. Used after Photon Fusion scene load is completed.
        /// </summary>
        public void Close()
        {
            _running = false;
        }

        private IEnumerator SimpleLoader()
        {
            _running = true;           

            AudioManager.Singleton.PlayLoadingMenu(loadingAudioClip, Loadingtag);
            UILoading.SetActive(true);

            float t0 = Time.time;
            while (_running || Time.time - t0 < _minTime)
            {
                float pct = Mathf.InverseLerp(0, _minTime, Time.time - t0);
                if (loadingProgressBar) loadingProgressBar.value = pct;
                if (loadingText)
                    loadingText.text =
                      $"{LanguageManager.LanguageManager.Instance.GetTextEntryByID("loading_loading")}{GetAnimatedDots()}\n{Mathf.RoundToInt(pct * 100)}%";
                yield return null;
            }

            UILoading.SetActive(false);
            AudioManager.Singleton.StopLoadingAudioPlay();
            isLoading = false;
        }

        /// <summary>
        /// Returns a string with animated dots based on time.
        /// </summary>
        /// <returns>A string with 0 to 3 dots.</returns>
        private string GetAnimatedDots()
        {
            dotTimer += Time.deltaTime;
            if (dotTimer >= 0.5f)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;
            }
            return new string('.', dotCount);
        }
    }
}
