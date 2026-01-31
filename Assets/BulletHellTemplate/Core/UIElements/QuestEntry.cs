using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a UI element for displaying quest information and handling quest completion triggers.
    /// Note that the actual logic for completing the quest and applying rewards
    /// is handled by UIQuestMenu, not here.
    /// </summary>
    public class QuestEntry : MonoBehaviour
    {
        #region UI References

        [Header("Quest Icon and Texts")]
        [Tooltip("Icon representing the quest")]
        public Image icon;

        [Tooltip("Quest title (UI Text)")]
        public TextMeshProUGUI title;

        [Tooltip("Quest description (UI Text)")]
        public TextMeshProUGUI description;

        [Header("EXP Text")]
        [Tooltip("UI Text for account experience amount")]
        public TextMeshProUGUI accountExpAmount;

        [Tooltip("UI Text for battle pass experience amount")]
        public TextMeshProUGUI passExpAmount;

        [Tooltip("UI Text for selected character experience amount")]
        public TextMeshProUGUI characterExpAmount;

        [Header("EXP GameObjects")]
        [Tooltip("GameObject containing account experience info")]
        public GameObject accountExpObject;

        [Tooltip("GameObject containing battle pass experience info")]
        public GameObject passExpObject;

        [Tooltip("GameObject containing selected character experience info")]
        public GameObject characterExpObject;

        [Header("Currency Reward")]
        [Tooltip("GameObject containing currency reward info")]
        public GameObject currencyRewardObject;

        [Tooltip("Currency reward icon")]
        public Image currencyRewardIcon;

        [Tooltip("UI Text for currency reward amount")]
        public TextMeshProUGUI currencyRewardAmount;

        [Header("Other Rewards")]
        [Tooltip("GameObject for additional rewards (character, icon, frame, item)")]
        public GameObject rewardGameObject;

        [Tooltip("UI Text for reward title/name")]
        public TextMeshProUGUI rewardTitle;

        [Tooltip("UI Image for reward icon")]
        public Image rewardIcon;

        [Header("Quest Status Icons")]
        [Tooltip("Icon that indicates a daily quest")]
        public Image dailyQuestIcon;

        [Tooltip("Icon that indicates a repeatable quest")]
        public Image repeatQuestIcon;

        [Tooltip("Image to indicate quest is completed")]
        public Image completed;

        [Header("Progress UI")]
        [Tooltip("UI Text for progress display (e.g. '3/10')")]
        public TextMeshProUGUI progressText;

        [Tooltip("UI image to visualize the progress bar fill amount")]
        public Image progressBar;

        [Tooltip("Button to confirm quest completion once progress is met")]
        public Button completeButton;

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal tracking of the player's current progress toward this quest.
        /// </summary>
        private int currentProgress;

        /// <summary>
        /// Flag indicating whether the quest is already completed.
        /// </summary>
        private bool isCompleted;

        /// <summary>
        /// Reference to the QuestItem ScriptableObject that holds quest data.
        /// </summary>
        private QuestItem questItem;

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets up the quest entry UI with the given quest data.
        /// </summary>
        /// <param name="_title">The translated or original title of the quest.</param>
        /// <param name="_description">The translated or original description of the quest.</param>
        /// <param name="_quest">The QuestItem containing all relevant quest data.</param>
        /// <param name="_isCompleted">Whether the quest is already marked as completed or not.</param>
        public void Setup(string _title, string _description, QuestItem _quest, bool _isCompleted)
        {
            // Store references locally
            questItem = _quest;
            isCompleted = _isCompleted;

            // Set icon, title, and description in UI
            if (icon != null && questItem.icon != null)
                icon.sprite = questItem.icon;

            if (title != null)
                title.text = _title;

            if (description != null)
                description.text = _description;

            // Handle EXP UI
            if (accountExpAmount != null)
                accountExpAmount.text = questItem.accountExp.ToString();

            if (passExpAmount != null)
                passExpAmount.text = questItem.battlePassExp.ToString();

            if (characterExpAmount != null)
                characterExpAmount.text = questItem.selectedCharacterExp.ToString();

            // Toggle visibility for EXP GameObjects
            if (accountExpObject != null)
                accountExpObject.SetActive(questItem.accountExp > 0);

            if (passExpObject != null)
                passExpObject.SetActive(questItem.battlePassExp > 0);

            if (characterExpObject != null)
                characterExpObject.SetActive(questItem.selectedCharacterExp > 0);

            // Handle Currency Reward
            if (currencyRewardAmount != null)
                currencyRewardAmount.text = questItem.currencyAmount.ToString();

            if (currencyRewardIcon != null && !string.IsNullOrEmpty(questItem.currencyReward) && questItem.currencyAmount > 0)
            {
                currencyRewardIcon.sprite = MonetizationManager.Singleton.GetCurrencyIcon(questItem.currencyReward);
            }

            if (currencyRewardObject != null)
                currencyRewardObject.SetActive(questItem.currencyAmount > 0);

            // Handle Other Rewards
            if (rewardGameObject != null)
                rewardGameObject.SetActive(questItem.questReward != QuestReward.None);

            // If there's a reward, set up the reward icon/title
            if (questItem.questReward != QuestReward.None && rewardIcon != null && rewardTitle != null)
            {
                UIQuestMenu uiQuestMenu = UIQuestMenu.Singleton;
                string currentLang = uiQuestMenu.currentLang;

                switch (questItem.questReward)
                {
                    case QuestReward.CharacterReward:
                        rewardIcon.sprite = questItem.characterData.icon;
                        rewardTitle.text = uiQuestMenu.GetTranslatedString(
                            questItem.characterData.characterNameTranslated,
                            questItem.characterData.characterName,
                            currentLang
                        );
                        break;

                    case QuestReward.IconReward:
                        rewardIcon.sprite = questItem.iconItem.icon;
                        rewardTitle.text = uiQuestMenu.GetTranslatedString(
                            questItem.iconItem.iconNameTranslated,
                            questItem.iconItem.iconName,
                            currentLang
                        );
                        break;

                    case QuestReward.FrameReward:
                        rewardIcon.sprite = questItem.frameItem.icon;
                        rewardTitle.text = uiQuestMenu.GetTranslatedString(
                            questItem.frameItem.frameNameTranslated,
                            questItem.frameItem.frameName,
                            currentLang
                        );
                        break;

                    case QuestReward.ItemReward:
                        rewardIcon.sprite = questItem.inventoryItem.itemIcon;
                        rewardTitle.text = uiQuestMenu.GetTranslatedString(
                            questItem.inventoryItem.titleTranslatedByLanguage,
                            questItem.inventoryItem.title,
                            currentLang
                        );
                        break;
                }
            }

            // Update quest-type icons
            if (dailyQuestIcon != null)
                dailyQuestIcon.gameObject.SetActive(questItem.questType == QuestType.Daily);

            if (repeatQuestIcon != null)
                repeatQuestIcon.gameObject.SetActive(questItem.questType == QuestType.Repeat);

            // Show the completed icon if already completed
            if (completed != null)
                completed.gameObject.SetActive(isCompleted);

            // Load and update progress from saved data
            int savedProgress = PlayerSave.LoadQuestProgress(questItem.questId);
            UpdateProgress(savedProgress, questItem.requirement.targetAmount);
        }

        /// <summary>
        /// Updates the progress UI of the quest and determines if the quest can be completed.
        /// </summary>
        /// <param name="currentProgressValue">The current progress of the quest.</param>
        /// <param name="targetAmountValue">The target amount required to complete the quest.</param>
        public void UpdateProgress(int currentProgressValue, int targetAmountValue)
        {
            currentProgress = currentProgressValue; // Store in our private field

            if (progressText != null)
                progressText.text = $"{currentProgress}/{targetAmountValue}";

            if (progressBar != null)
            {
                float progressPercentage = (float)currentProgress / targetAmountValue;
                progressBar.fillAmount = progressPercentage;
            }

            // Only allow completion if progress is met and quest is not flagged as completed
            if (completeButton != null)
                completeButton.interactable = (currentProgress >= targetAmountValue && !isCompleted);
        }

        /// <summary>
        /// Called when the user presses the "Complete Quest" button.
        /// Delegates the actual completion logic to UIQuestMenu.
        /// </summary>
        public void OnCompleteQuestButtonPressed()
        {
            if (questItem == null || isCompleted) return;

            // Defer the logic to UIQuestMenu
            if (UIQuestMenu.Singleton != null)
            {
                UIQuestMenu.Singleton.OnUserAttemptCompleteQuest(questItem);
            }
        }

        #endregion
    }
}
