using System;
using UnityEngine;
using static BulletHellTemplate.PlayerSave;

namespace BulletHellTemplate
{
    /// <summary>
    /// Displays and claims **Daily Rewards** using a *local-first, server-confirmed* flow.
    /// 
    /// Workflow:
    /// 1. UI is built from locally cached <see cref="DailyRewardsData"/> (PlayerSave).
    /// 2. When the player presses "Claim", minimal local validation runs (already claimed? already today? correct sequence?).
    /// 3. If local checks pass, a server claim is sent via <see cref="BackendManager.Service"/>.
    /// 4. On server success, reward is applied (inside the backend claim extension) and local cache is updated.
    /// 5. UI refreshes.
    /// 
    /// This approach reduces server calls while keeping the server authoritative over the final grant.
    /// </summary>
    public sealed class DailyRewardManager : MonoBehaviour
    {
        /*────────────────────────── Inspector ──────────────────────────*/

        [Header("UI Prefabs & Containers")]
        [Tooltip("Prefab used to display each daily reward entry.")]
        public RewardEntry rewardEntryPrefab;

        [Tooltip("Parent transform that will hold all spawned daily reward entries.")]
        public Transform rewardsContainer;

        [Tooltip("Popup shown when a reward is successfully claimed.")]
        public RewardPopup rewardPopup;

        /*────────────────────────── Private State ──────────────────────*/

        private DailyRewardsData localDailyData;
        private RewardItem[] rewardItems;
        private string currentLang;

        // Cached flags used when building UI
        private bool _alreadyClaimedToday;
        private int _nextIndex;              // index expected to be claimed next (sequential)
        private int _daysSinceFirstClaim;    // used only for visual gating

        /*────────────────────────── Unity Events ───────────────────────*/

        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            rewardItems = GameInstance.Singleton.dailyRewardItems;

            // Pull local cache
            localDailyData = PlayerSave.GetDailyRewardsLocal();

            // Optionally perform a lazy local reset if the cycle completed AND at least 1 new day has passed.
            TryLocalCycleResetIfNeeded();

            // Precompute visual gating values
            _alreadyClaimedToday = localDailyData.lastClaimDate != DateTime.MinValue &&
                                   localDailyData.lastClaimDate.Date == DateTime.Now.Date;

            _nextIndex = localDailyData.claimedRewards.Count;

            if (localDailyData.firstClaimDate != DateTime.MinValue)
            {
                _daysSinceFirstClaim = Mathf.Max(
                    0,
                    (int)(DateTime.Now.Date - localDailyData.firstClaimDate.Date).TotalDays
                );
            }
            else
            {
                _daysSinceFirstClaim = 0;
            }

            BuildUI();
        }

        /*────────────────────────── UI Build ───────────────────────────*/

