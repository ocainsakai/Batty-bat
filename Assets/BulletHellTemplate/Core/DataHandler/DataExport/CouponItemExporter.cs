#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles the export of CouponItem data into a simplified JSON format for server validation.
    /// </summary>
    public static class CouponItemExporter
    {
        /// <summary>
        /// Exports all CouponItems from GameInstance into a simplified JSON format.
        /// </summary>
        public static void ExportCoupons()
        {
            if (GameInstance.Singleton == null || GameInstance.Singleton.couponData == null)
            {
                Debug.LogError("GameInstance or couponData not found.");
                return;
            }

            List<SerializableCouponItem> exportList = new List<SerializableCouponItem>();

            foreach (var coupon in GameInstance.Singleton.couponData)
            {
                if (coupon == null)
                    continue;

                var serializableCoupon = new SerializableCouponItem
                {
                    idCoupon = coupon.idCoupon,
                    couponCode = coupon.couponCode,
                    currencyRewardId = coupon.currencyRewardId,
                    currencyAmount = coupon.currencyAmount
                };

                exportList.Add(serializableCoupon);
            }

            string json = JsonConvert.SerializeObject(exportList, Formatting.Indented);

            string directoryPath = Path.Combine(Application.dataPath, "ExportedData");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, "CouponItems.json");
            File.WriteAllText(filePath, json);

            Debug.Log($"Coupon items exported successfully to {filePath}");
        }

        /// <summary>
        /// Simplified data structure for a coupon item.
        /// </summary>
        private class SerializableCouponItem
        {
            public string idCoupon;
            public string couponCode;
            public string currencyRewardId;
            public int currencyAmount;
        }
    }
}
#endif