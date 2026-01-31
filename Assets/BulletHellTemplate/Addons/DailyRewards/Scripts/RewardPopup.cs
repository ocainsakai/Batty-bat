using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BulletHellTemplate
{
    public class RewardPopup : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;

        /// <summary>
        /// Sets up the reward popup with the given reward information.
        /// </summary>
        /// <param name="_icon">Reward icon</param>
        /// <param name="_title">Reward title</param>
        /// <param name="_description">Reward description</param>
        public void Setup(Sprite _icon, string _title, string _description)
        {
            icon.sprite = _icon;
            title.text = _title;
            description.text = _description;
            gameObject.SetActive(true); // Ensure the popup is visible
        }

        /// <summary>
        /// Closes the popup.
        /// </summary>
        public void ClosePopup()
        {
            gameObject.SetActive(false);
        }
    }
}
