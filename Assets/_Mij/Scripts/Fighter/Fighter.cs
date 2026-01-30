using UnityEngine;

public class Fighter : MonoBehaviour
{
    public FighterDefinition fighterDefinition;
    public FighterType fighterType;

    public void Initialize(FighterDefinition definition, FighterType type)
    {
        fighterDefinition = definition;
        fighterType = type;
        // Additional initialization logic here
    }
    
}