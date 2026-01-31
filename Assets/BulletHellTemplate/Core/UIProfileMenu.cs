using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the profile menu UI, including displaying player information (name, icon, frame), favorite character data,
    /// audio sliders, localized status messages, and *toggleable* icon / frame selection panels.
    /// <para>Buttons that previously called <see cref="LoadIcons"/> or <see cref="LoadFrames"/> directly can remain wired the same way: each method now behaves as a toggle. If the requested content is already visible, the call will close the container; otherwise it will open the container and (re)build the corresponding entries.</para>
    /// <para>The container now starts <b>closed</b> every time the menu is enabled.</para>
    /// </summary>
    public class UIProfileMenu : MonoBehaviour
    {
        /*────────────────────────────────────────────────────────────────────────────*/
        #region Inspector Fields

        [Header("UI Elements")]
        [Tooltip("Text component to display the player's name.")]
        public TextMeshProUGUI playerName;

        [Tooltip("Image component to display the player's icon.")]
        public Image playerIcon;

        [Tooltip("Image component to display the player's frame.")]
        public Image playerFrame;

        [Header("UI Prefabs")]
        [Tooltip("Prefab for creating icon entries.")]
        public IconsEntry iconsEntryPrefab;

        [Tooltip("Prefab for creating frame entries.")]
        public FramesEntry framesEntryPrefab;

        [Tooltip("Container to hold dynamically created icon and frame entries.")]
        public Transform container;

        [Tooltip("Container GameObject for icons/frames.")]
        public GameObject containerPref;

        [Tooltip("UI passEntryPrefab for the name change input screen.")]
        public GameObject changeNamePref;

        [Tooltip("Input field for entering a new name.")]
        public TMP_InputField changeNameInput;

        [Tooltip("UI component to display status messages (success/error).")]
        public TextMeshProUGUI statusText;

        [Header("Favorite character")]
        [Tooltip("Image to display mastery icon of the favorite character.")]
        public Image masteryIcon;

        [Tooltip("Image fill component to display mastery progress.")]
        public Image masteryProgressBar;

        [Tooltip("UI text to display current mastery experience.")]
        public TextMeshProUGUI currentMasteryExp;

        [Tooltip("UI text to display the mastery level name.")]
        public TextMeshProUGUI masteryName;

        [Tooltip("Container to hold the temporary favorite character model.")]
        public Transform tempCharacterContainer;

        [Header("UI Translations")]
        [Tooltip("Fallback message when there are not enough tickets to change the name.")]
        public string notEnoughTickets = "Not enough tickets to change the name.";
        [Tooltip("Translations for the message when there are not enough tickets to change the name.")]
        public NameTranslatedByLanguage[] notEnoughTicketsTranslated;

        [Tooltip("Fallback message when name length is invalid.")]
        public string nameLengthError = "Name must be between 3 and 14 characters.";
        [Tooltip("Translations for the message when name length is invalid.")]
        public NameTranslatedByLanguage[] nameLengthErrorTranslated;

        [Tooltip("Fallback message when the name is already taken.")]
        public string nameAlreadyTaken = "Name already taken.";
        [Tooltip("Translations for the message when the name is already taken.")]
        public NameTranslatedByLanguage[] nameAlreadyTakenTranslated;

        [Tooltip("Fallback message when the name is changed successfully.")]
        public string nameChangeSuccess = "Name changed successfully.";
        [Tooltip("Translations for the message when the name is changed successfully.")]
        public NameTranslatedByLanguage[] nameChangeSuccessTranslated;

        [Tooltip("Fallback message when there's an error changing the name.")]
        public string nameChangeFail = "Error changing name.";
        [Tooltip("Translations for the message when there's an error changing the name.")]
        public NameTranslatedByLanguage[] nameChangeFailTranslated;

        [Header("Events")]
        [Tooltip("Event invoked when the menu is opened.")]
        public UnityEvent OnOpenMenu;

        [Tooltip("Event invoked when the menu is closed.")]
        public UnityEvent OnCloseMenu;

        [Tooltip("Event invoked when the nickname is successfully changed.")]
        public UnityEvent OnChangeNickname;

        [Tooltip("Event invoked when the icon is successfully changed.")]
        public UnityEvent OnChangeIcon;

        [Tooltip("Event invoked when the frame is successfully changed.")]
        public UnityEvent OnChangeFrame;

        [Tooltip("Event invoked when a coupon is redeemed successfully.")]
        public UnityEvent OnRedeemCoupon;

        [Header("Audio Settings")]
        [Tooltip("Slider for adjusting master volume.")]
        public Slider masterVolumeSlider;

        [Tooltip("Slider for adjusting VFX volume.")]
        public Slider vfxVolumeSlider;

        [Tooltip("Slider for adjusting ambience volume.")]
        public Slider ambienceVolumeSlider;

        #endregion
        /*────────────────────────────────────────────────────────────────────────────*/

        /*────────────────────────────────────────────────────────────────────────────*/
        #region Private Runtime State

        private string currentLang;
        private readonly List<IconsEntry> iconsEntries = new();
        private readonly List<FramesEntry> framesEntries = new();
        public static UIProfileMenu Singleton; // maintained for legacy usages

        /// <summary>
        /// Runtime state for the dynamic selection container.
        /// </summary>
        private enum ContentState { Closed, Icons, Frames }
        private ContentState _contentState = ContentState.Closed;

        #endregion
        /*────────────────────────────────────────────────────────────────────────────*/

        #region Unity Lifecycle
        private void Awake()
        {
            LanguageManager.LanguageManager.onLanguageChanged += UpdateText;
            Singleton = this;
        }

        private void OnDestroy()
        {
            LanguageManager.LanguageManager.onLanguageChanged -= UpdateText;
        }

        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            OnOpenMenu?.Invoke();
            DestroyTemporaryModel();
            LoadProfile();
            LoadFavoriteCharacter();
            UpdateText();
            SetupSliders();

            // Ensure the selection container starts closed when menu opens.
            HideContainer();
        }

        private void OnDisable()
        {
            DestroyTemporaryModel();
            HideContainer();
            OnCloseMenu?.Invoke();
            if (UIMainMenu.Singleton != null)
                UIMainMenu.Singleton.LoadPlayerInfo();
        }
        #endregion

        #region Language & Localization
        private void UpdateText()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            LoadFavoriteCharacter();
        }
        #endregion

        #region Profile Loading
        /// <summary>
        /// Loads and displays the player's profile information (name, icon, frame).
        /// NOTE: No longer toggles the icon/frame container; that behavior is handled
        /// by <see cref="LoadIcons"/> / <see cref="LoadFrames"/> toggle methods.
        /// </summary>
        public void LoadProfile()
        {
            string name = PlayerSave.GetPlayerName();
            if (playerName != null)
                playerName.text = !string.IsNullOrEmpty(name) ? name : "Unknown Player";

            string iconId = PlayerSave.GetPlayerIcon();
            string frameId = PlayerSave.GetPlayerFrame();

            SetPlayerIcon(iconId);
            SetPlayerFrame(frameId);
        }
        #endregion

        #region Audio Sliders
        /// <summary>
        /// Initialize sliders from persisted audio values and hook change events.
        /// Uses <see cref="AudioManager"/> setters (which also persist changes).
        /// </summary>
        private void SetupSliders()
        {
            var am = AudioManager.Singleton;
            if (am == null)
                return;

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveAllListeners();
                masterVolumeSlider.SetValueWithoutNotify(am.masterVolume);          // start from saved
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (vfxVolumeSlider != null)
            {
                vfxVolumeSlider.onValueChanged.RemoveAllListeners();
                vfxVolumeSlider.SetValueWithoutNotify(am.vfxVolume);                // start from saved
                vfxVolumeSlider.onValueChanged.AddListener(OnVFXVolumeChanged);
            }

            if (ambienceVolumeSlider != null)
            {
                ambienceVolumeSlider.onValueChanged.RemoveAllListeners();
                ambienceVolumeSlider.SetValueWithoutNotify(am.ambienceVolume);      // start from saved
                ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            }
        }

        /// <summary>Called by Master slider – forwards to <see cref="AudioManager.SetMasterVolume"/>.</summary>
        public void OnMasterVolumeChanged(float value)
        {
            var am = AudioManager.Singleton;
            if (am != null)
                am.SetMasterVolume(value);
        }

        /// <summary>Called by VFX slider – forwards to <see cref="AudioManager.SetVFXVolume"/>.</summary>
        public void OnVFXVolumeChanged(float value)
        {
            var am = AudioManager.Singleton;
            if (am != null)
                am.SetVFXVolume(value);
        }

        /// <summary>Called by Ambience slider – forwards to <see cref="AudioManager.SetAmbienceVolume"/>.</summary>
        public void OnAmbienceVolumeChanged(float value)
        {
            var am = AudioManager.Singleton;
            if (am != null)
                am.SetAmbienceVolume(value);
        }
        #endregion

        /*────────────────────────────────────────────────────────────────────────────*/
        #region Toggle Container API (Call These From Buttons)
        /// <summary>
        /// Toggle the icon selection panel. If icons are visible, closes the panel.
        /// If closed or currently showing frames, shows icons.
        /// </summary>
        public void LoadIcons()
        {
            if (_contentState == ContentState.Icons && containerPref != null && containerPref.activeSelf)
            {
                HideContainer();
                return;
            }
            ShowIcons();
        }

        /// <summary>
        /// Toggle the frame selection panel. If frames are visible, closes the panel.
        /// If closed or currently showing icons, shows frames.
        /// </summary>
        public void LoadFrames()
        {
            if (_contentState == ContentState.Frames && containerPref != null && containerPref.activeSelf)
            {
                HideContainer();
                return;
            }
            ShowFrames();
        }
        #endregion

        #region Internal Build Helpers
        private void ShowIcons()
        {
            ClearContainer();
            if (containerPref != null)
                containerPref.SetActive(true);
            BuildIcons();
            _contentState = ContentState.Icons;
        }

        private void ShowFrames()
        {
            ClearContainer();
            if (containerPref != null)
                containerPref.SetActive(true);
            BuildFrames();
            _contentState = ContentState.Frames;
        }

        private void HideContainer()
        {
            ClearContainer();
            if (containerPref != null && containerPref.activeSelf)
                containerPref.SetActive(false);
            _contentState = ContentState.Closed;
        }

        private void ClearContainer()
        {
            iconsEntries.Clear();
            framesEntries.Clear();
            if (container == null)
                return;
            for (int i = container.childCount - 1; i >= 0; --i)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Build icon entry UI elements into the container (assumes container active/cleared).
        /// </summary>
        private void BuildIcons()
        {
            if (container == null || iconsEntryPrefab == null || GameInstance.Singleton == null)
                return;

            foreach (IconItem item in GameInstance.Singleton.iconItems)
            {
                if (PlayerSave.IsIconPurchased(item.iconId) || item.isUnlocked)
                {
                    IconsEntry iconEntry = Instantiate(iconsEntryPrefab, container);
                    iconEntry.SetIconInfo(item.iconId);
                    iconEntry.icon.sprite = item.icon;
                    string iconNameTranslatedName = GetTranslatedString(item.iconNameTranslated, item.iconName, currentLang);
                    iconEntry.iconName.text = iconNameTranslatedName;
                    iconsEntries.Add(iconEntry);
                }
            }
        }

        /// <summary>
        /// Build frame entry UI elements into the container (assumes container active/cleared).
        /// </summary>
        private void BuildFrames()
        {
            if (container == null || framesEntryPrefab == null || GameInstance.Singleton == null)
                return;

            foreach (FrameItem item in GameInstance.Singleton.frameItems)
            {
                if (PlayerSave.IsFramePurchased(item.frameId) || item.isUnlocked)
                {
                    FramesEntry frameEntry = Instantiate(framesEntryPrefab, container);
                    frameEntry.SetFrameInfo(item.frameId);
                    string frameNameTranslatedName = GetTranslatedString(item.frameNameTranslated, item.frameName, currentLang);
                    frameEntry.icon.sprite = item.icon;
                    frameEntry.frameName.text = frameNameTranslatedName;
                    framesEntries.Add(frameEntry);
                }
            }
        }
        #endregion

        /*────────────────────────────────────────────────────────────────────────────*/
        #region Favorite Character
        public void LoadFavoriteCharacter()
        {
            int favouriteCharacterId = PlayerSave.GetFavouriteCharacter();
            int currentMasteryLevel = PlayerSave.GetCharacterMasteryLevel(favouriteCharacterId);
            int currMasteryExp = PlayerSave.GetCharacterCurrentMasteryExp(favouriteCharacterId);

            CharacterMasteryLevel masteryInfo = GameInstance.Singleton.GetMasteryLevel(currentMasteryLevel);
            string masteryTranslatedName = GetTranslatedString(masteryInfo.masteryNameTranslated, masteryInfo.masteryName, currentLang);

            if (masteryName != null) masteryName.text = masteryTranslatedName;
            if (masteryIcon != null) masteryIcon.sprite = masteryInfo.masteryIcon;

            if (currentMasteryLevel < GameInstance.Singleton.characterMastery.maxMasteryLevel)
            {
                int requiredMasteryExp = GameInstance.Singleton.GetMasteryExpForLevel(currentMasteryLevel);
                if (currentMasteryExp != null) currentMasteryExp.text = $"{currMasteryExp}/{requiredMasteryExp}";
                if (masteryProgressBar != null) masteryProgressBar.fillAmount = (float)currMasteryExp / requiredMasteryExp;
            }
            else
            {
                if (currentMasteryExp != null) currentMasteryExp.text = $"{currMasteryExp}/MAX";
                if (masteryProgressBar != null) masteryProgressBar.fillAmount = 1f;
            }

            // Rebuild the temporary model visual
            DestroyTemporaryModel();
            CharacterData cd = GetCharacterDataById(favouriteCharacterId);
            if (cd != null && tempCharacterContainer != null)
            {
                int skinIndex = PlayerSave.GetCharacterSkin(cd.characterId);
                if (cd.characterSkins != null && cd.characterSkins.Length > 0 && skinIndex >= 0 && skinIndex < cd.characterSkins.Length)
                {
                    CharacterSkin skin = cd.characterSkins[skinIndex];
                    if (skin.skinCharacterModel != null)
                        Instantiate(skin.skinCharacterModel, tempCharacterContainer);
                }
                else if (cd.characterModel != null)
                {
                    Instantiate(cd.characterModel, tempCharacterContainer);
                }
            }
        }
        #endregion

        /*────────────────────────────────────────────────────────────────────────────*/
        #region Name Change Flow
        public void OnClickChangeName()
        {
            if (GameInstance.Singleton.needTicket && MonetizationManager.GetCurrency(GameInstance.Singleton.changeNameTick) < GameInstance.Singleton.ticketsToChange)
            {
                DisplayStatusMessage(GetTranslatedString(notEnoughTicketsTranslated, notEnoughTickets, currentLang), Color.red);
                return;
            }
            if (changeNamePref != null)
                changeNamePref.SetActive(true);
        }

        public async void ProcessChangeName()
        {
            string newName = changeNameInput.text;
            RequestResult res = await BackendManager.Service.ChangePlayerNameAsync(newName);

            string msg;
            Color color;

            if (res.Success)
            {
                msg = GetTranslatedString(nameChangeSuccessTranslated, nameChangeSuccess, currentLang);
                color = Color.white;

                if (playerName != null)
                    playerName.text = newName;
                if (UIMainMenu.Singleton != null)
                    UIMainMenu.Singleton.LoadPlayerInfo();
                OnChangeNickname?.Invoke();
                if (changeNamePref != null)
                    changeNamePref.SetActive(false);
            }
            else
            {
                (msg, color) = res.Reason switch
                {
                    "0" => (GetTranslatedString(nameLengthErrorTranslated, nameLengthError, currentLang), Color.red),
                    "1" => (GetTranslatedString(notEnoughTicketsTranslated, notEnoughTickets, currentLang), Color.red),
                    "2" => (GetTranslatedString(nameAlreadyTakenTranslated, nameAlreadyTaken, currentLang), Color.red),
                    _ => (GetTranslatedString(nameChangeFailTranslated, nameChangeFail, currentLang), Color.red)
                };
            }

            DisplayStatusMessage(msg, color);
        }
        #endregion

        /*────────────────────────────────────────────────────────────────────────────*/
        #region Utility Helpers
        private CharacterData GetCharacterDataById(int characterId)
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.characterData == null)
                return null;

            foreach (CharacterData item in GameInstance.Singleton.characterData)
            {
                if (item.characterId == characterId)
                    return item;
            }
            return null;
        }

        private void DestroyTemporaryModel()
        {
            if (tempCharacterContainer == null) return;
            for (int i = tempCharacterContainer.childCount - 1; i >= 0; i--)
                Destroy(tempCharacterContainer.GetChild(i).gameObject);
        }

        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var translation in translations)
                {
                    if (!string.IsNullOrEmpty(translation.LanguageId) &&
                        translation.LanguageId.Equals(currentLang) &&
                        !string.IsNullOrEmpty(translation.Translate))
                    {
                        return translation.Translate;
                    }
                }
            }
            return fallback;
        }

        public void DisplayStatusMessage(string message, Color color)
        {
            if (statusText == null)
                return;
            statusText.text = message;
            statusText.color = color;
            StartCoroutine(HideStatusMessageAfterDelay(statusText, 2f));
        }

        private IEnumerator HideStatusMessageAfterDelay(TextMeshProUGUI txt, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (txt != null)
                txt.text = string.Empty;
        }

        private void SetPlayerIcon(string iconId)
        {
            if (playerIcon == null || GameInstance.Singleton == null)
                return;
            foreach (IconItem item in GameInstance.Singleton.iconItems)
            {
                if (item.iconId == iconId)
                {
                    playerIcon.sprite = item.icon;
                    break;
                }
            }
        }

        private void SetPlayerFrame(string frameId)
        {
            if (playerFrame == null || GameInstance.Singleton == null)
                return;
            foreach (FrameItem item in GameInstance.Singleton.frameItems)
            {
                if (item.frameId == frameId)
                {
                    playerFrame.sprite = item.icon;
                    break;
                }
            }
        }
        #endregion
    }
}
