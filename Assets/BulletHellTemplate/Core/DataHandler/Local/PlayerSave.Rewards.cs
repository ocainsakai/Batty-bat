using System;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    public static partial class PlayerSave
    {
        /*────────────────────────── REWARD KEYS ─────────────────*/

        private const string DailyRewardsKey = "LOCAL_DAILYREWARDS_DATA";
        private const string NewPlayerRewardsKey = "LOCAL_NEWPLAYERREWARDS_DATA";
        private const string NextDailyResetKey = "NEXTDAILYRESETTIME";
        private const string BattlePassClaimedKey = "BATTLEPASS_CLAIMED";
        private const string MapRewardsClaimedKey = "LOCAL_MAPREWARDS_CLAIMED"; // csv int list      

        /*────────────────────────── Daily / New-Player Rewards ──────────────*/

        public static void SetDailyRewardsLocal(DailyRewardsData d) =>
            SecurePrefs.SetEncryptedString(DailyRewardsKey, JsonUtility.ToJson(d));

        public static DailyRewardsData GetDailyRewardsLocal()
        {
            var json = SecurePrefs.GetDecryptedString(DailyRewardsKey, "");
            return string.IsNullOrEmpty(json)
                ? new DailyRewardsData()
                : JsonUtility.FromJson<DailyRewardsData>(json) ?? new DailyRewardsData();
        }

        public static void SetNewPlayerRewardsLocal(NewPlayerRewardsData d) =>
            SecurePrefs.SetEncryptedString(NewPlayerRewardsKey, JsonUtility.ToJson(d));

        public static NewPlayerRewardsData GetNewPlayerRewardsLocal()
        {
            var json = SecurePrefs.GetDecryptedString(NewPlayerRewardsKey, "");
            return string.IsNullOrEmpty(json)
                ? new NewPlayerRewardsData()
                : JsonUtility.FromJson<NewPlayerRewardsData>(json) ?? new NewPlayerRewardsData();
        }

        public static void SetNextDailyReset(DateTime next) =>
            SecurePrefs.SetEncryptedString(NextDailyResetKey, next.Ticks.ToString());

        public static DateTime GetNextDailyReset()
        {
            var ticksStr = SecurePrefs.GetDecryptedString(NextDailyResetKey, "");
            return long.TryParse(ticksStr, out var t) ? new DateTime(t) : DateTime.MinValue;
        }

        /*────────────────────────── Battle-Pass helpers ─────────────────────*/

        public static (int xp, int level, bool premium) GetBattlePass() =>
            (SecurePrefs.GetDecryptedInt(BattlePassXpKey, 0),
             SecurePrefs.GetDecryptedInt(BattlePassLevelKey, 1),
             SecurePrefs.GetDecryptedInt(BattlePassPremiumKey, 0) == 1);

        public static void SetBattlePass(int xp, int level, bool premium)
        {
            SecurePrefs.SetEncryptedInt(BattlePassXpKey, xp, false);
            SecurePrefs.SetEncryptedInt(BattlePassLevelKey, level, false);
            SecurePrefs.SetEncryptedInt(BattlePassPremiumKey, premium ? 1 : 0, false);
            PlayerPrefs.Save();
        }

        public static void SaveClaimedPassRewards(List<string> ids) =>
            SecurePrefs.SetEncryptedString(BattlePassClaimedKey, string.Join(",", ids));

        public static List<string> LoadClaimedPassRewards()
        {
            var csv = SecurePrefs.GetDecryptedString(BattlePassClaimedKey, "");
            return new(csv.Split(',', StringSplitOptions.RemoveEmptyEntries));
        }

        /*────────────────────────── reward first complete map ─────────────*/

        public static void SaveClaimedMapRewards(List<int> ids) =>
            SecurePrefs.SetEncryptedString(MapRewardsClaimedKey, string.Join(",", ids));

        public static List<int> LoadClaimedMapRewards()
        {
            var csv = SecurePrefs.GetDecryptedString(MapRewardsClaimedKey, "");
            if (string.IsNullOrEmpty(csv)) return new List<int>();
            var parts = csv.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = new List<int>(parts.Length);
            foreach (var p in parts)
                if (int.TryParse(p, out var id)) list.Add(id);
            return list;
        }

        /*────────────────────────── Data-classes ─────────────*/

        [Serializable]
        public class DailyRewardsData
        {
            public string firstClaimDateString;
            public string lastClaimDateString;          // NEW: track last day claimed

            public List<int> claimedRewards = new();

            [NonSerialized] private DateTime _first;
            [NonSerialized] private DateTime _last;

            /// <summary>Date of the very first daily-claim cycle start.</summary>
            public DateTime firstClaimDate
            {
                get
                {
                    if (_first == DateTime.MinValue &&
                        !string.IsNullOrEmpty(firstClaimDateString) &&
                        DateTime.TryParse(firstClaimDateString, out var d))
                    {
                        _first = d;
                    }
                    return _first;
                }
                set
                {
                    _first = value;
                    firstClaimDateString = value.ToString("yyyy-MM-dd");
                }
            }

            /// <summary>Most recent calendar date a daily reward was claimed.</summary>
            public DateTime lastClaimDate
            {
                get
                {
                    if (_last == DateTime.MinValue &&
                        !string.IsNullOrEmpty(lastClaimDateString) &&
                        DateTime.TryParse(lastClaimDateString, out var d))
                    {
                        _last = d;
                    }
                    return _last;
                }
                set
                {
                    _last = value;
                    lastClaimDateString = value.ToString("yyyy-MM-dd");
                }
            }
        }

        [Serializable]
        public class NewPlayerRewardsData
        {
            public string accountCreationDateString;
            public string lastClaimDateString;    // track last calendar date a new-player reward was claimed

            public List<int> claimedRewards = new();

            [NonSerialized] private DateTime _created;
            [NonSerialized] private DateTime _last;

            /// <summary>Date the account was created (UTC date cached locally; day resolution).</summary>
            public DateTime accountCreationDate
            {
                get
                {
                    if (_created == DateTime.MinValue &&
                        !string.IsNullOrEmpty(accountCreationDateString) &&
                        DateTime.TryParse(accountCreationDateString, out var d))
                    {
                        _created = d;
                    }
                    return _created;
                }
                set
                {
                    _created = value;
                    accountCreationDateString = value.ToString("yyyy-MM-dd");
                }
            }

            /// <summary>Most recent calendar date (local) a new-player reward was claimed.</summary>
            public DateTime lastClaimDate
            {
                get
                {
                    if (_last == DateTime.MinValue &&
                        !string.IsNullOrEmpty(lastClaimDateString) &&
                        DateTime.TryParse(lastClaimDateString, out var d))
                    {
                        _last = d;
                    }
                    return _last;
                }
                set
                {
                    _last = value;
                    lastClaimDateString = value.ToString("yyyy-MM-dd");
                }
            }
        }
    }
}
