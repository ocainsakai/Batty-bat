using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Core.Systems.UISystem
{
    /// <summary>
    /// Base class for full-screen UI screens.
    /// Screens are typically mutually exclusive (only one visible at a time).
    /// </summary>
    public class UIScreen : UIView
    {
        [Header("Screen Settings")]
        [SerializeField] protected bool hideOthersOnShow = true;
        [SerializeField] protected bool canGoBack = true;

        public bool HideOthersOnShow => hideOthersOnShow;
        public bool CanGoBack => canGoBack;

        public override async UniTask Show()
        {
            // Notify UIManager to handle screen switching
            if (hideOthersOnShow && UIManager.HasInstance)
            {
                UIManager.Instance.OnScreenShowing(this);
            }

            await base.Show();

            // Register as active screen
            if (UIManager.HasInstance)
            {
                UIManager.Instance.SetActiveScreen(this);
            }
        }

        public override async UniTask Hide()
        {
            await base.Hide();

            // Unregister from UIManager
            if (UIManager.HasInstance)
            {
                UIManager.Instance.OnScreenHidden(this);
            }
        }

        /// <summary>
        /// Handle back button/gesture
        /// </summary>
        public virtual void OnBackPressed()
        {
            if (canGoBack && UIManager.HasInstance)
            {
                UIManager.Instance.GoBack().Forget();
            }
        }
    }
}
