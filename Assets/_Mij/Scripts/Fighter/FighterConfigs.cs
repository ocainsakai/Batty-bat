using System;
using UnityEngine;

[Serializable]
public class FighterConfig
{
    public FighterDefinition fighterDefinitions;
    public float AttackRange;
    public float SightRange;
    public float PatrolRadius;
    public float MoveSpeed;
    public float AttackSpeed;
    public int MaxHealth;
    public int Damage;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "FighterConfigs", menuName = "Mij/Fighter/FighterConfigs")]
public class FighterConfigs : ScriptableObject
{
    public FighterConfig[] fighterDefinitions;
}