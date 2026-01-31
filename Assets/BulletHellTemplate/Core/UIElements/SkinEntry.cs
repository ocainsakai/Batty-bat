using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents a UI entry for a character skin.
    /// </summary>
    public class SkinEntry : MonoBehaviour
    {
        [Tooltip("Image displaying the skin icon.")]
        public Image skinIcon;
        [Tooltip("Image indicating if the skin is locked.")]
        public Image lockedSkin;
        [Tooltip("Image indicating if the skin is selected.")]
        public Image selectedSkin;
        [Tooltip("Text displaying the skin name.")]
        public TextMeshProUGUI skinName;

        private int skinIndex;

        /// <summary>
        /// Configures the SkinEntry with the given skin data.
        /// </summary>
        /// <param name="skin">The character skin data.</param>
        /// <param name="index">The index of the skin.</param>
        /// <param name="isUnlocked">Indicates if the skin is unlocked.</param>
        public void SetupSkinEntry(CharacterSkin skin, int index, bool isUnlocked)
        {
            skinIndex = index;
            if (skinIcon != null)
                skinIcon.sprite = skin.skinIcon;
            string currentLang = LanguageManager.LanguageManager.Instance.GetCurrentLanguage();
            skinName.text = UICharacterMenu.Singleton.GetTranslatedString(skin.skinNameTranslated, skin.skinName, currentLang);
            if (lockedSkin != null)
                lockedSkin.gameObject.SetActive(!isUnlocked);
        }

        /// <summary>
        /// Handles the selection of this skin.
        /// </summary>
        public void OnClickSelectSkin()
        {
            if (UICharacterMenu.Singleton != null)
            {
                 UICharacterMenu.Singleton.ChangeCharacterSkin(skinIndex);
                if (UICharacterMenu.Singleton.UISkins != null)
                    UICharacterMenu.Singleton.UISkins.SetActive(false);
            }
        }
    }
}
