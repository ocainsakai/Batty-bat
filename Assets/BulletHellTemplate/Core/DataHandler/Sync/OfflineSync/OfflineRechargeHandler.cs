using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BulletHellTemplate
{
    /// <summary>
    /// Centralizes rechargeable-currency logic in the offline backend layer.
    /// </summary>
    public static class OfflineRechargeHandler
    {
        private const string PREF_KEY = "RechargeTimestamps";  // JSON in PlayerPrefs
        private const float AUTO_SAVE_INTERVAL = 60f;          // seconds
        private static Dictionary<string, long> lastTime = new(); // coinID → unix

        private static bool isRunning;
        private static float saveTimer;

        /*──────────── PUBLIC API ────────────*/

        /// <summary>
        /// Starts the whole recharge system: applies offline ticks
        /// and spins a UniTask loop que concede recarga on-line.
        /// </summary>
        public static UniTask<RequestResult> StartAsync()
        {
            Load();
            ApplyOfflineTicks();
            if (!isRunning) RunLoop().Forget();
            return UniTask.FromResult(RequestResult.Ok());
        }

        /*──────────── CORE LOGIC ────────────*/

        private static async UniTaskVoid RunLoop()
        {
            isRunning = true;
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                TickCurrencies(1f);

                saveTimer += 1f;
                if (saveTimer >= AUTO_SAVE_INTERVAL)
                {
                    saveTimer = 0f;
                    Save();
                }
            }
        }

        private static void TickCurrencies(float delta)
        {
            foreach (Currency cur in GameInstance.Singleton.currencyData)
            {
                if (!cur.isRechargeableCurrency) continue;

                long now = GetUnix();
                long last = GetLast(cur.coinID);

                int amount = MonetizationManager.GetCurrency(cur.coinID);
                if (cur.useMaxAmount && amount >= cur.maxAmount)
                {
                    SetLast(cur.coinID, 0);
                    continue;
                }

                float secondsPerTick = ToSeconds(cur.rechargeableTime, cur.rechargeableTimeScale);
                if (now - last < secondsPerTick) continue;

                int ticks = Mathf.FloorToInt((now - last) / secondsPerTick);
                int gained = ticks * cur.rechargeAmount;
                int newAmount = amount + gained;

                if (cur.useMaxAmount && newAmount > cur.maxAmount)
                {
                    newAmount = cur.maxAmount;
                    SetLast(cur.coinID, 0);
                }
                else
                {
                    long used = (long)(ticks * secondsPerTick);
                    SetLast(cur.coinID, last + used);
                }

                MonetizationManager.SetCurrency(cur.coinID, newAmount, pushToBackend: false);
            }
        }

        private static void ApplyOfflineTicks()
        {
            TickCurrencies(0f);
        }

        /*──────────── HUD helpers ───────────*/

        /// <summary>
        /// Returns how many seconds remain until the next tick for <paramref name="coinId"/>.  
        /// If the currency is full (max reached), <paramref name="atMax"/> = true and
        /// <paramref name="secondsLeft"/> = 0.
        /// </summary>
        public static bool TryGetSecondsToNextTick(string coinId, out float secondsLeft, out bool atMax)
        {
            secondsLeft = 0;
            atMax = false;

            Currency cur = GameInstance.Singleton.currencyData
                                .Find(c => c.coinID == coinId && c.isRechargeableCurrency);
            if (cur == null) return false;

            int amount = MonetizationManager.GetCurrency(coinId);
            if (cur.useMaxAmount && amount >= cur.maxAmount && !cur.canExceedMaxValue)
            {
                atMax = true;
                return true;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long last = lastTime.TryGetValue(coinId, out long t) ? t : now;

            float tickSec = ToSeconds(cur.rechargeableTime, cur.rechargeableTimeScale);
            float elapsed = now - last;
            secondsLeft = Mathf.Max(0, tickSec - elapsed % tickSec);
            return true;
        }

        /*───────────── Utility ─────────────*/

        private static long GetLast(string cid) => lastTime.TryGetValue(cid, out long t) ? t : 0;
        private static void SetLast(string cid, long t) => lastTime[cid] = t;
        private static long GetUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private static float ToSeconds(float v, rechargeableTimeScale s) =>
            s switch
            {
                rechargeableTimeScale.Seconds => v,
                rechargeableTimeScale.Minutes => v * 60f,
                rechargeableTimeScale.Hours => v * 3600f,
                _ => v
            };

        /*───────────── Persistence ─────────────*/

        private static void Load()
        {
            string json = PlayerPrefs.GetString(PREF_KEY, "{}");
            var data = JsonUtility.FromJson<RechargeSaveData>(json);
            lastTime.Clear();
            if (data != null)
                foreach (var kv in data.entries)
                    lastTime[kv.coinID] = kv.lastRechargeTime;
        }

        private static void Save()
        {
            var data = new RechargeSaveData();
            foreach (var kv in lastTime)
                data.entries.Add(new RechargeKeyValue { coinID = kv.Key, lastRechargeTime = kv.Value });

            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PREF_KEY, json);
            PlayerPrefs.Save();
        }
    }
}
