using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents the data for a Icon in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewIconData", menuName = "BulletHellTemplate/Icon Data/Create Icon Data", order = 52)]
    public class IconItem : ScriptableObject
    {
        public string iconName;
        public NameTranslatedByLanguage[] iconNameTranslated;
        public string iconId;
        public Sprite icon;
        public bool isUnlocked;
    }
}
