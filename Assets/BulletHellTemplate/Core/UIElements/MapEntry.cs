using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BulletHellTemplate
{
    public class MapEntry : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image needCurrencyIcon;
        public TextMeshProUGUI needCurrencyValue;
        [Tooltip("GameObject for standard map display")]
        public GameObject normalMap;
        [Tooltip("GameObject for currency-required map display")]
        public GameObject needCurrencyMap;

        [Header("Visual Elements")]
        [Tooltip("Preview image of the map")]
        public Image mapPreviewImage;
        [Tooltip("Locked state overlay image")]
        public Image lockedMapImage;
        [Tooltip("Text component for map name")]
        public TextMeshProUGUI mapNameText;
        [Tooltip("Text component for map description")]
        public TextMeshProUGUI mapDescriptionText;
        [Tooltip("Text component for difficulty rating")]
        public TextMeshProUGUI mapDifficultyText;
        [Tooltip("Interaction button for map selection")]
        public Button mapSelectButton;

        [Header("Visual States")]
        [Tooltip("Color when map is unlocked")]
        public Color unlockedColor = Color.white;
        [Tooltip("Color when map is locked")]
        public Color lockedColor = Color.gray;

        private MapInfoData _mapData;

        public void Setup(MapInfoData mapData, string _mapName, string _mapDescription, string difficulty, bool isUnlocked)
        {
            _mapData = mapData;
            if (mapNameText != null) mapNameText.text = _mapName;
            if (mapDescriptionText != null) mapDescriptionText.text = _mapDescription;
            if (mapDifficultyText != null) mapDifficultyText.text = $"{difficulty} {_mapData.difficultyRating}/5";
            if (mapPreviewImage != null) mapPreviewImage.sprite = _mapData.mapPreviewImage;

            if (_mapData.isNeedCurrency)
            {
                if (needCurrencyIcon != null)
                {
                    needCurrencyIcon.sprite = MonetizationManager.Singleton.GetCurrencyIcon(_mapData.currency.coinID);
                }
                if (needCurrencyValue != null)
                {
                    needCurrencyValue.text = _mapData.amount.ToString();
                }
            }
            if (normalMap != null) normalMap.SetActive(!_mapData.isNeedCurrency);
            if (needCurrencyMap != null) needCurrencyMap.SetActive(_mapData.isNeedCurrency);

            mapSelectButton.interactable = isUnlocked;
            mapPreviewImage.color = isUnlocked ? unlockedColor : lockedColor;
            lockedMapImage.gameObject.SetActive(!isUnlocked);           
        }

        public void OnMapSelected()
        {
            if (!CheckMapAccess()) return;

            if (GameManager.Singleton != null)
            {
                GameManager.Singleton.StartGame(_mapData.mapId);
            }
        }

        private bool CheckMapAccess()
        {
            if (_mapData.isNeedCurrency)
            {
                int currentBalance = MonetizationManager.GetCurrency(_mapData.currency.coinID);
                if (currentBalance < _mapData.amount)
                {
                    StartCoroutine(UIMapsMenu.Singleton.ShowErrorMessage(1f));
                    return false;
                }

                int newBalance = currentBalance - _mapData.amount;
                MonetizationManager.SetCurrency(_mapData.currency.coinID, newBalance);
            }
            return true;
        }
    }
}