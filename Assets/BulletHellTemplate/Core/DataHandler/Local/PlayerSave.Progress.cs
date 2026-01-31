using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace BulletHellTemplate
{
    public static partial class PlayerSave
    {
        /*────────────────────────── Prefixes & Keys ─────────────────────────*/

        private const string MonstersKilled = "MONSTERSKILLED_";          
        private const string UnlockedMapsKey = "UNLOCKED_MAPS";            
        private const string QuestProgressPrefix = "QUEST_PROGRESS_";        
        private const string QuestCompletionPrefix = "QUEST_COMPLETED_";      
        private const string QuestCompletionTimePref = "QUEST_COMPLETION_TIME_";  
        private const string UsedCouponsKey = "USED_COUPONS_";        
        private const string BattlePassXpKey = "BATTLEPASS_XP";
        private const string BattlePassLevelKey = "BATTLEPASS_LEVEL";
        private const string BattlePassSeasonKey = "BATTLEPASS_SEASON";
        private const string BattlePassSeasonStartTimeKey = "BATTLEPASS_STARTTIME";
        private const string BattlePassSeasonEndTimeKey = "BATTLEPASS_ENDTIME";
        private const string BattlePassSeasonStartUtcKey = "BATTLEPASS_STARTUTC";
        private const string BattlePassSeasonDurationKey = "BATTLEPASS_DURATION_DAYS";
        private const string BattlePassSeasonEndedKey = "BATTLEPASS_SEASON_ENDED";
        private const string BattlePassPremiumKey = "BATTLEPASS_PREMIUM";
 
        /*──────────────────────────── Score / Rank ──────────────────────────*/

        public static void SetScore(int score) => SecurePrefs.SetEncryptedInt(MonstersKilled, score);
        public static int GetScore() => SecurePrefs.GetDecryptedInt(MonstersKilled, 0);

        /*─────────────────────────── Unlocked Maps ──────────────────────────*/

        public static void SetUnlockedMaps(List<int> ids)
            => SecurePrefs.SetEncryptedString(UnlockedMapsKey, string.Join(",", ids));

        public static List<int> GetUnlockedMaps()
        {
            var csv = SecurePrefs.GetDecryptedString(UnlockedMapsKey, "");
            if (string.IsNullOrEmpty(csv)) return new();
            var list = new List<int>();
            foreach (var s in csv.Split(',')) if (int.TryParse(s, out var id)) list.Add(id);
            return list;
        }

        /// <summary>
        /// Checks if the specified map ID is the latest unlocked map.
        /// This ensures that only the latest map can unlock the next one.
        /// </summary>       
        public static bool IsLatestUnlockedMap(int mapId)
        {
            List<int> unlockedMaps = GetUnlockedMaps();
            MapInfoData[] allMaps = GameInstance.Singleton.mapInfoData;

            if (unlockedMaps.Count == 0)
            {
                foreach (var map in allMaps)
                {
                    if (map.mapId == mapId && map.isUnlocked)
                    {
                        return true;
                    }
                }
            }
            return unlockedMaps.Contains(mapId) && unlockedMaps[unlockedMaps.Count - 1] == mapId;
        }
        /*──────────────────────────── Quests (progress / done) ──────────────*/
        public static void SaveQuestProgress(int questId, int progress) =>
            SecurePrefs.SetEncryptedInt($"{QuestProgressPrefix}{questId}", progress);

        public static int LoadQuestProgress(int questId) =>
            SecurePrefs.GetDecryptedInt($"{QuestProgressPrefix}{questId}", 0);

        public static void SaveQuestCompletion(int questId) =>
            SecurePrefs.SetEncryptedInt($"{QuestCompletionPrefix}{questId}", 1);

        public static bool IsQuestCompleted(int questId) =>
            SecurePrefs.GetDecryptedInt($"{QuestCompletionPrefix}{questId}", 0) == 1;

        public static void SaveQuestCompletionTime(int questId, DateTime serverTime) =>
            SecurePrefs.SetEncryptedString($"{QuestCompletionTimePref}{questId}",
                                           serverTime.ToString("yyyy-MM-dd HH:mm:ss"));

        public static void ClearLocalDailyQuestProgress()
        {
            foreach (var q in GameInstance.Singleton.questData)
                if (q.questType == QuestType.Daily)
                {
                    SecurePrefs.DeleteKey($"{QuestProgressPrefix}{q.questId}", false);
                    SecurePrefs.DeleteKey($"{QuestCompletionPrefix}{q.questId}", false);
                    SecurePrefs.DeleteKey($"{QuestCompletionTimePref}{q.questId}", false);
                }
            PlayerPrefs.Save();
        }

        /*─────────────────────────── Used Coupons ───────────────────────────*/

        public static List<string> GetUsedCoupons()
        {
            var csv = SecurePrefs.GetDecryptedString(UsedCouponsKey, "");
            return new(csv.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }
        public static bool IsCouponUsed(string couponId)
        {
            var list = GetUsedCoupons();
            if (list.Contains(couponId)) return true;
            return false;
        }
        public static void MarkCouponAsUsed(string id)
        {
            var list = GetUsedCoupons();
            if (list.Contains(id)) return;
            list.Add(id);
            SecurePrefs.SetEncryptedString(UsedCouponsKey, string.Join(",", list));
        }

        /*──────────────────────────── Battle Pass ─────────────────────*/

        public static (int xp, int level, bool premium) GetBattlePassProgress() =>
            (SecurePrefs.GetDecryptedInt(BattlePassXpKey, 0),
             SecurePrefs.GetDecryptedInt(BattlePassLevelKey, 1),
             SecurePrefs.GetDecryptedInt(BattlePassPremiumKey, 0) == 1);

        public static void SetBattlePassProgress(int xp, int level, bool premium)
        {
            SecurePrefs.SetEncryptedInt(BattlePassXpKey, xp, false);
            SecurePrefs.SetEncryptedInt(BattlePassLevelKey, level, false);
            SecurePrefs.SetEncryptedInt(BattlePassPremiumKey, premium ? 1 : 0, false);
            PlayerPrefs.Save();
        }

        /// <summary>Wipes every local trace of the current battle-pass run.</summary>
        public static void ClearBattlePassLocal()
        {
            SecurePrefs.DeleteKey(BattlePassXpKey, false);          
            SecurePrefs.DeleteKey(BattlePassLevelKey, false);
            SecurePrefs.DeleteKey(BattlePassPremiumKey, false);
            foreach (BattlePassItem item in GameInstance.Singleton.battlePassData)
            {
                SecurePrefs.DeleteKey(BattlePassClaimedKey+ item.passId, false);
            }
            
            PlayerPrefs.Save();
        }

        /// <summary>Adds <paramref name="rewardId"/> to the claimed-rewards list.</summary>
        public static void MarkBattlePassReward(string rewardId)
        {
            var list = LoadClaimedPassRewards();
            if (list.Contains(rewardId)) return;  
            list.Add(rewardId);
            SaveClaimedPassRewards(list);
        }

        public static bool CheckBattlePassRewardClaimed(string rewardId)
        {
            var list = LoadClaimedPassRewards();
            return list.Contains(rewardId);
        }
      
        public static bool HasSeasonEnded() => SecurePrefs.HasKey(BattlePassSeasonEndedKey) && SecurePrefs.GetDecryptedInt(BattlePassSeasonEndedKey) == 1;
        public static void SetSeasonEnded() => SecurePrefs.SetEncryptedInt(BattlePassSeasonEndedKey, 1);
        public static string GetBattlePassSeasonEndTime() => SecurePrefs.GetDecryptedString(BattlePassSeasonEndTimeKey);
        public static void SetBattlePassCurrentSeason(int season) => SecurePrefs.SetEncryptedInt(BattlePassSeasonKey, season);
        public static int GetBattlePassCurrentSeason() => SecurePrefs.GetDecryptedInt(BattlePassSeasonKey);
        public static void BattlePassPremiumUnlock() => SecurePrefs.SetEncryptedInt(BattlePassPremiumKey, 1);  
        public static bool CheckBattlePassPremiumUnlocked() => SecurePrefs.GetDecryptedInt(BattlePassPremiumKey) == 1;

        /// <summary>
        /// Saves full season meta (season #, UTC start, total days).
        /// Also writes legacy end-time key for backward compatibility.
        /// </summary>
        public static void SetBattlePassSeasonMeta(int season, DateTime startUtc, int durationDays)
        {
            SetBattlePassCurrentSeason(season);
            // Normalize to UTC & round to seconds (MySQL compat)
            var sUtc = startUtc.ToUniversalTime();
            var eUtc = sUtc.AddDays(durationDays);

            SecurePrefs.SetEncryptedString(BattlePassSeasonStartUtcKey, sUtc.ToString("o"), false);
            SecurePrefs.SetEncryptedInt(BattlePassSeasonDurationKey, durationDays, false);

            // Legacy compatibility: SeasonEndTimeKey used by old builds.
            SecurePrefs.SetEncryptedString(BattlePassSeasonEndTimeKey, eUtc.ToString("o"), false);

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Tries to read season start (UTC) from local storage.
        /// </summary>
        public static bool TryGetBattlePassSeasonStartUtc(out DateTime startUtc)
        {
            var raw = SecurePrefs.GetDecryptedString(BattlePassSeasonStartUtcKey, null);
            if (string.IsNullOrEmpty(raw))
            {
                startUtc = default;
                return false;
            }
            // 1) Strict ISO round-trip (Recommended).
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out startUtc))
            {
                startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
                return true;
            }
            // 2) Loose parse with AssumeUniversal.
            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out startUtc))
            {
                startUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
                return true;
            }

            startUtc = default;
            return false;
        }

        /// <summary>
        /// Tries to read season end (UTC) from local storage. Prefers Duration+Start.
        /// Falls back to legacy end-time string.
        /// </summary>
        public static bool TryGetBattlePassSeasonEndUtc(out DateTime endUtc)
        {
            // Preferred: compute from start + duration
            if (SecurePrefs.HasKey(BattlePassSeasonDurationKey) && TryGetBattlePassSeasonStartUtc(out var s))
            {
                int d = SecurePrefs.GetDecryptedInt(BattlePassSeasonDurationKey, 30);
                endUtc = s.AddDays(d);
                return true;
            }

            // Legacy: parse saved end-time string
            var raw = SecurePrefs.GetDecryptedString(BattlePassSeasonEndTimeKey, null);
            if (!string.IsNullOrEmpty(raw))
            {
                if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out endUtc))
                {
                    endUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
                    return true;
                }
                if (DateTime.TryParse(raw, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out endUtc))
                {
                    endUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
                    return true;
                }
            }

            endUtc = default;
            return false;
        }

        /// <summary>
        /// Computes remaining time until season end. Returns zero if unknown.
        /// </summary>
        public static TimeSpan GetBattlePassSeasonRemaining()
        {
            if (TryGetBattlePassSeasonEndUtc(out var eUtc))
            {
                var rem = eUtc - DateTime.UtcNow;
                return rem.TotalSeconds < 0 ? TimeSpan.Zero : rem;
            }
            return TimeSpan.Zero;
        }

        /*──────────────────────────── Currency ─────────────────────*/

        public static void SetCurrency(string coinId, int amount) =>
            SecurePrefs.SetEncryptedInt(coinId , amount);

        public static int GetCurrency(string coinId) =>
            SecurePrefs.GetDecryptedInt(coinId , 0);
    }
}
