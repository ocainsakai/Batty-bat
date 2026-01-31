using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace BulletHellTemplate
{
    /// <summary>
    /// Popup that displays pending "first completion" map rewards and lets the player redeem them.
    /// 
    /// Relies on RewardManagerPopup for pending + claimed state.
    /// When the player clicks Redeem, we call the authoritative server endpoint
    /// (via BackendManager.Service.ApplyMapRewardsAsync); on success we mark claimed locally
    /// and update UI/economy using the server echo already applied in the backend service extension.
    /// </summary>
    public sealed class UIRewardManagerPopup : MonoBehaviour
    {
        public static UIRewardManagerPopup Singleton;

        [Header("UI Components")]
        [Tooltip("Root popup GameObject.")]
        public GameObject rewardPopupMenu;

        [Tooltip("Prefab used for each reward entry row.")]
        public RewardPopupEntry rewardEntryPrefab;

        [Tooltip("Parent container that holds spawned reward entry rows.")]
        public Transform entryContainer;

        [Tooltip("Button that redeems all pending map rewards.")]
        public Button redeemButton;

        [Tooltip("Text field used to display temporary error messages.")]
        public TextMeshProUGUI errorText;

        [Header("Icons for EXP types")]
        [Tooltip("Icon representing Account EXP.")]
        public Sprite ExpAccountIcon;
        [Tooltip("Icon representing Mastery EXP.")]
        public Sprite ExpMasteryIcon;
        [Tooltip("Icon representing Character EXP.")]
        public Sprite ExpCharacterIcon;

        [Header("Settings")]
        [Tooltip("Seconds to display transient error messages.")]
        public float errorDisplayTime = 1.5f;

        [Tooltip("Fallback error prefix if translation unavailable.")]
        public string errorMessage = "Error loading rewards:";

        [Tooltip("Translated versions of the fallback error message.")]
        public NameTranslatedByLanguage[] errorMessageTranslated;

        private readonly List<MapInfoData> _pendingRewards = new();
        private string _currentLang;

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

        private void Start()
        {
            _currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            if (rewardPopupMenu != null)
                rewardPopupMenu.SetActive(false);

            ShowPendingRewards();
        }

        /// <summary>
        /// Query pending maps from RewardManagerPopup and rebuild the popup UI.
        /// </summary>
        public void ShowPendingRewards()
        {
            _pendingRewards.Clear();

            var unclaimed = RewardManagerPopup.Singleton?.GetLocalUnclaimedMaps();
            if (unclaimed == null || unclaimed.Count == 0)
            {
                HidePopup();
                return;
            }

            foreach (var mapId in unclaimed)
            {
                var mapData = GetMapDataById(mapId);
                if (IsValidRewardMap(mapData))
                    _pendingRewards.Add(mapData);
            }

            if (_pendingRewards.Count == 0)
            {
                HidePopup();
                return;
            }

            ShowPopup();
            PopulateRewardEntries();
        }

        private void ShowPopup()
        {
            if (rewardPopupMenu != null)
                rewardPopupMenu.SetActive(true);
        }

        private void HidePopup()
        {
            if (rewardPopupMenu != null)
                rewardPopupMenu.SetActive(false);
        }

        /// <summary>
        /// Builds the UI rows for all pending rewards.
        /// </summary>
        private void PopulateRewardEntries()
        {
            ClearEntries();

            foreach (var mapData in _pendingRewards)
            {
                // Currency / EXP rewards
                if (mapData.WinMapRewards != null)
                {
                    foreach (var r in mapData.WinMapRewards)
                    {
                        if (r.currency != null)
                            CreateEntry(r.currency.icon, r.amount.ToString());

                        if (r.accountExp > 0)
                            CreateEntry(ExpAccountIcon, r.accountExp.ToString());

                        if (r.characterExp > 0)
                            CreateEntry(ExpCharacterIcon, r.characterExp.ToString());

                        if (r.characterMasteryAmount > 0)
                            CreateEntry(ExpMasteryIcon, r.characterMasteryAmount.ToString());
                    }
                }

                // Special reward
                if (mapData.rewardType != MapRewardType.None)
                    CreateSpecialRewardEntry(mapData);
            }

            if (redeemButton != null)
            {
                redeemButton.onClick.RemoveAllListeners();
                redeemButton.onClick.AddListener(() => ClaimAllRewards().Forget());
            }
        }

        private void ClearEntries()
        {
            if (entryContainer == null) return;
            foreach (Transform child in entryContainer)
                Destroy(child.gameObject);
        }

        private void CreateEntry(Sprite icon, string amount)
        {
            if (rewardEntryPrefab == null || entryContainer == null) return;
            var entry = Instantiate(rewardEntryPrefab, entryContainer);
            entry.Setup(icon, amount);
        }

        private void CreateSpecialRewardEntry(MapInfoData mapData)
        {
            switch (mapData.rewardType)
            {
                case MapRewardType.Icon when mapData.iconItem != null:
                    CreateEntry(mapData.iconItem.icon, "1");
                    break;
                case MapRewardType.Frame when mapData.frameItem != null:
                    CreateEntry(mapData.frameItem.icon, "1");
                    break;
                case MapRewardType.Character when mapData.characterData != null:
                    CreateEntry(mapData.characterData.icon, "1");
                    break;
                case MapRewardType.InventoryItem when mapData.inventoryItem != null:
                    CreateEntry(mapData.inventoryItem.itemIcon, "1");
                    break;
            }
        }

        /// <summary>
        /// Redeems all pending map rewards sequentially (await each to avoid request burst).
        /// </summary>
        private async UniTaskVoid ClaimAllRewards()
        {
            if (redeemButton != null)
                redeemButton.interactable = false;

            foreach (var mapData in _pendingRewards)
                await ClaimSingleMapRewardAsync(mapData);

            // refresh after claims
            HidePopup();
            if (redeemButton != null)
                redeemButton.interactable = true;

            UIMainMenu.Singleton?.LoadPlayerInfo();
            UIMainMenu.Singleton?.LoadCharacterInfo();
        }

        /// <summary>
        /// Claim a single map reward from the server; mark local on success.
        /// </summary>
        private async UniTask ClaimSingleMapRewardAsync(MapInfoData mapData)
        {
            if (mapData == null) return;

            int characterId = PlayerSave.GetSelectedCharacter();

            var result = await BackendManager.Service.ApplyMapRewardsAsync(
                new MapCompletionRewardData
                {
                    mapId = mapData.mapId,
                    characterId = characterId
                });

            if (!result.Success)
            {
                Debug.LogWarning($"[UIRewardManagerPopup] Map {mapData.mapId} claim failed (code:{result.Reason}).");
                ShowError($"Reward {mapData.mapId} failed ({result.Reason})");
                return;
            }

            RewardManagerPopup.Singleton?.MarkRewardClaimed(mapData.mapId);
        }

        private bool IsValidRewardMap(MapInfoData mapData)
        {
            if (mapData == null) return false;
            bool hasList = mapData.WinMapRewards != null && mapData.WinMapRewards.Count > 0;
            bool hasSpecial = mapData.rewardType != MapRewardType.None;
            return mapData.isRewardOnCompleteFirstTime && (hasList || hasSpecial);
        }

        private MapInfoData GetMapDataById(int mapId)
        {
            var maps = GameInstance.Singleton?.mapInfoData;
            if (maps == null) return null;
            return Array.Find(maps, m => m != null && m.mapId == mapId);
        }

        private void ShowError(string message)
        {
            if (errorText == null) return;
            errorText.text = message;
            CancelInvoke(nameof(ClearError));
            Invoke(nameof(ClearError), errorDisplayTime);
        }

        private void ClearError()
        {
            if (errorText != null)
                errorText.text = string.Empty;
        }

        public string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }
    }
}
