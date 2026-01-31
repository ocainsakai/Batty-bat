// BulletHellTemplate/Monetization/MonetizationManager.cs
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Holds currency cache & thin wrappers that forward purchase calls
    /// to <see cref="BackendManager.Service"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MonetizationManager : MonoBehaviour
    {
        public static MonetizationManager Singleton { get; private set; }

        /// <summary>Raised every time a local currency value is updated.</summary>
        public static event Action<string, int> OnCurrencyChanged;

        /*──────────────────── Unity ────────────────────*/

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /*───────────────── Purchases (wrappers) ─────────────────*/

        public UniTask<RequestResult> PurchaseShopItemAsync(ShopItem item) =>
            BackendManager.Service.PurchaseShopItemAsync(item);

        public UniTask<RequestResult> ClaimBattlePassRewardAsync(BattlePassItem reward) =>
            BackendManager.Service.ClaimBattlePassRewardAsync(reward);

        /*──────────────────── Currency API ────────────────────*/

        /// <summary>Returns current amount stored in <c>PlayerPrefs</c>.</summary>
        public static int GetCurrency(string currencyKey) => PlayerSave.GetCurrency(currencyKey);

        /// <summary>
        /// Sets the amount locally, fires <see cref="OnCurrencyChanged"/>,
        /// and (optionally) pushes the change to the active backend.
        /// </summary>
        public static void SetCurrency(string currencyKey, int amount, bool pushToBackend = true)
        {
            PlayerSave.SetCurrency(currencyKey, amount);
            OnCurrencyChanged?.Invoke(currencyKey, amount);
        }

        /// <summary>Returns the sprite associated with a currency (or <c>null</c>).</summary>
        public Sprite GetCurrencyIcon(string currencyKey)
        {
            var entry = GameInstance.Singleton.currencyData.Find(c => c.coinID == currencyKey);
            return entry != null ? entry.icon : null;
        }
    }
}
