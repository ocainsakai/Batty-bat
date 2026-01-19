using System;
using UnityEngine;

namespace Core.Systems.CollectionFramework
{
    /// <summary>
    /// Represents an item in the gallery/collection book.
    /// Tracks unlock status and collection statistics.
    /// </summary>
    [Serializable]
    public class GalleryItem : IGalleryItem
    {
        [SerializeField] private ItemDefinition _definition;
        [SerializeField] private bool _isUnlocked;
        [SerializeField] private string _unlockedDateString;
        [SerializeField] private int _timesCollected;

        public ItemDefinition Definition => _definition;
        public bool IsUnlocked => _isUnlocked;
        public int TimesCollected => _timesCollected;

        public DateTime UnlockedDate
        {
            get
            {
                if (string.IsNullOrEmpty(_unlockedDateString))
                    return DateTime.MinValue;
                
                if (DateTime.TryParse(_unlockedDateString, out DateTime date))
                    return date;
                
                return DateTime.MinValue;
            }
        }

        public GalleryItem(ItemDefinition definition)
        {
            _definition = definition;
            _isUnlocked = false;
            _timesCollected = 0;
        }

        public void Unlock()
        {
            if (!_isUnlocked)
            {
                _isUnlocked = true;
                _unlockedDateString = DateTime.Now.ToString("o"); // ISO 8601 format
                Debug.Log($"[GalleryItem] Unlocked: {Definition.ItemName}");
            }
        }

        public void IncrementCollected()
        {
            _timesCollected++;
        }

        /// <summary>
        /// Get display info for UI
        /// </summary>
        public string GetDisplayInfo()
        {
            if (!IsUnlocked)
                return "???";

            return $"{Definition.ItemName}\nCollected: {TimesCollected}x\nUnlocked: {UnlockedDate:yyyy-MM-dd}";
        }
    }
}
