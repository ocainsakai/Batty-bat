using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Patterns;
using Core.EventSystem;

namespace Core.Systems.CollectionFramework
{
    /// <summary>
    /// Central gallery system for tracking collection progress.
    /// Manages unlocks, achievements, and collection statistics.
    /// </summary>
    public class GallerySystem : PersistentSingleton<GallerySystem>, IGallery
    {
        private Dictionary<string, GalleryItem> _items = new Dictionary<string, GalleryItem>();
        private Dictionary<ItemCategory, List<GalleryItem>> _categories = new Dictionary<ItemCategory, List<GalleryItem>>();
        private Dictionary<ItemRarity, List<GalleryItem>> _rarities = new Dictionary<ItemRarity, List<GalleryItem>>();

        public int TotalItems => _items.Count;
        public int UnlockedItems => _items.Values.Count(i => i.IsUnlocked);

        public void RegisterItem(ItemDefinition item)
        {
            if (item == null || !item.ShowInGallery)
                return;

            if (_items.ContainsKey(item.ItemID))
            {
                Debug.LogWarning($"[GallerySystem] Item already registered: {item.ItemName}");
                return;
            }

            var galleryItem = new GalleryItem(item);
            _items[item.ItemID] = galleryItem;

            // Add to category
            if (!_categories.ContainsKey(item.Category))
            {
                _categories[item.Category] = new List<GalleryItem>();
            }
            _categories[item.Category].Add(galleryItem);

            // Add to rarity
            if (!_rarities.ContainsKey(item.Rarity))
            {
                _rarities[item.Rarity] = new List<GalleryItem>();
            }
            _rarities[item.Rarity].Add(galleryItem);

            Debug.Log($"[GallerySystem] Registered: {item.ItemName}");
        }

        public void UnlockItem(ItemDefinition item)
        {
            if (item == null) return;

            if (_items.TryGetValue(item.ItemID, out var galleryItem))
            {
                bool wasUnlocked = galleryItem.IsUnlocked;
                
                galleryItem.Unlock();
                galleryItem.IncrementCollected();

                if (!wasUnlocked)
                {
                    EventBus.Publish(new ItemUnlockedEvent(item));
                    CheckMilestones();
                }

                EventBus.Publish(new ItemCollectedEvent(item, galleryItem.TimesCollected));
            }
            else
            {
                Debug.LogWarning($"[GallerySystem] Item not registered: {item.ItemName}");
            }
        }

        public bool IsUnlocked(ItemDefinition item)
        {
            if (item == null) return false;
            return _items.TryGetValue(item.ItemID, out var galleryItem) && galleryItem.IsUnlocked;
        }

        public float GetCompletionPercentage()
        {
            if (TotalItems == 0) return 0f;
            return (float)UnlockedItems / TotalItems * 100f;
        }

        public float GetCategoryCompletion(ItemCategory category)
        {
            if (!_categories.TryGetValue(category, out var items) || items.Count == 0)
                return 0f;

            int unlocked = items.Count(i => i.IsUnlocked);
            return (float)unlocked / items.Count * 100f;
        }

        public float GetRarityCompletion(ItemRarity rarity)
        {
            if (!_rarities.TryGetValue(rarity, out var items) || items.Count == 0)
                return 0f;

            int unlocked = items.Count(i => i.IsUnlocked);
            return (float)unlocked / items.Count * 100f;
        }

        /// <summary>
        /// Get all gallery items
        /// </summary>
        public List<GalleryItem> GetAllItems()
        {
            return _items.Values.ToList();
        }

        /// <summary>
        /// Get items by category
        /// </summary>
        public List<GalleryItem> GetByCategory(ItemCategory category)
        {
            return _categories.TryGetValue(category, out var items) ? items : new List<GalleryItem>();
        }

        /// <summary>
        /// Get items by rarity
        /// </summary>
        public List<GalleryItem> GetByRarity(ItemRarity rarity)
        {
            return _rarities.TryGetValue(rarity, out var items) ? items : new List<GalleryItem>();
        }

        /// <summary>
        /// Get unlocked items only
        /// </summary>
        public List<GalleryItem> GetUnlockedItems()
        {
            return _items.Values.Where(i => i.IsUnlocked).ToList();
        }

        /// <summary>
        /// Get locked items only
        /// </summary>
        public List<GalleryItem> GetLockedItems()
        {
            return _items.Values.Where(i => !i.IsUnlocked).ToList();
        }

        private void CheckMilestones()
        {
            float completion = GetCompletionPercentage();

            // Check for milestone achievements
            if (completion >= 25f)
            {
                EventBus.Publish(new CollectionMilestoneEvent(25, UnlockedItems, TotalItems));
            }
            if (completion >= 50f)
            {
                EventBus.Publish(new CollectionMilestoneEvent(50, UnlockedItems, TotalItems));
            }
            if (completion >= 75f)
            {
                EventBus.Publish(new CollectionMilestoneEvent(75, UnlockedItems, TotalItems));
            }
            if (completion >= 100f)
            {
                EventBus.Publish(new CollectionCompletedEvent(TotalItems));
            }
        }

        /// <summary>
        /// Clear all gallery data (for reset/new game)
        /// </summary>
        public void ClearAll()
        {
            _items.Clear();
            _categories.Clear();
            _rarities.Clear();
        }
    }

    #region Events

    public class ItemUnlockedEvent : IEvent
    {
        public ItemDefinition Item { get; set; }

        public ItemUnlockedEvent(ItemDefinition item)
        {
            Item = item;
        }
    }

    public class ItemCollectedEvent : IEvent
    {
        public ItemDefinition Item { get; set; }
        public int TotalCollected { get; set; }

        public ItemCollectedEvent(ItemDefinition item, int totalCollected)
        {
            Item = item;
            TotalCollected = totalCollected;
        }
    }

    public class CollectionMilestoneEvent : IEvent
    {
        public int Percentage { get; set; }
        public int UnlockedCount { get; set; }
        public int TotalCount { get; set; }

        public CollectionMilestoneEvent(int percentage, int unlockedCount, int totalCount)
        {
            Percentage = percentage;
            UnlockedCount = unlockedCount;
            TotalCount = totalCount;
        }
    }

    public class CollectionCompletedEvent : IEvent
    {
        public int TotalItems { get; set; }

        public CollectionCompletedEvent(int totalItems)
        {
            TotalItems = totalItems;
        }
    }

    #endregion
}