        /// <summary>
        /// Spawns one entry per configured daily reward and wires up claim buttons
        /// according to the current local state.
        /// </summary>
        private void BuildUI()
        {
            // Clear previous children
            if (rewardsContainer != null)
            {
                for (int i = rewardsContainer.childCount - 1; i >= 0; --i)
                    Destroy(rewardsContainer.GetChild(i).gameObject);
            }

            if (rewardItems == null || rewardItems.Length == 0)
            {
                Debug.LogWarning("[DailyRewardManager] No daily rewards configured.");
                return;
            }

            for (int i = 0; i < rewardItems.Length; i++)
            {
                var def = rewardItems[i];
                RewardEntry entry = Instantiate(rewardEntryPrefab, rewardsContainer);

                // Localized display strings
                string title = GetTranslatedString(def.titleTranslated, def.title, currentLang);
                string description = GetTranslatedString(def.descriptionTranslated, def.description, currentLang);

                // Display day label (1-based for player clarity)
                entry.Setup(
                    def.icon,
                    title,
                    description,
                    $"Day {i + 1}",
                    i + 1
                );

                // State assignment
                if (localDailyData.claimedRewards.Contains(i))
                {
                    entry.SetClaimed();
                    continue;
                }

                // If already claimed today, everything else is locked
                if (_alreadyClaimedToday)
                {
                    entry.SetLocked();
                    continue;
                }

                // Sequential gating: allow claim only for the next expected index
                if (i == _nextIndex && i <= _daysSinceFirstClaim)
                {
                    int capture = i; // closure
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
        /// Called by UI button; runs local validation and (if passed) performs an async server claim.
        /// </summary>
        private async void OnClaimButtonPressed(int dayIndex)
        {
            // Fast local validation; reduces unnecessary server requests.
            if (!LocalCanClaim(dayIndex, out var failCode))
            {
                HandleClaimFail(failCode, dayIndex, isPreServer: true);
                return;
            }

            // Build a temporary BattlePassItem to describe reward for local application & popup visuals.
            RewardItem rewardDef = rewardItems[dayIndex];
            BattlePassItem bpItem = BuildBattlePassMirror(rewardDef);

            // Perform server-authorized claim.
            RequestResult result = await BackendManager.Service.ClaimDailyRewardAsync(dayIndex, bpItem);

            if (!result.Success)
            {
                HandleClaimFail(result.Reason, dayIndex, isPreServer: false);
                return;
            }

            // Update local cache on success
            if (!localDailyData.claimedRewards.Contains(dayIndex))
                localDailyData.claimedRewards.Add(dayIndex);

            if (localDailyData.claimedRewards.Count == 1)
                localDailyData.firstClaimDate = DateTime.Now.Date;

            localDailyData.lastClaimDate = DateTime.Now.Date;
            PlayerSave.SetDailyRewardsLocal(localDailyData);

            // Show popup (optional)
            if (rewardPopup != null)
            {
                string title = GetTranslatedString(rewardDef.titleTranslated, rewardDef.title, currentLang);
                string description = GetTranslatedString(rewardDef.descriptionTranslated, rewardDef.description, currentLang);
                rewardPopup.Setup(rewardDef.icon, title, description);
            }

            // Rebuild UI with updated data
            OnEnable(); // quick refresh
        }

        /// <summary>
        /// Minimal local validation: already claimed? already claimed today? correct sequence?
        /// failCode: "0" (already) / "1" (not available) / null (ok).
        /// </summary>
        private bool LocalCanClaim(int dayIndex, out string failCode)
        {
            failCode = null;

            int total = rewardItems?.Length ?? 0;
            if (dayIndex < 0 || dayIndex >= total)
            {
                failCode = "1";
                return false;
            }

            if (localDailyData.claimedRewards.Contains(dayIndex))
            {
                failCode = "0";
                return false;
            }

            if (localDailyData.lastClaimDate != DateTime.MinValue &&
                localDailyData.lastClaimDate.Date == DateTime.Now.Date)
            {
                failCode = "0"; // already claimed today
                return false;
            }

            int expected = localDailyData.claimedRewards.Count;
            if (dayIndex > expected)
            {
                failCode = "1"; // trying to skip days
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles failed claim attempts (local or server) with consistent logging.
        /// </summary>
        private void HandleClaimFail(string reasonCode, int dayIndex, bool isPreServer)
        {
            string prefix = isPreServer ? "[DailyRewardManager][Local]" : "[DailyRewardManager][Server]";
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
            temp.passId = "DailyReward_" + reward.rewardId;
            temp.itemTitle = reward.title;
            temp.itemDescription = reward.description ?? "Daily reward.";
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
        /// For UX: after a full cycle is completed, clear local progress if at least one new calendar day has passed.
        /// This does NOT contact the server; server still validates claims.
        /// </summary>
        private void TryLocalCycleResetIfNeeded()
        {
            int total = GameInstance.Singleton.dailyRewardItems?.Length ?? 0;
            if (total <= 0) return;

            if (localDailyData == null)
                localDailyData = PlayerSave.GetDailyRewardsLocal();

            // Completed cycle?
            if (localDailyData.claimedRewards.Count < total)
                return;

            // Wait at least one day after the last claim before resetting locally.
            if (localDailyData.lastClaimDate == DateTime.MinValue)
                return;

            if ((DateTime.Now.Date - localDailyData.lastClaimDate.Date).TotalDays < 1d)
                return;

            localDailyData.claimedRewards.Clear();
            localDailyData.firstClaimDate = DateTime.Now.Date;
            localDailyData.lastClaimDate = DateTime.MinValue;
            PlayerSave.SetDailyRewardsLocal(localDailyData);
        }

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
