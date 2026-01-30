using Core.Patterns.ObjectPool;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] private GameObject fighterPrefab;
    private ComponentPool<Fighter> _fighterPool;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_fighterPool == null) _fighterPool = PoolManager.Instance.CreatePool<Fighter>("FighterPool", fighterPrefab, initialSize: 20, maxSize: 100);
        _fighterPool.Warmup(10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
