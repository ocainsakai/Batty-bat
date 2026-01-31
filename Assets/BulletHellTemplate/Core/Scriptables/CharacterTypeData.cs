using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Represents data for a character type.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterTypeData", menuName = "Character Type Data", order = 51)]
    public class CharacterTypeData : ScriptableObject
    {
        public string typeName; // The name of the character type (e.g., Fire, Water, etc.)
        public NameTranslatedByLanguage typeNameTranslated;
        public Sprite icon; // The icon representing the character type
        public CharacterTypeData[] weaknesses; // Array of weaknesses (other character types this type is weak against)
        public CharacterTypeData[] strengths; // Array of strengths (other character types this type is strong against)
    }
}
