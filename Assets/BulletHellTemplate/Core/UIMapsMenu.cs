using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace BulletHellTemplate
{
    /// <summary>
    /// Manages the maps UI menu, including loading standard and event maps.
    /// Adapted for offline mode by completely removing Firebase dependencies.
    /// </summary>
    public class UIMapsMenu : MonoBehaviour
    {
        #region Map Entry Settings

        /// <summary>
        /// Prefab for standard map entries in the UI.
        /// </summary>
        [Tooltip("Prefab for standard map entries in the UI.")]
        public MapEntry mapEntryPrefab;

        /// <summary>
        /// Container for standard map entries.
        /// </summary>
        [Tooltip("Container for standard map entries.")]
        public Transform mapContainer;

        /// <summary>
        /// Prefab for event map entries in the UI.
        /// </summary>
        [Tooltip("Prefab for event map entries in the UI.")]
        public MapEntry eventMapEntryPrefab;

        /// <summary>
        /// Container for event map entries.
        /// </summary>
        [Tooltip("Container for event map entries.")]
        public Transform eventMapContainer;

        #endregion

        #region Event Map Settings

        /// <summary>
        /// Bypass Firebase check and display all event maps.
        /// </summary>
        [Tooltip("Bypass Firebase check and display all event maps")]
        public bool alwaysShowEventMap;

        #endregion

        #region UI Elements

        /// <summary>
        /// Text component for error messages.
        /// </summary>
        [Tooltip("Text component for error messages")]
        public TextMeshProUGUI errorMessage;

        /// <summary>
        /// Difficulty label.
        /// </summary>
        public string difficulty = "Difficulty:";

        /// <summary>
        /// Translated difficulty labels.
        /// </summary>
        public NameTranslatedByLanguage[] difficultyTranslated;

        /// <summary>
        /// Fallback string for insufficient currency.
        /// </summary>
        public string insufficientCurrency = "Insufficient Tickets!";

        /// <summary>
        /// Translated messages for insufficient currency.
        /// </summary>
        public NameTranslatedByLanguage[] insufficientCurrencyTranslated;

        #endregion

        #region Private Variables

        private List<MapEntry> mapEntries = new List<MapEntry>();
        public static UIMapsMenu Singleton;
        private string currentLang;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            StartCoroutine(LoadMapsCoroutine());
        }

        #endregion

        #region Map Loading

        /// <summary>
        /// Coroutine that loads and processes map entries.
        /// </summary>
        private IEnumerator LoadMapsCoroutine()
        {
            ClearMapEntries();

            if (GameInstance.Singleton == null || GameInstance.Singleton.mapInfoData == null)
            {
                Debug.LogError("Missing GameInstance or map data");
                yield break;
            }
            List<MapInfoData> allMaps = new List<MapInfoData>(GameInstance.Singleton.mapInfoData);

            for (int i = 0; i < allMaps.Count; i++)
            {
                MapInfoData current = allMaps[i];

                if (!current.isEventMap)
                {
                    CreateStandardMapEntry(current);
                }
                else
                {
                    yield return StartCoroutine(ProcessEventMap(current));
                }
            }
        }

        /// <summary>
        /// Creates a standard map entry UI element.
        /// </summary>
        /// <param name="mapData">Map information data.</param>
        /// <returns>The created map entry.</returns>
        private MapEntry CreateStandardMapEntry(MapInfoData mapData)
        {
            MapEntry entry = Instantiate(mapEntryPrefab, mapContainer);
            string mapNameTranslated = GetTranslatedString(mapData.mapNameTranslated, mapData.mapName, currentLang);
            string mapDescriptionTranslated = GetTranslatedString(mapData.mapDescriptionTranslated, mapData.mapDescription, currentLang);
            string difficultyTranslatedStr = GetTranslatedString(difficultyTranslated, difficulty, currentLang);
            entry.Setup(mapData, mapNameTranslated, mapDescriptionTranslated, difficultyTranslatedStr, IsMapUnlocked(mapData));
            mapEntries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Processes event map entries.
        /// </summary>
        /// <param name="mapData">Map information data for event map.</param>
        private IEnumerator ProcessEventMap(MapInfoData mapData)
        {
            bool shouldDisplay = alwaysShowEventMap;

            if (shouldDisplay)
            {
                MapEntry eventEntry = Instantiate(eventMapEntryPrefab, eventMapContainer);
                string mapNameTranslated = GetTranslatedString(mapData.mapNameTranslated, mapData.mapName, currentLang);
                string mapDescriptionTranslated = GetTranslatedString(mapData.mapDescriptionTranslated, mapData.mapDescription, currentLang);
                string difficultyTranslatedStr = GetTranslatedString(difficultyTranslated, difficulty, currentLang);
                eventEntry.Setup(mapData, mapNameTranslated, mapDescriptionTranslated, difficultyTranslatedStr, IsMapUnlocked(mapData));
                mapEntries.Add(eventEntry);
            }
            yield return null;
        }

        /// <summary>
        /// Clears all map entries from the UI containers.
        /// </summary>
        private void ClearMapEntries()
        {
            foreach (Transform child in mapContainer)
                Destroy(child.gameObject);
            foreach (Transform child in eventMapContainer)
                Destroy(child.gameObject);
            mapEntries.Clear();
        }

        /// <summary>
        /// Reloads the map entries.
        /// </summary>
        public void ReloadMaps()
        {
            StartCoroutine(LoadMapsCoroutine());
        }

        #endregion

        #region Map Unlocking and Translation

        /// <summary>
        /// Determines if a map is unlocked.
        /// </summary>
        /// <param name="mapData">Map information data.</param>
        /// <returns>True if unlocked; otherwise, false.</returns>
        /// <summary>Retorna true se o mapa estiver jogável.</summary>
        private bool IsMapUnlocked(MapInfoData mapData)
        {
            if (mapData.isUnlocked) return true;
            if (PlayerSave.GetUnlockedMaps().Contains(mapData.mapId)) return true;

            foreach (var m in GameInstance.Singleton.mapInfoData)
            {
                if (m.isEventMap) continue;   
                return m.mapId == mapData.mapId; 
            }
            return false;
        }

        /// <summary>
        /// Retrieves a translated string based on the current language.
        /// </summary>
        /// <param name="translations">Array of translations.</param>
        /// <param name="fallback">Fallback string if no translation is found.</param>
        /// <param name="currentLang">Current language ID.</param>
        /// <returns>Translated string.</returns>
        private string GetTranslatedString(NameTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        /// <summary>
        /// Retrieves a translated string for map descriptions based on the current language.
        /// </summary>
        /// <param name="translations">Array of description translations.</param>
        /// <param name="fallback">Fallback description if no translation is found.</param>
        /// <param name="currentLang">Current language ID.</param>
        /// <returns>Translated description.</returns>
        private string GetTranslatedString(DescriptionTranslatedByLanguage[] translations, string fallback, string currentLang)
        {
            if (translations != null)
            {
                foreach (var trans in translations)
                {
                    if (!string.IsNullOrEmpty(trans.LanguageId) &&
                        trans.LanguageId.Equals(currentLang) &&
                        !string.IsNullOrEmpty(trans.Translate))
                    {
                        return trans.Translate;
                    }
                }
            }
            return fallback;
        }

        #endregion

        #region Error Display

        /// <summary>
        /// Displays an error message for a specified duration.
        /// </summary>
        /// <param name="duration">Duration in seconds to display the error message.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        public IEnumerator ShowErrorMessage(float duration)
        {
            string errorTranslated = GetTranslatedString(insufficientCurrencyTranslated, insufficientCurrency, currentLang);
            errorMessage.text = errorTranslated;
            yield return new WaitForSeconds(duration);
            errorMessage.text = "";
        }

        #endregion
    }
}
