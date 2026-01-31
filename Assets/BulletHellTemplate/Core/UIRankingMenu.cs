using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class UIRankingMenu : MonoBehaviour
    {
        [Header("Prefabs and Containers")]
        [Tooltip("Prefab for each ranking entry.")]
        public RankingEntry rankingEntryPrefab;

        [Tooltip("Container for ranking entries.")]
        public Transform container;

        [Tooltip("Container for displaying the favorite character.")]
        public Transform characterFavouriteContainer;

        [Header("Selected Player UI Elements")]
        [Tooltip("Text displaying the selected player's nickname.")]
        public TextMeshProUGUI selectedPlayerNickname;

        [Tooltip("Text displaying the selected player's rank.")]
        public TextMeshProUGUI selectedPlayerRank;

        [Tooltip("Image displaying the selected player's icon.")]
        public Image selectedPlayerIcon;

        [Tooltip("Image displaying the selected player's frame.")]
        public Image selectedPlayerFrame;

        [Header("Other UI Elements")]
        [Tooltip("Text displaying the current player's rank.")]
        public TextMeshProUGUI myRank;

        public TextMeshProUGUI localPlayerName;
        public Image localPlayerIcon;
        public Image localPlayerFrame;

        [Tooltip("Button to refresh the rankings.")]
        public Button refreshButton;

        [Tooltip("Image used to display cooldown on the refresh button.")]
        public Image cooldownImage;

        [Header("UI Translates Messages")]
        public string yourRank = "Your Rank: {0}";
        public NameTranslatedByLanguage[] yourRankTranslated;

        public string rankNotAvaliable = "Rank not available";
        public NameTranslatedByLanguage[] rankNotAvaliableTranslated;

        public string rank = "Rank:";
        public NameTranslatedByLanguage[] rankTranslated;

       [Header("Events")]
        [Tooltip("Event invoked when the menu is opened.")]
        public UnityEvent OnOpenMenu;

        [Tooltip("Event invoked when the menu is closed.")]
        public UnityEvent OnCloseMenu;


        public static UIRankingMenu Singleton;
        private string currentLang;
        private int localRanking;

        private const float RANK_FETCH_MIN_INTERVAL = 60f; 
        private bool isFetchingRankings = false;

        private const string PREF_RANK_LAST_FETCH_UNIX = "rank_last_fetch_unix"; // PlayerPrefs
        private static List<Dictionary<string, object>> s_cachedTopPlayers = new();
        private static int s_cachedLocalRank = -1;
        private static long s_lastFetchUnix = -1; // seconds Unix

        /// <summary>
        /// List of all instantiated ranking entries.
        /// </summary>
        private List<RankingEntry> rankingEntries = new List<RankingEntry>();

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
        }

        private static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// Selects the first ranking entry once everything is loaded.
        /// </summary>
        private void OnEnable()
        {
            OnOpenMenu.Invoke();
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            LoadLocalPlayerInfo();
            bool hadCache = BuildUIFromCache();
            LoadRankings(force: !hadCache);

            if (rankingEntries.Count > 0)
            {
                OnRankingEntrySelected(rankingEntries[0]);
            }
        }

        private void OnDisable()
        {
            OnCloseMenu.Invoke();            
        }

        public void OnClickRefreshRankings()
        {
            LoadRankings(force: false);
            ResetCooldown();
        }

        /// <summary>
        /// Resets the cooldown for the refresh button.
        /// </summary>
        public void ResetCooldown()
        {
            StartCoroutine(StartCooldown());
        }

        /// <summary>
        /// Loads the rankings and displays the top players, sorted by score in descending order from memory cache.
        /// </summary>
        private bool BuildUIFromCache()
        {
            if (s_cachedTopPlayers == null || s_cachedTopPlayers.Count == 0)
                return false;

            foreach (Transform child in container) Destroy(child.gameObject);
            rankingEntries.Clear();

            var sorted = s_cachedTopPlayers
                .Select(pd =>
                {
                    long score = 0;
                    if (pd.TryGetValue("score", out var sObj))
                        long.TryParse(sObj?.ToString(), out score);

                    string playerName = pd.TryGetValue("PlayerName", out var nObj)
                        ? nObj?.ToString() ?? "Unknown"
                        : "Unknown";

                    return new { PlayerData = pd, Score = score, PlayerName = playerName };
                })
                .OrderByDescending(p => p.Score)
                .Take(20)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                var p = sorted[i];
                var pd = p.PlayerData;
                RankingEntry entry = Instantiate(rankingEntryPrefab, container);

                string scoreStr = p.Score.ToString();
                string iconId = pd.TryGetValue("PlayerIcon", out var icObj) ? icObj?.ToString() ?? "defaultIconId" : "defaultIconId";
                string frameId = pd.TryGetValue("PlayerFrame", out var frObj) ? frObj?.ToString() ?? "defaultFrameId" : "defaultFrameId";
                int favChar = pd.TryGetValue("PlayerCharacterFavourite", out var favObj) && int.TryParse(favObj?.ToString(), out var favParsed)
                                    ? favParsed : 0;

                entry.SetRankingInfo(
                    _playerName: p.PlayerName,
                    _score: scoreStr,
                    rankingPosition: i + 1,
                    playerIconId: GetPlayerIcon(iconId),
                    playerFrameId: GetPlayerFrame(frameId),
                    playerCharacterFavourite: favChar
                );
                rankingEntries.Add(entry);
            }

            localRanking = (s_cachedLocalRank > 0) ? s_cachedLocalRank : localRanking;
            UpdateMyRankLabel();

            if (rankingEntries.Count > 0)
                OnRankingEntrySelected(rankingEntries[0]);

            return true;
        }

        /// <summary>
        /// Loads the rankings and displays the top players, sorted by score in descending order.
        /// </summary>
        public async void LoadRankings(bool force = false)
        {
            long lastUnix = LoadLastFetchUnix();
            long nowUnix = NowUnix();
            long elapsed = (lastUnix >= 0) ? (nowUnix - lastUnix) : long.MaxValue;

            if (!force && elapsed < (long)RANK_FETCH_MIN_INTERVAL)
            {
                long remain = (long)RANK_FETCH_MIN_INTERVAL - elapsed;
                Debug.Log($"[Ranking] Refresh throttled. Try again in {remain}s.");
                return;
            }

            if (isFetchingRankings)
            {
                Debug.Log("[Ranking] Refresh already in progress.");
                return;
            }

            isFetchingRankings = true;

            ResetCooldown();

            List<Dictionary<string, object>> topPlayers;
            try
            {
                topPlayers = await BackendManager.Service.GetTopPlayersAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Ranking] Fetch failed: {ex.Message}");
                isFetchingRankings = false;
                return; 
            }

            s_cachedTopPlayers = topPlayers ?? new();
            s_cachedLocalRank = await BackendManager.Service.GetPlayerRankAsync();

            SaveLastFetchUnix(nowUnix);
            BuildUIFromCache();

            isFetchingRankings = false;
        }


        public void LoadLocalPlayerInfo()
        {
            string _playerName = PlayerSave.GetPlayerName();
            string _playerIcon = PlayerSave.GetPlayerIcon();
            string _playerFrame = PlayerSave.GetPlayerFrame();

            localPlayerName.text = _playerName;
            localPlayerIcon.sprite = GetPlayerIcon(_playerIcon);
            localPlayerFrame.sprite = GetPlayerFrame(_playerFrame);

            UpdateMyRankLabel();
        }

        private void UpdateMyRankLabel()
        {
            string _yourRankTranslated = GetTranslatedString(yourRankTranslated, yourRank, currentLang);
            string _rankNotEvaliableTranslated = GetTranslatedString(rankNotAvaliableTranslated, rankNotAvaliable, currentLang);

            myRank.text = (localRanking > 0)
                ? string.Format(_yourRankTranslated, localRanking)
                : _rankNotEvaliableTranslated;
        }

        /// <summary>
        /// Handles the selection of a ranking entry.
        /// Activates the selected image and updates selected player variables.
        /// </summary>
        /// <param name="selectedEntry">The ranking entry that was selected.</param>
        public void OnRankingEntrySelected(RankingEntry selectedEntry)
        {
            // Deactivate 'selected' image on all entries
            foreach (var entry in rankingEntries)
            {
                entry.SetSelected(false);
            }

            // Activate 'selected' image on the selected entry
            selectedEntry.SetSelected(true);

            // Update selected player variables
            selectedPlayerNickname.text = selectedEntry.playerName.text;
            string stringRankTranslated = GetTranslatedString(rankTranslated, rank, currentLang);
            selectedPlayerRank.text = $"{stringRankTranslated} {selectedEntry.ranking.text}";
            selectedPlayerIcon.sprite = selectedEntry.playerIcon.sprite;
            selectedPlayerFrame.sprite = selectedEntry.playerFrame.sprite;

            // Display the favorite character
            DisplayFavouriteCharacter(selectedEntry.characterFavourite);
        }

        /// <summary>
        /// Instantiates the favorite character in the specified containerPassItems.
        /// </summary>
        /// <param name="characterId">The ID of the character to instantiate.</param>
        public void DisplayFavouriteCharacter(int characterId)
        {
            // Clear existing favorite character display
            foreach (Transform child in characterFavouriteContainer)
            {
                Destroy(child.gameObject);
            }

            // Find the character passEntryPrefab by ID and instantiate it
            CharacterData characterPrefab = null;
            foreach (var character in GameInstance.Singleton.characterData)
            {
                if (character.characterId == characterId)
                {
                    characterPrefab = character;
                    break;
                }
            }

            if (characterPrefab != null)
            {
                Instantiate(characterPrefab.characterModel, characterFavouriteContainer);
            }
            else
            {
                Debug.LogWarning("Character with ID " + characterId + " not found.");
            }

            if (DragRotateAndHit.Singleton != null)
            {
                DragRotateAndHit.Singleton.UpdateCharacter();
            }
        }

        private static long LoadLastFetchUnix()
        {
            if (s_lastFetchUnix >= 0) return s_lastFetchUnix;
            if (SecurePrefs.HasKey(PREF_RANK_LAST_FETCH_UNIX))
            {
                var str = SecurePrefs.GetDecryptedString(PREF_RANK_LAST_FETCH_UNIX, "-1");
                long.TryParse(str, out s_lastFetchUnix);
            }
            return s_lastFetchUnix;
        }

        private static void SaveLastFetchUnix(long unix)
        {
            s_lastFetchUnix = unix;
            PlayerPrefs.SetString(PREF_RANK_LAST_FETCH_UNIX, unix.ToString());
            PlayerPrefs.Save();
        }

        public Sprite GetPlayerIcon(string iconId)
        {

            foreach (var icon in GameInstance.Singleton.iconItems)
            {
                if (icon.iconId == iconId)
                {                   
                    return icon.icon; ;
                }
            }
            return null;
        }

        public Sprite GetPlayerFrame(string frameId)
        {

            foreach (var frame in GameInstance.Singleton.frameItems)
            {
                if (frame.frameId == frameId)
                {
                    return frame.icon; ;
                }
            }
            return null;
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

        /// <summary>
        /// Starts the cooldown for the refresh button and updates the UI.
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartCooldown()
        {
            refreshButton.interactable = false;

            float start = Time.unscaledTime;
            const float UI_COOLDOWN = 5f;
            while (Time.unscaledTime - start < UI_COOLDOWN)
            {
                float t = (Time.unscaledTime - start) / UI_COOLDOWN;
                cooldownImage.fillAmount = 1f - t;
                yield return null;
            }
            cooldownImage.fillAmount = 0f;
            refreshButton.interactable = true;
        }

    }
}
