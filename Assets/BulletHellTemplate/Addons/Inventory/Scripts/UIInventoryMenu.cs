using BulletHellTemplate;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// UI menu for displaying and managing inventory items that have been purchased.
    /// Supports filtering by rarity, category, and slot, and upgrading items.
    /// </summary>
    public class UIInventoryMenu : MonoBehaviour
    {
        public static UIInventoryMenu Singleton;

        [Header("Inventory UI Elements")]
        public InventoryEntry inventoryEntryPrefab;
        public Transform inventoryContainer;

        [Header("Item Info")]
        public InventoryItemInfo inventoryItemInfoPrefab;

        [Header("Message UI")]
        public TextMeshProUGUI messageText;

        [Header("Dropdowns")]
        public TMP_Dropdown sortDropdown;
        public TMP_Dropdown filterRarityDropdown;
        public TMP_Dropdown filterCategoryDropdown;
        public TMP_Dropdown filterSlotDropdown;

        [Header("Filter Options")]
        public List<string> categoryOptions;
        public OptionsTranslate[] categoryOptionsTranslate;
        public List<string> slotOptions;
        public OptionsTranslate[] slotOptionsTranslate;

        // Localization for sorting
        [Header("Dropdown Localization")]
        public NameTranslatedByLanguage[] sortByRarityAscTranslated;
        public string sortByRarityAscFallback = "Sort by Rarity (Asc)";

        public NameTranslatedByLanguage[] sortByRarityDescTranslated;
        public string sortByRarityDescFallback = "Sort by Rarity (Desc)";

        public NameTranslatedByLanguage[] sortBySlotTranslated;
        public string sortBySlotFallback = "Sort by Slot";

        public NameTranslatedByLanguage[] allOptionTranslated;
        public string allOptionFallback = "All";

        // Messages Localization
        [Header("Upgrade Messages Localization")]
        public NameTranslatedByLanguage[] maxLevelReachedTranslated;
        public string maxLevelReachedFallback = "Max Level Reached!";

        public NameTranslatedByLanguage[] insufficientCurrencyTranslated;
        public string insufficientCurrencyFallback = "Insufficient currency!";

        public NameTranslatedByLanguage[] upgradeSuccessTranslated;
        public string upgradeSuccessFallback = "Upgrade Successful!";

        public NameTranslatedByLanguage[] upgradeFailTranslated;
        public string upgradeFailFallback = "Upgrade Failed!";

        public NameTranslatedByLanguage[] levelDecreasedTranslated;
        public string levelDecreasedFallback = "Level Decreased!";

        private Dictionary<string, string> categoryTranslationMap = new Dictionary<string, string>();
        private Dictionary<string, string> slotTranslationMap = new Dictionary<string, string>();
        private List<InventoryEntry> spawnedEntries = new List<InventoryEntry>();

        private string currentRarityFilter = "";
        private string currentCategoryFilter = "";
        private string currentSlotFilter = "";

        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            SetupDropdowns();
            UpdateInventoryUI();
        }

        /// <summary>
        /// Configures the dropdown options for sorting and filtering, using translations.
        /// </summary>
        private void SetupDropdowns()
        {
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();

            // Sort dropdown
            string rarityAsc = GetTranslatedString(sortByRarityAscTranslated, sortByRarityAscFallback, currentLang);
            string rarityDesc = GetTranslatedString(sortByRarityDescTranslated, sortByRarityDescFallback, currentLang);
            string slotSort = GetTranslatedString(sortBySlotTranslated, sortBySlotFallback, currentLang);

            sortDropdown.ClearOptions();
            var sortOptions = new List<string> { rarityAsc, rarityDesc, slotSort };
            sortDropdown.AddOptions(sortOptions);
            sortDropdown.onValueChanged.AddListener(OnSortDropdownChanged);

            // Rarity dropdown
            filterRarityDropdown.ClearOptions();
            List<string> rarityNames = System.Enum.GetNames(typeof(Rarity)).ToList();
            string allString = GetTranslatedString(allOptionTranslated, allOptionFallback, currentLang);
            rarityNames.Insert(0, allString);
            filterRarityDropdown.AddOptions(rarityNames);
            filterRarityDropdown.onValueChanged.AddListener(OnFilterRarityDropdownChanged);

            // Category dropdown
            filterCategoryDropdown.ClearOptions();
            categoryTranslationMap.Clear();
            var catOptions = new List<string> { allString };
            for (int i = 0; i < categoryOptions.Count; i++)
            {
                string originalCat = categoryOptions[i];
                var matchedStruct = FindTranslationStruct(categoryOptionsTranslate, i);
                if (matchedStruct.HasValue)
                {
                    string translatedCat = GetTranslatedString(matchedStruct.Value.optionsTranslates, originalCat, currentLang);
                    categoryTranslationMap[translatedCat] = originalCat;
                    catOptions.Add(translatedCat);
                }
                else
                {
                    categoryTranslationMap[originalCat] = originalCat;
                    catOptions.Add(originalCat);
                }
            }
            filterCategoryDropdown.AddOptions(catOptions);
            filterCategoryDropdown.onValueChanged.AddListener(OnFilterCategoryDropdownChanged);

            // Slot dropdown
            filterSlotDropdown.ClearOptions();
            slotTranslationMap.Clear();
            var slotOpts = new List<string> { allString };
            for (int i = 0; i < slotOptions.Count; i++)
            {
                string origSlot = slotOptions[i];
                var matchedStruct = FindTranslationStruct(slotOptionsTranslate, i);
                if (matchedStruct.HasValue)
                {
                    string translatedSlot = GetTranslatedString(matchedStruct.Value.optionsTranslates, origSlot, currentLang);
                    slotTranslationMap[translatedSlot] = origSlot;
                    slotOpts.Add(translatedSlot);
                }
                else
                {
                    slotTranslationMap[origSlot] = origSlot;
                    slotOpts.Add(origSlot);
                }
            }
            filterSlotDropdown.AddOptions(slotOpts);
            filterSlotDropdown.onValueChanged.AddListener(OnFilterSlotDropdownChanged);
        }

        /// <summary>
        /// Clears and rebuilds the inventory UI based on the purchased items.
        /// </summary>
        public void UpdateInventoryUI()
        {
            ClearInventory();

            // Get purchased items from MonetizationManager
            var purchasedItems = PlayerSave.GetInventoryItems();

            foreach (var purchasedItem in purchasedItems)
            {
                // Get the scriptable object (InventoryItem) by itemId
                var matchingSO = GameInstance.Singleton.inventoryItems
                    .FirstOrDefault(so => so.itemId == purchasedItem.itemId);

                int currentLevel = InventorySave.GetItemUpgradeLevel(purchasedItem.uniqueItemGuid);

                if (matchingSO == null)
                {
                    Debug.LogWarning($"No scriptable for itemId {purchasedItem.itemId}");
                    continue;
                }

                // Filter by Rarity
                if (!string.IsNullOrEmpty(currentRarityFilter) && currentRarityFilter != "All")
                {
                    if (matchingSO.rarity.ToString() != currentRarityFilter)
                        continue;
                }

                // Filter by Category
                if (!string.IsNullOrEmpty(currentCategoryFilter) && currentCategoryFilter != "All")
                {
                    if (matchingSO.category != currentCategoryFilter)
                        continue;
                }

                // Filter by Slot
                if (!string.IsNullOrEmpty(currentSlotFilter) && currentSlotFilter != "All")
                {
                    if (matchingSO.slot != currentSlotFilter)
                        continue;
                }

                // Create Entry
                InventoryEntry entry = Instantiate(inventoryEntryPrefab, inventoryContainer);

                // Setup: now pass uniqueItemGuid and itemLevel from purchasedItem
                entry.Setup(purchasedItem.uniqueItemGuid, matchingSO, currentLevel);
                spawnedEntries.Add(entry);
            }
        }

        private void ClearInventory()
        {
            foreach (Transform child in inventoryContainer)
                Destroy(child.gameObject);

            spawnedEntries.Clear();
        }

        public void DeselectAllEntries()
        {
            foreach (var entry in spawnedEntries)
            {
                entry.Deselect();
            }
        }

        // Filter
        private void OnFilterRarityDropdownChanged(int index)
        {
            string selectedText = filterRarityDropdown.options[index].text;
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            string allString = GetTranslatedString(allOptionTranslated, allOptionFallback, currentLang);

            currentRarityFilter = (selectedText == allString) ? "All" : selectedText;
            UpdateInventoryUI();
        }

        private void OnFilterCategoryDropdownChanged(int index)
        {
            string selectedText = filterCategoryDropdown.options[index].text;
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            string allString = GetTranslatedString(allOptionTranslated, allOptionFallback, currentLang);

            if (selectedText == allString)
            {
                currentCategoryFilter = "All";
            }
            else if (categoryTranslationMap.ContainsKey(selectedText))
            {
                currentCategoryFilter = categoryTranslationMap[selectedText];
            }
            else
            {
                currentCategoryFilter = "All";
            }
            UpdateInventoryUI();
        }

        private void OnFilterSlotDropdownChanged(int index)
        {
            string selectedText = filterSlotDropdown.options[index].text;
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            string allString = GetTranslatedString(allOptionTranslated, allOptionFallback, currentLang);

            if (selectedText == allString)
            {
                currentSlotFilter = "All";
            }
            else if (slotTranslationMap.ContainsKey(selectedText))
            {
                currentSlotFilter = slotTranslationMap[selectedText];
            }
            else
            {
                currentSlotFilter = "All";
            }
            UpdateInventoryUI();
        }

        // Sort
        private void OnSortDropdownChanged(int index)
        {
            switch (index)
            {
                case 0:
                    SortByRarity(false);
                    break;
                case 1:
                    SortByRarity(true);
                    break;
                case 2:
                    SortBySlot();
                    break;
            }
        }

        public void SortByRarity(bool descending)
        {
            if (descending)
                spawnedEntries = spawnedEntries.OrderByDescending(e => e.RarityValue()).ToList();
            else
                spawnedEntries = spawnedEntries.OrderBy(e => e.RarityValue()).ToList();

            ReorderSpawnedEntries();
        }

        public void SortBySlot()
        {
            spawnedEntries = spawnedEntries.OrderBy(e => e.SlotValue()).ToList();
            ReorderSpawnedEntries();
        }

        private void ReorderSpawnedEntries()
        {
            for (int i = 0; i < spawnedEntries.Count; i++)
            {
                spawnedEntries[i].transform.SetSiblingIndex(i);
            }
        }
        /// <summary>
        /// Sends an upgrade request to the backend and refreshes UI based on the result.
        /// </summary>        
        public async void UpgradeItem(string uniqueItemGuid, InventoryItem inventorySO, InventoryItemInfo itemInfo)
        {
            if (string.IsNullOrEmpty(uniqueItemGuid) || inventorySO == null)
                return;

            int oldLevel = InventorySave.GetItemUpgradeLevel(uniqueItemGuid);

            RequestResult res = await InventorySave.UpgradeItemAsync(uniqueItemGuid, inventorySO);

            string lang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            string msg;
            bool isError;

            if (res.Success)
            {
                int newLevel = InventorySave.GetItemUpgradeLevel(uniqueItemGuid);

                if (newLevel > oldLevel)
                {
                    msg = GetTranslatedString(upgradeSuccessTranslated, upgradeSuccessFallback, lang);
                    isError = false;
                }
                else if (newLevel < oldLevel)
                {
                    msg = GetTranslatedString(levelDecreasedTranslated, levelDecreasedFallback, lang);
                    isError = true; 
                }
                else
                {
                    msg = GetTranslatedString(upgradeFailTranslated, upgradeFailFallback, lang);
                    isError = true;
                }
            }
            else
            {
                msg = res.Reason switch
                {
                    "0" => GetTranslatedString(maxLevelReachedTranslated, maxLevelReachedFallback, lang),
                    "1" => GetTranslatedString(insufficientCurrencyTranslated, insufficientCurrencyFallback, lang),
                    "2" or _ => GetTranslatedString(upgradeFailTranslated, upgradeFailFallback, lang),
                };
                isError = true;
            }

            ShowMessage(msg, isError);

            int finalLevel = InventorySave.GetItemUpgradeLevel(uniqueItemGuid);
            if (itemInfo != null)
            {
                bool didLevelUp = finalLevel > oldLevel;
                itemInfo.StartCoroutine(itemInfo.UpgradeProgressRoutine(didLevelUp, 2f));
                itemInfo.UpdateItemInfo(inventorySO, finalLevel);
            }

            UpdateInventoryUI();
        }


        private OptionsTranslate? FindTranslationStruct(OptionsTranslate[] translationsArray, int indexToMatch)
        {
            if (translationsArray == null || translationsArray.Length == 0) return null;
            foreach (var item in translationsArray)
            {
                if (item.optionIndex == indexToMatch)
                {
                    return item;
                }
            }
            return null;
        }

        private IEnumerator ClearMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            messageText.text = string.Empty;
        }

        public void ShowMessage(string msg, bool isError)
        {
            messageText.text = msg;
            messageText.color = isError ? Color.red : Color.green;
            StartCoroutine(ClearMessageAfterDelay(3f));
        }

        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId)
                        && trans.LanguageId.Equals(currentLang)
                        && !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }
    }

    [System.Serializable]
    public struct OptionsTranslate
    {
        public int optionIndex;
        public NameTranslatedByLanguage[] optionsTranslates;
    }
}
