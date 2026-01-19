using UnityEngine;
using UnityEngine.Pool; // Bắt buộc dòng này để dùng ObjectPool

public class Rosin : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    
    [SerializeField] float _deadZoneX = -10f; 

    public static float RosinSpeed = 5f; 
    
    private IObjectPool<Rosin> _pool;
    private bool _hasPassedPlayer;

    public void SetPool(IObjectPool<Rosin> pool)
    {
        _pool = pool;
    }

    private void OnEnable()
    {
        _hasPassedPlayer = false;
    }

    void Update()
    {
        _rb.linearVelocityX = -RosinSpeed;

        if (transform.position.x < _deadZoneX)
        {
            ReturnToPool();
        }

        if (!_hasPassedPlayer && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            // Assuming BattyBat is available via GameManager, or finding it if not.
            // For safety, let's assume if x < -4 (typical player position is left side) it's passed.
            // Better: access player transform from GameManager if exposed.
            if (GameManager.Instance.BattyBat != null && transform.position.x < GameManager.Instance.BattyBat.transform.position.x)
            {
                _hasPassedPlayer = true;
                GameManager.Instance.AddScore(1);
            }
        }
    }

    private void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(this); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}