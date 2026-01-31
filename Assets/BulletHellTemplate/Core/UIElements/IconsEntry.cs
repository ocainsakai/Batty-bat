using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the icon selection and application process in the UI.
    /// </summary>
    public class IconsEntry : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Displays the icon image.")]
        public Image icon;

        [Tooltip("Highlights the selected icon.")]
        public Image selected;

        [Tooltip("Displays the name of the icon.")]
        public TextMeshProUGUI iconName;

        private string iconId;

        /// <summary>
        /// Sets the icon information for this entry.
        /// </summary>
        /// <param name="_iconId">The ID of the icon to set.</param>
        public void SetIconInfo(string _iconId)
        {
            iconId = _iconId;
        }

        /// <summary>
        /// Applies the icon locally when clicked.
        /// </summary>
        public async void OnClickChangeIcon()
        {
            RequestResult res = await BackendManager.Service.ChangePlayerIconAsync(iconId);

            if (res.Success)
            {
                UIProfileMenu.Singleton.LoadProfile();
                UIMainMenu.Singleton.LoadPlayerInfo();
                UIProfileMenu.Singleton.OnChangeIcon.Invoke();
            }
            else
            {
                Debug.LogWarning(res.Reason == "0"
                    ? "Icon not owned!"
                    : "Icon not found!");
            }
        }
    }
}
