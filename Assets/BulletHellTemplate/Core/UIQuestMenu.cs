using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the display of quests in the UI.
    /// Displays available quests, handles quest completion logic,
    /// and can optionally hide completed quests.
    /// </summary>
    public class UIQuestMenu : MonoBehaviour
    {
        #region Singleton and Public Fields

        [Tooltip("Prefab for individual quest entries")]
        public QuestEntry questEntryPrefab;

        [Tooltip("Container where quest entries will be instantiated")]
        public Transform container;

        /// <summary>
        /// Singleton reference for global access to UIQuestMenu.
        /// </summary>
        public static UIQuestMenu Singleton;

        [Tooltip("Whether or not to hide completed quests from the UI")]
        public bool hideCompletedQuests;

        /// <summary>
        /// Current language code or identifier used by the UI.
        /// </summary>
        [HideInInspector]
        public string currentLang;

        #endregion

        #region Unity Callbacks

        /// <summary>
        /// Initializes the singleton reference on startup.
        /// </summary>
        private void Start()
        {
            Singleton = this;
        }

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// Sets the current language and loads quests into the UI.
        /// </summary>
        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            LoadQuestsAsync().Forget();  
        }

        #endregion

        #region Public Methods

        private async UniTaskVoid LoadQuestsAsync()
        {
            await BackendManager.Service.RefreshQuestLevelProgressAsync();

            LoadQuests();
        }

        /// <summary>
        /// Loads and displays the available quests in the UI.
        /// If 'hideCompletedQuests' is true, completed quests (except repeat) will be skipped.
        /// Otherwise, all quests are shown, with an indicator for those that are completed.
        /// Additionally, it checks if the player's level or characters' levels
        /// exceed any quest requirements to set progress to 100%.
        /// </summary>
        public void LoadQuests()
        {
            // Clear containerPassItems
            foreach (Transform child in container)
                Destroy(child.gameObject);

            List<QuestItem> allQuests = GameInstance.Singleton.questData.ToList();

            foreach (QuestItem quest in allQuests)
            {
                bool isCompleted = PlayerSave.IsQuestCompleted(quest.questId);
                if (hideCompletedQuests && quest.questType != QuestType.Repeat && isCompleted)
                    continue;

                var entry = Instantiate(questEntryPrefab, container);

                string title = GetTranslatedString(quest.titleTranslated, quest.title, currentLang);
                string desc = GetTranslatedString(quest.descriptionTranslated, quest.description, currentLang);

                entry.Setup(title, desc, quest, isCompleted);

                int progress = PlayerSave.LoadQuestProgress(quest.questId);
                int target = quest.requirement.targetAmount;
                entry.UpdateProgress(progress, target);
            }
        }


        /// <summary>
        /// Called by the QuestEntry when the player tries to complete the quest.
        /// Checks if the quest is indeed completable, then applies rewards.
        /// Marks the quest as completed or resets its progress if it's a repeat quest.
        /// Finally, reloads the quest UI for updated feedback.
        /// </summary>
        /// <param name="quest">The quest item the player is attempting to complete.</param>
        public async void OnUserAttemptCompleteQuest(QuestItem quest)
        {
            if (quest == null) return;

            RequestResult res = await BackendManager.Service.CompleteQuestAsync(quest.questId);

            if (!res.Success)
            {
                string msg = res.Reason switch
                {
                    "0" => "Already completed!",
                    "1" => "Not enough progress!",
                    "2" => "Quest not found!",
                    _ => "Unknown error"
                };
                Debug.LogWarning(msg);
                return;
            }

            // Reload UI to reflect new state
            LoadQuests();
        }

        /// <summary>
        /// Returns a translated string according to the current language
        /// or a fallback string if no translation is found.
        /// </summary>
        /// <param name="translations">An array of translations for different languages.</param>
        /// <param name="fallback">The default (fallback) string if no matching translation is found.</param>
        /// <param name="currentLang">The language code or identifier currently in use.</param>
        /// <returns>A string in the user's selected language, or the fallback if none is found.</returns>
        public string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        /// <summary>
        /// Returns a translated description string according to the current language
        /// or a fallback string if no translation is found.
        /// </summary>
        /// <param name="translations">An array of description translations for different languages.</param>
        /// <param name="fallback">The default (fallback) string if no matching translation is found.</param>
        /// <param name="currentLang">The language code or identifier currently in use.</param>
        /// <returns>A string in the user's selected language, or the fallback if none is found.</returns>
        public string GetTranslatedString(DescriptionTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        #endregion
    }
}
