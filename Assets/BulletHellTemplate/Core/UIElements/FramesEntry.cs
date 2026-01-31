using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the frame selection and application process in the UI.
    /// </summary>
    public class FramesEntry : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Displays the frame image.")]
        public Image icon;

        [Tooltip("Highlights the selected frame.")]
        public Image selected;

        [Tooltip("Displays the name of the frame.")]
        public TextMeshProUGUI frameName;

        private string frameId;

        /// <summary>
        /// Sets the frame information for this entry.
        /// </summary>
        /// <param name="_frameId">The ID of the frame to set.</param>
        public void SetFrameInfo(string _frameId)
        {
            frameId = _frameId;
        }

        /// <summary>
        /// Applies the frame locally when clicked.
        /// </summary>
        public async void OnClickChangeFrame()
        {
            RequestResult res = await BackendManager.Service.ChangePlayerFrameAsync(frameId);

            if (res.Success)
            {
                UIProfileMenu.Singleton.LoadProfile();
                UIMainMenu.Singleton.LoadPlayerInfo();
                UIProfileMenu.Singleton.OnChangeFrame.Invoke();
            }
            else
            {
                Debug.LogWarning(res.Reason == "0"
                    ? "Frame not owned!"
                    : "Frame not found!");
            }
        }
    }
}
