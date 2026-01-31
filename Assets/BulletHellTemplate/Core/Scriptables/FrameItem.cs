using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents the data for a Frame in the game.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFrameData", menuName = "BulletHellTemplate/Frame Data/Create Frame Data", order = 52)]
    public class FrameItem : ScriptableObject
    {
        public string frameName;
        public NameTranslatedByLanguage[] frameNameTranslated;
        public string frameId;
        public Sprite icon;
        public bool isUnlocked;
    }
}
