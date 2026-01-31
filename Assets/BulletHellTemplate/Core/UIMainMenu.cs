using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    public class UIMainMenu : MonoBehaviour
    {
        [Header("UI Account")]
        public TextMeshProUGUI playerName;
        public Image playerIcon;
        public Image playerFrame;
        public TextMeshProUGUI accountLevel;
        public TextMeshProUGUI accountcurrentExp;
        public Image accountLevelProgressBar;

        public Transform characterContainer;
        public static UIMainMenu Singleton;
        private int selectedCharacter;
        private CharacterData characterData;

        [Header("Battle Pass")]
        public TextMeshProUGUI battlePassLevel;
        public TextMeshProUGUI battlePassCurrentExp;
        public Image battlePassProgressBar;
        public GameObject hasRewardToClaim;

        [Header("Character")]
        public TextMeshProUGUI characterLevel;
        public TextMeshProUGUI characterCurrentExp;
        public Image characterProgressLevel;
        public TextMeshProUGUI masteryLevelName;
        public Image masteryIcon;

        [Header("Daily Rewards")]
        public TextMeshProUGUI nextDailyReward;
        public GameObject canClaimDailyRewardObj;
        public GameObject canClaimNewPlayerRewardObj;

        private string currentLang;
        private DailyRewardsData localDailyData;
        private NewPlayerRewardsData localNewPlayerData;
        private bool canClaimDaily;           // cached flags
        private bool canClaimNewPlayer;
        private int currentDayIndex;

        private bool allRewardsCollected;
        private DateTime nextResetTime;
        /// <summary>
        /// Awake is called when the script Singleton is being loaded.
        /// It initializes the Singleton.
        /// </summary>
        private void Awake()
        {
            Singleton = this;
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            LanguageManager.LanguageManager.onLanguageChanged += UpdateCharacter;
        }

        /// <summary>
        /// Refreshes reward flags every time.
        /// </summary>
        private void OnEnable()
        {
            localDailyData = PlayerSave.GetDailyRewardsLocal();
            localNewPlayerData = PlayerSave.GetNewPlayerRewardsLocal();

            if (localDailyData.firstClaimDate != DateTime.MinValue)
                currentDayIndex = (int)(DateTime.Now.Date - localDailyData.firstClaimDate.Date).TotalDays;
            else
                currentDayIndex = 0;

            nextResetTime = PlayerSave.GetNextDailyReset();
            if (nextResetTime == DateTime.MinValue || nextResetTime <= DateTime.Now)
            {
                nextResetTime = DateTime.Now.Date.AddDays(1);
                PlayerSave.SetNextDailyReset(nextResetTime);   
            }

            EvaluateRewardFlags();  
            UpdateDailyRewardUI(); 
        }


        public void OnDestroy()
        {
            Singleton = null;
            LanguageManager.LanguageManager.onLanguageChanged -= UpdateCharacter;
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        private void Start()
        {
            BackendManager.SetBackendInitialized();
            LoadPlayerInfo();

            canClaimDailyRewardObj.SetActive(false);
            canClaimNewPlayerRewardObj.SetActive(false);

            // Load local daily data once
            localDailyData = PlayerSave.GetDailyRewardsLocal();

            // Calculate current day index based on firstClaimDate
            DateTime firstDate = localDailyData.firstClaimDate.Date;
            currentDayIndex = (int)(DateTime.Now.Date - firstDate).TotalDays;

            // Get the next daily reset time
            nextResetTime = PlayerSave.GetNextDailyReset();
        }

        private void Update()
        {
            if (Time.frameCount % 30 == 0) EvaluateRewardFlags();
            UpdateDailyRewardUI();
        }

        /// <summary>
        /// Loads player information from PlayerSave and updates the UI elements.
        /// Shows "MAX" if the account level is at maximum.
        /// </summary>
        public void LoadPlayerInfo()
        {
            // Get player name
            string name = PlayerSave.GetPlayerName();
            playerName.text = !string.IsNullOrEmpty(name) ? name : "Unknown Player";

            // Account level and EXP
            int _accountLevel = PlayerSave.GetAccountLevel();
            int _currentAccountExp = PlayerSave.GetAccountCurrentExp();
            int _expToNextLevel = GameInstance.Singleton.GetAccountExpForLevel(_accountLevel);

            accountLevel.text = _accountLevel.ToString();

            // Check if the account is at max level
            int maxAccountLevel = GameInstance.Singleton.accountLevels.accountMaxLevel;
            if (_accountLevel >= maxAccountLevel)
            {
                accountcurrentExp.text = "MAX";
                accountLevelProgressBar.fillAmount = 1f;
            }
            else
            {
                accountcurrentExp.text = $"{_currentAccountExp}/{_expToNextLevel}";
                accountLevelProgressBar.fillAmount = _expToNextLevel > 0
                    ? (float)_currentAccountExp / _expToNextLevel
                    : 0f;
            }

            // Assign player icon
            string iconId = PlayerSave.GetPlayerIcon();
            foreach (IconItem item in GameInstance.Singleton.iconItems)
            {
                if (item.iconId == iconId)
                {
                    playerIcon.sprite = item.icon;
                    break;
                }
            }

            // Assign player frame
            string frameId = PlayerSave.GetPlayerFrame();
            foreach (FrameItem item in GameInstance.Singleton.frameItems)
            {
                if (item.frameId == frameId)
                {
                    playerFrame.sprite = item.icon;
                    break;
                }
            }

            // Battle Pass info
            int currentPassLevel = BattlePassManager.Singleton.currentLevel;
            int currentPassXP = BattlePassManager.Singleton.currentXP;
            int xpForNextPassLevel = BattlePassManager.Singleton.xpForNextLevel;

            battlePassLevel.text = currentPassLevel.ToString();
            battlePassCurrentExp.text = currentPassXP + "/" + xpForNextPassLevel;
            battlePassProgressBar.fillAmount = (float)currentPassXP / xpForNextPassLevel;

            // Finally, load the selected character info
            LoadCharacterInfo();
        }

        /// <summary>
        /// Updates only the battle pass elements.
        /// </summary>
        public void UpdateBattlePassInfo()
        {
            int currentPassLevel = BattlePassManager.Singleton.currentLevel;
            int currentPassXP = BattlePassManager.Singleton.currentXP;
            int xpForNextPassLevel = BattlePassManager.Singleton.xpForNextLevel;

            battlePassLevel.text = currentPassLevel.ToString();
            battlePassCurrentExp.text = currentPassXP + "/" + xpForNextPassLevel;
            battlePassProgressBar.fillAmount = (float)currentPassXP / xpForNextPassLevel;
        }

        /// <summary>
        /// Loads character information and updates the character model.
        /// </summary>
        public void LoadCharacterInfo()
        {
            selectedCharacter = PlayerSave.GetSelectedCharacter();
            UpdateCharacter();
        }

        /// <summary>
        /// Updates the displayed character model based on the selected character and applied skin.
        /// Shows "MAX" if the character is at max level.
        /// </summary>
        public void UpdateCharacter()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();

            // Clear previous character model
            if (characterContainer != null)
            {
                for (int i = characterContainer.childCount - 1; i >= 0; i--)
                {
                    Transform child = characterContainer.GetChild(i);
                    if (child != null)
                        Destroy(child.gameObject);
                }
            }

            // Find the character data for the selected character
            characterData = null;
            foreach (CharacterData character in GameInstance.Singleton.characterData)
            {
                if (character.characterId == selectedCharacter)
                {
                    characterData = character;
                    break;
                }
            }

            if (characterContainer == null || characterData == null)
            {
                return;
            }

            // Retrieve the skin index from PlayerSave
            int skinIndex = PlayerSave.GetCharacterSkin(selectedCharacter);

            // Check if a valid skin is applied
            if (characterData.characterSkins != null &&
                characterData.characterSkins.Length > 0 &&
                skinIndex >= 0 &&
                skinIndex < characterData.characterSkins.Length)
            {
                CharacterSkin skin = characterData.characterSkins[skinIndex];
                if (skin.skinCharacterModel != null)
                {
                    Instantiate(skin.skinCharacterModel, characterContainer);
                }
                else if (characterData.characterModel != null)
                {
                    Instantiate(characterData.characterModel, characterContainer);
                }
            }
            else if (characterData.characterModel != null)
            {
                Instantiate(characterData.characterModel, characterContainer);
            }

            // Optional: update rotation or other logic
            if (DragRotateAndHit.Singleton != null)
            {
                DragRotateAndHit.Singleton.UpdateCharacter();
            }

            // Get character level and current exp
            int _chracterLevel = PlayerSave.GetCharacterLevel(selectedCharacter);
            int _characterCurrentExp = PlayerSave.GetCharacterCurrentExp(selectedCharacter);

            // Calculate required exp safely
            int requiredExp = 0;
            if (characterData.expPerLevel != null && _chracterLevel < characterData.expPerLevel.Length)
            {
                requiredExp = characterData.expPerLevel[_chracterLevel];
            }

            // Check if character is at max level
            if (_chracterLevel >= characterData.maxLevel)
            {
                // Display MAX
                characterLevel.text = _chracterLevel.ToString();
                characterCurrentExp.text = "MAX";
                characterProgressLevel.fillAmount = 1f;
            }
            else
            {
                characterLevel.text = _chracterLevel.ToString();
                characterCurrentExp.text = $"{_characterCurrentExp}/{requiredExp}";
                characterProgressLevel.fillAmount = requiredExp > 0
                    ? (float)_characterCurrentExp / requiredExp
                    : 0f;
            }

            // Mastery level name and icon
            int currentMasteryLevel = PlayerSave.GetCharacterMasteryLevel(selectedCharacter);
            CharacterMasteryLevel masteryInfo = GameInstance.Singleton.GetMasteryLevel(currentMasteryLevel);
            string masteryTranslatedName = GetTranslatedString(
                masteryInfo.masteryNameTranslated,
                masteryInfo.masteryName,
                currentLang);

            masteryLevelName.text = masteryTranslatedName;
            if (masteryIcon != null)
                masteryIcon.sprite = masteryInfo.masteryIcon;
        }

        /// <summary>
        /// Changes the selected character and updates the UI accordingly.
        /// </summary>
        /// <param name="characterId">The ID of the new selected character.</param>
        public void ChangeSelectedCharacter(int characterId)
        {
            LoadCharacterInfo();
        }

        private void EvaluateRewardFlags()
        {
            /* DAILY */
            bool claimedToday = localDailyData.lastClaimDate != DateTime.MinValue &&
                                localDailyData.lastClaimDate.Date == DateTime.Now.Date;

            bool notClaimedCurrentDay = !localDailyData.claimedRewards.Contains(currentDayIndex);
            bool resetReached = (nextResetTime - DateTime.Now).TotalSeconds <= 0;

            canClaimDaily = (!claimedToday && notClaimedCurrentDay) || resetReached;

            /* NEW‑PLAYER */
            int expectedIdx = localNewPlayerData.claimedRewards.Count;
            int daysSinceJoin = localNewPlayerData.accountCreationDate != DateTime.MinValue
                ? (int)(DateTime.Now.Date - localNewPlayerData.accountCreationDate.Date).TotalDays
                : int.MaxValue;

            bool claimedNPtoday = localNewPlayerData.lastClaimDate != DateTime.MinValue &&
                                  localNewPlayerData.lastClaimDate.Date == DateTime.Now.Date;

            canClaimNewPlayer =
                !claimedNPtoday &&
                expectedIdx <= daysSinceJoin;      
        }

        public void UpdateDailyRewardUI()
        {
            // Toggle buttons
            canClaimDailyRewardObj.SetActive(canClaimDaily);
            canClaimNewPlayerRewardObj.SetActive(canClaimNewPlayer);

            if (canClaimDaily)
            {
                nextDailyReward.text = "Daily reward ready!";
                return;
            }

            TimeSpan remaining = nextResetTime - DateTime.Now;
            if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;

            nextDailyReward.text = string.Format(
                "{0:D2}:{1:D2}:{2:D2}",
                remaining.Hours,
                remaining.Minutes,
                remaining.Seconds
            );
        }

        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
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

        public async void TestAddAccountExp()
        {
            var result = await BackendManager.Service.TestAddAccountExpAsync(200);
            if (result.Success)
            {
                LoadPlayerInfo();
                return;
            }
            Debug.LogError("Failed to add account exp");
        }
    }
}
