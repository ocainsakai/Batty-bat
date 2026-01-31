using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class RewardEntry : MonoBehaviour
    {
        public Image icon; // Reward icon
        public TextMeshProUGUI title; // Reward title
        public TextMeshProUGUI description; // Reward description
        public Button claimButton; // Button to claim the reward
        public Image claimedIcon; // Icon indicating if the reward has been claimed
        public TextMeshProUGUI day; // Text component to display the day number
        private string rewardId;

        /// <summary>
        /// Sets up the reward entry UI.
        /// </summary>
        /// <param name="_icon">Icon to display</param>
        /// <param name="_title">Title of the reward</param>
        /// <param name="_description">Description of the reward</param>
        /// <param name="_rewardId">Unique ID of the reward</param>
        /// <param name="_dayNumber">Day number of the reward</param>
        public void Setup(Sprite _icon, string _title, string _description, string _rewardId, int _dayNumber)
        {
            icon.sprite = _icon;
            title.text = _title;
            description.text = _description;
            day.text = $"Day {_dayNumber}"; // Set the day text
            rewardId = _rewardId;
            claimedIcon.gameObject.SetActive(false); // Hide claimed icon initially
            claimButton.onClick.RemoveAllListeners();
            // The claim action is assigned by the manager using EnableClaimButton
        }

        /// <summary>
        /// Marks the reward as claimed and updates the UI.
        /// </summary>
        public void SetClaimed()
        {
            claimButton.interactable = false; // Disable claim button
            claimedIcon.gameObject.SetActive(true); // Show claimed icon
        }

        /// <summary>
        /// Disables the claim button for unclaimable rewards.
        /// </summary>
        public void SetLocked()
        {
            claimButton.interactable = false; // Disable claim button
            claimedIcon.gameObject.SetActive(false); // Ensure claimed icon is hidden
        }

        /// <summary>
        /// Enables the claim button with a click action.
        /// </summary>
        /// <param name="claimAction">Action to perform when claimed</param>
        public void EnableClaimButton(UnityEngine.Events.UnityAction claimAction)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(claimAction);
            claimButton.interactable = true;
        }
    }
}
