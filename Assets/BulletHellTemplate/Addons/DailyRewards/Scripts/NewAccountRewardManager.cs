using System;
using UnityEngine;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    /// <summary>
    /// Displays and claims **New Player Rewards** with a *one-claim-per-day* rule.
    /// 
    /// Design:
    /// • Rewards unlock over time based on days since account creation.
    /// • Player may fall behind (missed logins) but can only claim **one** unclaimed day per calendar day.
    ///   (Catch-up over multiple days.)
    /// • UI is built from locally cached <see cref="NewPlayerRewardsData"/>.
    /// • On claim click → local validation → server validation → local cache update → UI refresh.
    /// </summary>
    public sealed class NewPlayerRewardManager : MonoBehaviour
    {
        /*────────────────────────── Inspector ──────────────────────────*/

        [Header("UI Prefabs & Containers")]
        [Tooltip("Prefab used to display each new-player reward entry.")]
        public RewardEntry rewardEntryPrefab;

        [Tooltip("Parent transform that will hold all spawned new-player reward entries.")]
        public Transform rewardsContainer;

        [Tooltip("Popup shown when a new-player reward is successfully claimed.")]
        public RewardPopup rewardPopup;

        [Header("Settings")]
        [Tooltip("Optional hard cap for new-player reward days. If <=0, GameInstance data length is used.")]
        public int totalRewardDaysOverride = 0;

        /*────────────────────────── Private State ──────────────────────*/

        private NewPlayerRewardsData localNewPlayerData;
        private RewardItem[] rewardItems;
        private string currentLang;

        // Cached per-enable state
        private bool _alreadyClaimedToday;
        private int _daysSinceCreation;
        private int _nextUnclaimedIndex;   // sequential catch-up target

        /*────────────────────────── Unity Events ───────────────────────*/

        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            rewardItems = GameInstance.Singleton.newPlayerRewardItems;

            // Pull local cache
            localNewPlayerData = PlayerSave.GetNewPlayerRewardsLocal();

            // Days since account creation
            if (localNewPlayerData.accountCreationDate != DateTime.MinValue)
            {
                _daysSinceCreation = Mathf.Max(
                    0,
                    (int)(DateTime.Now.Date - localNewPlayerData.accountCreationDate.Date).TotalDays
                );
            }
            else
            {
                _daysSinceCreation = 0;
            }

            // 1/day gating: already claimed today?
            _alreadyClaimedToday = localNewPlayerData.lastClaimDate != DateTime.MinValue &&
                                   localNewPlayerData.lastClaimDate.Date == DateTime.Now.Date;

            // Next unclaimed reward index (sequential catch-up)
            _nextUnclaimedIndex = localNewPlayerData.claimedRewards.Count;

            BuildUI();
        }

        /*────────────────────────── UI Build ───────────────────────────*/

        /// <summary>
        /// Spawns UI entries for each configured new-player reward and enables the claim
        /// button only on the **next unclaimed** day (if unlocked by time and not claimed today).
        /// </summary>
        private void BuildUI()
        {
            // Clear previous
            if (rewardsContainer != null)
            {
                for (int i = rewardsContainer.childCount - 1; i >= 0; --i)
                    Destroy(rewardsContainer.GetChild(i).gameObject);
            }

            if (rewardItems == null || rewardItems.Length == 0)
            {
                Debug.LogWarning("[NewPlayerRewardManager] No new-player rewards configured.");
                return;
            }

            int totalCap = totalRewardDaysOverride > 0
                ? Mathf.Min(totalRewardDaysOverride, rewardItems.Length)
                : rewardItems.Length;

            for (int i = 0; i < totalCap; i++)
            {
                var def = rewardItems[i];
                RewardEntry entry = Instantiate(rewardEntryPrefab, rewardsContainer);

                // Localized UI text
                string title = GetTranslatedString(def.titleTranslated, def.title, currentLang);
                string description = GetTranslatedString(def.descriptionTranslated, def.description, currentLang);

                entry.Setup(
                    def.icon,
                    title,
                    description,
                    $"Day {i + 1}",
                    i + 1
                );

                // Already claimed?
                if (localNewPlayerData.claimedRewards.Contains(i))
                {
                    entry.SetClaimed();
                    continue;
                }

                // Locked because already claimed today's new-player reward?
                if (_alreadyClaimedToday)
                {
                    entry.SetLocked();
                    continue;
                }

                // Only the NEXT unclaimed reward can be claimed today (catch-up)
                if (i == _nextUnclaimedIndex && i <= _daysSinceCreation)
                {
                    int capture = i;
                    entry.EnableClaimButton(() => OnClaimButtonPressed(capture));
                }
                else
                {
                    entry.SetLocked();
                }
            }
        }

        /*────────────────────────── Claim Flow ─────────────────────────*/

        /// <summary>
        /// Triggered by UI button; runs local validation then performs server claim.
        /// </summary>
        private async void OnClaimButtonPressed(int dayIndex)
        {
            if (!LocalCanClaim(dayIndex, out var failCode))
            {
                HandleClaimFail(failCode, dayIndex, isPreServer: true);
                return;
            }

            // Build a BattlePass mirror so we can reuse reward-apply logic
            RewardItem rewardDef = rewardItems[dayIndex];
            BattlePassItem bpItem = BuildBattlePassMirror(rewardDef);

            // Server validation + grant
            RequestResult result = await BackendManager.Service.ClaimNewPlayerRewardAsync(dayIndex, bpItem);

            if (!result.Success)
            {
                HandleClaimFail(result.Reason, dayIndex, isPreServer: false);
                return;
            }

            // Update local cache
            if (!localNewPlayerData.claimedRewards.Contains(dayIndex))
                localNewPlayerData.claimedRewards.Add(dayIndex);
            localNewPlayerData.lastClaimDate = DateTime.Now.Date;
            PlayerSave.SetNewPlayerRewardsLocal(localNewPlayerData);

            // Popup
            if (rewardPopup != null)
            {
                string title = GetTranslatedString(rewardDef.titleTranslated, rewardDef.title, currentLang);
                string description = GetTranslatedString(rewardDef.descriptionTranslated, rewardDef.description, currentLang);
                rewardPopup.Setup(rewardDef.icon, title, description);
            }

            // Refresh UI
            OnEnable();
        }

        /// <summary>
        /// Minimal local gating for *one-per-day* catch-up.
        /// failCode: "0" (already) / "1" (not available) / null (ok).
        /// </summary>
        private bool LocalCanClaim(int dayIndex, out string failCode)
        {
            failCode = null;

            int totalCap = totalRewardDaysOverride > 0
                ? Mathf.Min(totalRewardDaysOverride, rewardItems.Length)
                : rewardItems.Length;

            // Range
            if (dayIndex < 0 || dayIndex >= totalCap)
            {
                failCode = "1";
                return false;
            }

            // Already claimed?
            if (localNewPlayerData.claimedRewards.Contains(dayIndex))
            {
                failCode = "0";
                return false;
            }

            // Already claimed a new-player reward today?
            if (localNewPlayerData.lastClaimDate != DateTime.MinValue &&
                localNewPlayerData.lastClaimDate.Date == DateTime.Now.Date)
            {
                failCode = "0"; // already today
                return false;
            }

            // Sequential catch-up: must be the next unclaimed index
            int expected = localNewPlayerData.claimedRewards.Count;
            if (dayIndex != expected)
            {
                failCode = "1";
                return false;
            }

            // Must be unlocked by time
            if (dayIndex > _daysSinceCreation)
            {
                failCode = "1";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles failed claim attempts (local or server) with consistent logging.
        /// </summary>
        private void HandleClaimFail(string reasonCode, int dayIndex, bool isPreServer)
        {
            string prefix = isPreServer ? "[NewPlayerRewardManager][Local]" : "[NewPlayerRewardManager][Server]";
            switch (reasonCode)
            {
                case "0":
                    Debug.LogWarning($"{prefix} Day {dayIndex + 1} already claimed.");
                    break;
                case "1":
                    Debug.LogWarning($"{prefix} Day {dayIndex + 1} not available yet.");
                    break;
                default:
                    Debug.LogWarning($"{prefix} Claim failed (code:{reasonCode}).");
                    break;
            }
        }

        /*────────────────────────── Reward Construction ─────────────────*/

        /// <summary>
        /// Builds a minimal <see cref="BattlePassItem"/> mirror from a <see cref="RewardItem"/> definition.
        /// Used only to route through shared reward‑apply code paths.
        /// </summary>
        private BattlePassItem BuildBattlePassMirror(RewardItem reward)
        {
            BattlePassItem temp = ScriptableObject.CreateInstance<BattlePassItem>();
            temp.passId = "NewPlayerReward_" + reward.rewardId;
            temp.itemTitle = reward.title;
            temp.itemDescription = reward.description ?? "New-player reward.";
            temp.itemIcon = reward.icon;
            temp.rewardTier = BattlePassItem.RewardTier.Free;

            switch (reward.rewardType)
            {
                case RewardType.Currency:
                    temp.rewardType = BattlePassItem.RewardType.CurrencyReward;
                    temp.currencyReward = new CurrencyReward
                    {
                        currency = reward.currencyRewards[0],
                        amount = reward.amount
                    };
                    break;

                case RewardType.Icon:
                    temp.rewardType = BattlePassItem.RewardType.IconReward;
                    temp.iconReward = reward.iconRewards[0];
                    break;

                case RewardType.Frame:
                    temp.rewardType = BattlePassItem.RewardType.FrameReward;
                    temp.frameReward = reward.frameRewards[0];
                    break;

                case RewardType.Character:
                    temp.rewardType = BattlePassItem.RewardType.CharacterReward;
                    temp.characterData = new CharacterData[] { reward.characterRewards[0] };
                    break;

                case RewardType.InventoryItem:
                    temp.rewardType = BattlePassItem.RewardType.InventoryItemReward;
                    temp.inventoryItems = new InventoryItem[] { reward.inventoryItems[0] };
                    break;
            }
            return temp;
        }

        /*────────────────────────── Helpers ─────────────────────────────*/

        /// <summary>
        /// Returns a translated string (title) or the fallback if not found.
        /// </summary>
        public string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string lang)
        {
            if (translations != null)
            {
                foreach (var t in translations)
                {
                    if (!string.IsNullOrEmpty(t.LanguageId) &&
                        t.LanguageId.Equals(lang, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(t.Translate))
                        return t.Translate;
                }
            }
            return fallback;
        }

        /// <summary>
        /// Returns a translated string (description) or the fallback if not found.
        /// </summary>
        public string GetTranslatedString(DescriptionTranslatedByLanguage[] translations, string fallback, string lang)
        {
            if (translations != null)
            {
                foreach (var t in translations)
                {
                    if (!string.IsNullOrEmpty(t.LanguageId) &&
                        t.LanguageId.Equals(lang, StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrEmpty(t.Translate))
                        return t.Translate;
                }
            }
            return fallback;
        }
    }
}
