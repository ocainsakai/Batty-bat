using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    public class CurrencyDisplay : MonoBehaviour
    {
        [Header("Currency Settings")]
        public string currencyID;
        public TextMeshProUGUI currencyAmount;
        public Image currencyIcon;

        [Header("Recharge UI")]
        public bool isRechargeable;
        public TextMeshProUGUI rechargeAmount;
        public TextMeshProUGUI nextRechargeTime;

        private Currency curData;
        private CancellationTokenSource cts;

        /*────────────────── Unity ──────────────────*/

        private void OnEnable()
        {
            MonetizationManager.OnCurrencyChanged += UpdateCurrencyDisplay;
            cts = new CancellationTokenSource();
            InitAsync(cts.Token).Forget();
        }

        private void OnDisable()
        {
            MonetizationManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
            cts?.Cancel();
            cts = null;
        }

        /*────────────────── Init ──────────────────*/

        private async UniTaskVoid InitAsync(CancellationToken token)
        {

            await BackendManager.CheckInitialized();
            await UniTask.WaitUntil(() =>
                   GameInstance.Singleton != null &&
                   GameInstance.Singleton.currencyData != null &&
                   MonetizationManager.Singleton != null);

            curData = GameInstance.Singleton.currencyData.Find(c => c.coinID == currencyID);

            if (curData != null && currencyIcon != null)
                currencyIcon.sprite = curData.icon;

            UpdateCurrencyDisplay(currencyID, MonetizationManager.GetCurrency(currencyID));
            UpdateRechargeUI();

            while (!token.IsCancellationRequested &&
                   isRechargeable &&
                   curData?.isRechargeableCurrency == true)
            {
                UpdateRechargeUI();
                await UniTask.Delay(1000, cancellationToken: token);
            }
        }

        /*────────────────── UI helpers ──────────────────*/

        private void UpdateCurrencyDisplay(string changed, int amount)
        {
            if (changed == currencyID && currencyAmount != null)
                currencyAmount.text = amount.ToString();
        }

        private void UpdateRechargeUI()
        {
            if (!isRechargeable || curData == null)
            {
                if (rechargeAmount) rechargeAmount.gameObject.SetActive(false);
                if (nextRechargeTime) nextRechargeTime.gameObject.SetActive(false);
                return;
            }

            if (rechargeAmount) rechargeAmount.gameObject.SetActive(true);
            if (nextRechargeTime) nextRechargeTime.gameObject.SetActive(true);

            OfflineRechargeHandler.TryGetSecondsToNextTick(
                currencyID, out float seconds, out bool atMax);

            if (rechargeAmount)
                rechargeAmount.text = atMax ? "" : $"+{curData.rechargeAmount}";

            if (nextRechargeTime)
                nextRechargeTime.text = atMax ? "" : $"{Mathf.CeilToInt(seconds)}s";
        }
    }
}
