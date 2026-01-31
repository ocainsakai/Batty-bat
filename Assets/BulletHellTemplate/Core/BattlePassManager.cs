using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BulletHellTemplate
{
    /// <summary>
    /// Maintains Battle-Pass XP, level and premium/reward flow.
    /// Server–side persistence is delegated to <see cref="BackendManager.Service"/>.
    /// Season state (number/start/end) is cached in <see cref="PlayerSave"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattlePassManager : MonoBehaviour
    {
        public static BattlePassManager Singleton { get; private set; }

        public event EventHandler OnXPChanged;

        /*──────────── Runtime state ───────────*/
        [HideInInspector] public int currentXP;
        [HideInInspector] public int currentLevel;
        [HideInInspector] public int xpForNextLevel;

        [Header("Translations")]
        [SerializeField]
        private string notEnoughFallback = "Not enough {1}. Need {0} more.";
        [SerializeField] private NameTranslatedByLanguage[] notEnoughTranslated;

        /*────────────────── Unity ──────────────*/
        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                LoadProgress();
            }
            else Destroy(gameObject);
        }

        /*───────────────── Public API ──────────*/
        public bool IsPremium() => PlayerSave.CheckBattlePassPremiumUnlocked();
        public bool IsRewardClaimed(string id) => PlayerSave.CheckBattlePassRewardClaimed(id);
        public bool HasSeasonEnded() => PlayerSave.HasSeasonEnded();

        public int XPForLevel(int level) =>
            Mathf.FloorToInt(GameInstance.Singleton.baseExpPass * Mathf.Pow(1f + GameInstance.Singleton.incPerLevelPass, level - 1));

        /// <summary>Re-reads XP/level from <see cref="PlayerSave"/> and raises <see cref="OnXPChanged"/>.</summary>
        public void SyncFromPlayerSave() => LoadProgress();

        /// <summary>Returns the cached season number.</summary>
        public UniTask<int> GetSeasonAsync() =>
            UniTask.FromResult(Mathf.Max(1, PlayerSave.GetBattlePassCurrentSeason()));

        /// <summary>Returns remaining time until season end based on cached timestamps.</summary>
        public UniTask<TimeSpan> GetRemainingAsync() =>
            UniTask.FromResult(PlayerSave.GetBattlePassSeasonRemaining());

        /*──────────── Premium / Reward ────────*/
        public async UniTask<string> TryUnlockPremiumAsync()
        {
            RequestResult ok = await BackendManager.Service.UnlockBattlePassPremiumAsync();
            if (ok.Success)
            {
                OnXPChanged?.Invoke(this, EventArgs.Empty);
                return null;
            }

            int balance = MonetizationManager.GetCurrency(GameInstance.Singleton.battlePassCurrencyID);
            string tpl = Translate(notEnoughTranslated, notEnoughFallback);
            return string.Format(tpl, GameInstance.Singleton.battlePassPrice - balance, GameInstance.Singleton.battlePassCurrencyID);
        }


        public async UniTask ClaimRewardAsync(BattlePassItem item)
        {
            var r = await BackendManager.Service.ClaimBattlePassRewardAsync(item);
            if (!r.Success) Debug.LogWarning(r.Reason);
        }

        /*──────────── Persistence ─────────────*/
        private void LoadProgress()
        {
            (int xp, int lvl, bool _) = PlayerSave.GetBattlePassProgress();
            currentXP = xp;
            currentLevel = lvl;
            xpForNextLevel = XPForLevel(lvl);
            OnXPChanged?.Invoke(this, EventArgs.Empty);
        }

        /*──────────── Translation helper ──────*/
        private static string Translate(NameTranslatedByLanguage[] table, string fallback)
        {
            string lang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            foreach (var t in table)
                if (t.LanguageId.Equals(lang, StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(t.Translate))
                    return t.Translate;
            return fallback;
        }
    }
}
