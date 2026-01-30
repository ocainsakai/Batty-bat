using UnityEngine;

namespace Games.BattyBat
{
    public class BattyBat : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Collider2D _col;
        [SerializeField] private float _jumpForce = 10f;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _rb.AddForceY(_jumpForce, ForceMode2D.Impulse);
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
