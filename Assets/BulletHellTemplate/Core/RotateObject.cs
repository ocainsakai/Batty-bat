using UnityEngine;

namespace MonsterTaming
{
    /// <summary>
    /// Rotates an object around its local axis at a specified speed.
    /// </summary>
    public class RotateObject : MonoBehaviour
    {
        // Speed of rotation in degrees per second
        [SerializeField]
        private Vector3 rotationSpeed = new Vector3(0, 100f, 0);

        /// <summary>
        /// Rotates the object every frame.
        /// </summary>
        void Update()
        {
            // Rotate the object based on the defined speed and delta time
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
