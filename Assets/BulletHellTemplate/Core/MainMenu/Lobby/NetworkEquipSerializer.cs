using BulletHellTemplate;
using System.Collections.Generic;
using System.Text;

namespace BulletHellTemplate
{
    /// <summary>
    /// Utility class for serializing and deserializing equipped item data
    /// for a character into a compact string format suitable for network transmission.
    /// </summary>
    public static class NetworkEquipSerializer
    {
        private const char ENTRY_SEP = '|';
        private const char KV_SEP = ':';

        /// <summary>
        /// Builds a serialized string representing the equipped items for a specific character.
        /// Each item is represented as a key-value pair separated by ':' and multiple entries are separated by '|'.
        /// </summary>
        /// <param name="charId">The ID of the character whose equipped items will be serialized.</param>
        /// <returns>A serialized string of slot-to-item GUID mappings, or an empty string if no items are equipped.</returns>
        public static string BuildItemCodeString(int charId)
        {
            var charData = GameInstance.Singleton.GetCharacterDataById(charId);
            if (charData == null || charData.itemSlots == null) return string.Empty;

            var sb = new StringBuilder(64);
            foreach (var slot in charData.itemSlots)
            {
                string guid = PlayerSave.GetCharacterSlotItem(charId, slot);
                if (string.IsNullOrEmpty(guid)) continue;

                if (sb.Length > 0) sb.Append(ENTRY_SEP);
                sb.Append(slot).Append(KV_SEP).Append(guid);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Parses a serialized item code string back into a dictionary of slot-to-item GUID mappings.
        /// </summary>
        /// <param name="codes">The serialized string containing equipped item data.</param>
        /// <returns>A dictionary mapping slot names to item GUIDs.</returns>
        public static Dictionary<string, string> ParseItemCodeString(string codes)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(codes)) return dict;

            var entries = codes.Split(ENTRY_SEP);
            foreach (var e in entries)
            {
                var kv = e.Split(KV_SEP);
                if (kv.Length == 2) dict[kv[0]] = kv[1];
            }
            return dict;
        }
    }
}