using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a coupon item that can be redeemed by the player.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCouponItem", menuName = "Coupon/Coupon Item", order = 51)]
    public class CouponItem : ScriptableObject
    {
        public string idCoupon; // Unique ID for the coupon
        public string couponCode; // The code the player needs to enter to claim the coupon
        public string currencyRewardId = "GO"; // The ID of the currency the coupon rewards
        public int currencyAmount = 0; // The amount of currency rewarded by the coupon
    }
}
