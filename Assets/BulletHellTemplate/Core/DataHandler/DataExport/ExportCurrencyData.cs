#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Exports the currency data for server-side validation in a simplified JSON format.
    /// </summary>
    public static class CurrencyDataExporter
    {
        /// <summary>
        /// Exports currency data found in MonetizationManager.Singleton.
        /// </summary>
        public static void ExportCurrencyData()
        {
            if (MonetizationManager.Singleton == null)
            {
                Debug.LogError("MonetizationManager is not initialized.");
                return;
            }

            List<CurrencyExportData> currencyList = new List<CurrencyExportData>();

            foreach (var currency in GameInstance.Singleton.currencyData)
            {
                if (currency == null)
                    continue;

                CurrencyExportData currencyExport = new CurrencyExportData
                {
                    coinName = currency.coinName,
                    coinID = currency.coinID,
                    initialAmount = currency.initialAmount,
                    useMaxAmount = currency.useMaxAmount,
                    maxAmount = currency.maxAmount,
                    canExceedMaxValue = currency.canExceedMaxValue,
                    isRechargeableCurrency = currency.isRechargeableCurrency,
                    rechargeableTimeScale = currency.rechargeableTimeScale.ToString(),
                    rechargeableTime = currency.rechargeableTime,
                    rechargeAmount = currency.rechargeAmount,
                    rechargeWhileOffline = currency.RechargeWhileOffline
                };

                currencyList.Add(currencyExport);
            }

            string json = JsonConvert.SerializeObject(currencyList, Formatting.Indented);

            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "CurrencyData.json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Currency data exported successfully to {filePath}");
        }

        /// <summary>
        /// Represents the data structure used to export each currency.
        /// </summary>
        private class CurrencyExportData
        {
            public string coinName;
            public string coinID;
            public int initialAmount;
            public bool useMaxAmount;
            public int maxAmount;
            public bool canExceedMaxValue;
            public bool isRechargeableCurrency;
            public string rechargeableTimeScale;
            public float rechargeableTime;
            public int rechargeAmount;
            public bool rechargeWhileOffline;
        }
    }
}
#endif