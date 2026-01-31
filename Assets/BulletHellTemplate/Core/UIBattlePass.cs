using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Globalization;

namespace BulletHellTemplate
{
    /// <summary>
    /// Battle-Pass UI controller. Builds item list, shows XP bar and live season timer.
    /// Reads season timing from <see cref="PlayerSave"/> (cached from server at login).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UIBattlePass : MonoBehaviour
    {
        public static UIBattlePass Singleton { get; private set; }

        /*──────────── Inspector refs ───────────*/
        [Header("Prefabs")]
        [SerializeField] private PassEntry passEntryPrefab;
        [SerializeField] private Transform containerPassItems;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI errorMessage;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private Image xpProgressBar;
        [SerializeField] private TextMeshProUGUI currentSeason;
        [SerializeField] private TextMeshProUGUI remainingSeasonTime;
        [SerializeField] private GameObject ButtonBuyPassVisual;

        [Header("Buy-Pass Popup")]
        [SerializeField] private GameObject purchasePopup;
        [SerializeField] private Image purchaseIconCurrency;
        [SerializeField] private TextMeshProUGUI purchaseTextPrice;

        [Header("Claim Reward Popup")]
        public GameObject claimPopup;
        public Image claimIconReward;
        public TextMeshProUGUI claimTextRewardAmount;

        [Header("Events")]
        public UnityEvent OnOpenMenu;
        public UnityEvent OnCloseMenu;

        /*──────────── Internal ────────────────*/
        private CancellationTokenSource cts;
        private DateTime seasonStartUtc;
        private DateTime seasonEndUtc;

        /*──────────── Unity ───────────────────*/
        private void Awake()
        {
            if (Singleton == null) Singleton = this;
        }

        private void OnEnable()
        {
            BattlePassManager.Singleton.OnXPChanged += RefreshXP;
            OnOpenMenu?.Invoke();
            _ = ReloadAsync();
        }

        private void OnDisable()
        {
            BattlePassManager.Singleton.OnXPChanged -= RefreshXP;
            cts?.Cancel();
            OnCloseMenu?.Invoke();
        }

        /*──────────── UI Actions ──────────────*/
        public void ShowBuyPopup()
        {
            purchaseIconCurrency.sprite = MonetizationManager.Singleton.GetCurrencyIcon(GameInstance.Singleton.battlePassCurrencyID);
            purchaseTextPrice.text = GameInstance.Singleton.battlePassPrice.ToString();
            purchasePopup.SetActive(true);
        }
        public void HideBuyPopup() => purchasePopup.SetActive(false);

        public async void ConfirmBuyPass()
        {
            string err = await BattlePassManager.Singleton.TryUnlockPremiumAsync();
            if (string.IsNullOrEmpty(err))
            {
                HideBuyPopup();
                await ReloadAsync();
            }
            else ShowErrorMessage(err).Forget();
        }

        /*──────────── Reload / Build ──────────*/
        public async UniTask ReloadAsync()
        {
            Clear();
            await BuildEntriesAsync();
            await RefreshSeasonAsync();
            RefreshXP(null, EventArgs.Empty);
            ButtonBuyPassVisual.SetActive(!BattlePassManager.Singleton.IsPremium());
        }

        private void Clear()
        {
            foreach (Transform c in containerPassItems) Destroy(c.gameObject);
        }

        private async UniTask BuildEntriesAsync()
        {
            string lang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            for (int i = 0; i < GameInstance.Singleton.battlePassData.Length; i++)
            {
                var it = GameInstance.Singleton.battlePassData[i];
                var e = Instantiate(passEntryPrefab, containerPassItems);
                bool premium = BattlePassManager.Singleton.IsPremium();
                bool unlocked = premium || it.rewardTier == BattlePassItem.RewardTier.Free;
                bool claimed = BattlePassManager.Singleton.IsRewardClaimed(it.passId);

                e.SetPassItemInfo(
                    it, it.passId,
                    it.GetTranslatedTitle(lang),
                    it.GetTranslatedDescription(lang),
                    it.itemIcon,
                    it.rewardTier == BattlePassItem.RewardTier.Paid,
                    unlocked, claimed,
                    i);
            }
            await UniTask.Yield();
        }

        /*──────────── XP / Season HUD ─────────*/
        public void RefreshXP(object _, EventArgs __)
        {
            var bp = BattlePassManager.Singleton;
            levelText.text = $"Lv. {bp.currentLevel}";
            xpText.text = $"{bp.currentXP}/{bp.xpForNextLevel}";
            xpProgressBar.fillAmount = bp.xpForNextLevel > 0
                ? (float)bp.currentXP / bp.xpForNextLevel
                : 0f;

            // Update each PassEntry progress/claim state
            foreach (Transform t in containerPassItems)
            {
                if (t.TryGetComponent<PassEntry>(out var entry))
                    entry.UpdateProgressBar();
            }
        }

        public void ShowClaimPopup(string title, Sprite icon, string desc)
        {
            claimIconReward.sprite = icon;
            claimTextRewardAmount.text = desc;
            claimPopup.SetActive(true);
        }

        public void HideClaimPopup() => claimPopup.SetActive(false);

        /// <summary>
        /// Loads cached season info from <see cref="PlayerSave"/> and starts countdown.
        /// </summary>
        private async UniTask RefreshSeasonAsync()
        {
            await UniTask.SwitchToMainThread();

            int season = PlayerSave.GetBattlePassCurrentSeason(); 

            if (!PlayerSave.TryGetBattlePassSeasonStartUtc(out seasonStartUtc))
                seasonStartUtc = DateTime.UtcNow;

            if (!PlayerSave.TryGetBattlePassSeasonEndUtc(out seasonEndUtc))
                seasonEndUtc = seasonStartUtc.AddDays(30);

            currentSeason.text = $"Season {season}";

            cts?.Cancel();
            cts = new CancellationTokenSource();
            CountdownAsync(cts.Token).Forget();
        }

        /// <summary>
        /// Displays live countdown to <see cref="seasonEndUtc"/>.
        /// </summary>
        private async UniTaskVoid CountdownAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                TimeSpan span = seasonEndUtc - DateTime.UtcNow;
                if (span.TotalSeconds <= 0)
                {
                    remainingSeasonTime.text = "Season ended";
                    PlayerSave.SetSeasonEnded();
                    break;
                }

                remainingSeasonTime.text =
                    $"{span.Days}d {span.Hours}h {span.Minutes}m {span.Seconds:D2}s";

                await UniTask.Delay(1000, cancellationToken: token);
            }
        }

        private async UniTaskVoid ShowErrorMessage(string message)
        {
            errorMessage.text = message;
            await UniTask.Delay(1500);
            errorMessage.text = "";
        }

        /// <summary>
        /// Debug helper: award XP via backend test endpoint; refreshes UI on success.
        /// </summary>
        public async void TestEXPPass(int xp)
        {
            var result = await BackendManager.Service.TestAddBattlePassXpAsync(xp);
            if (result.Success)
            {
                // Reload manager from PlayerSave so level/XP recalc + event dispatch.
                BattlePassManager.Singleton.SyncFromPlayerSave();
                RefreshXP(null, null);
            }
        }
    }
}
