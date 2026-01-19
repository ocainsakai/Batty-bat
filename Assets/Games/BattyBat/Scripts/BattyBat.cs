using UnityEngine;

namespace Games.BattyBat
{
    public class BattyBat : MonoBehaviour
    {
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] Collider2D _col;

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            _rb.AddForceY(10f, ForceMode2D.Impulse);
            Environment.Instance.VisionMode = VisionMode.EchoMode;
        }    
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Environment.Instance.VisionMode = VisionMode.BlindMode;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            GameManager.Instance.GameOver();
        }
    }
    }
}
