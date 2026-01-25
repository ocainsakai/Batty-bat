using UnityEngine;

public enum FighterType
{
    Player,
    Enemy
}
public enum FighterClass
{
    Warrior,
    Mage,
    Archer
}
public enum FighterAttribute
{
    Vong,
    Linh,
    Yeu,
    Quai,
}
[CreateAssetMenu(fileName = "FighterDefinition", menuName = "Mij/Fighter/FighterDefinition")]
public class FighterDefinition : ScriptableObject
{
    public string fighterName => name;
    public FighterClass fighterClass;
    public FighterAttribute fighterAttribute;
}
