using System.Collections.Generic;
using System;

namespace BulletHellTemplate
{
    /// <summary>
    /// Data class for saving recharge information.
    /// </summary>
    [Serializable]
    public class RechargeSaveData
    {
        public List<RechargeKeyValue> entries = new List<RechargeKeyValue>();

        /// <summary>
        /// Sets the last recharge time for a given coin.
        /// </summary>
        /// <param name="coinID">Identifier for the coin.</param>
        /// <param name="unixTime">The Unix time to set as the last recharge time.</param>
        public void SetValue(string coinID, long unixTime)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].coinID == coinID)
                {
                    entries[i].lastRechargeTime = unixTime;
                    return;
                }
            }
            entries.Add(new RechargeKeyValue { coinID = coinID, lastRechargeTime = unixTime });
        }

        /// <summary>
        /// Gets the last recharge time for a given coin.
        /// </summary>
        /// <param name="coinID">Identifier for the coin.</param>
        /// <returns>Last recharge time as Unix time. Returns 0 if not found.</returns>
        public long GetValue(string coinID)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].coinID == coinID)
                    return entries[i].lastRechargeTime;
            }
            return 0L;
        }
    }

    /// <summary>
    /// Represents a key-value pair for recharge data.
    /// </summary>
    [Serializable]
    public class RechargeKeyValue
    {
        public string coinID;
        public long lastRechargeTime;
    }
}