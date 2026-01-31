using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles player movement using configurable keys for each direction and mouse aiming for PC with the new Input System.
    /// </summary>
    public class PCInputController : MonoBehaviour
    {
        private CharacterEntity characterEntity;
        private Camera mainCamera;

        [Header("Directional Keys")]
        [Tooltip("Keys for moving upwards")]
        public List<Key> upKeys = new List<Key> { Key.W, Key.UpArrow };

        [Tooltip("Keys for moving downwards")]
        public List<Key> downKeys = new List<Key> { Key.S, Key.DownArrow };

        [Tooltip("Keys for moving left")]
        public List<Key> leftKeys = new List<Key> { Key.A, Key.LeftArrow };

        [Tooltip("Keys for moving right")]
        public List<Key> rightKeys = new List<Key> { Key.D, Key.RightArrow };

        private void Start()
        {
#if UNITY_6000_0_OR_NEWER           
            characterEntity = FindFirstObjectByType<CharacterEntity>();
#else
    
            CharacterEntity characterEntity = FindObjectOfType<CharacterEntity>();
#endif
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (characterEntity != null)
            {
                // Get movement direction from configured keys
                Vector3 direction = GetMovementInput();
                characterEntity.Move(direction);

                // Update aim direction based on mouse position
                UpdateAimDirection();
            }
        }

        /// <summary>
        /// Gets the movement input based on the configured keys.
        /// </summary>
        /// <returns>A Vector3 representing the movement direction.</returns>
        private Vector3 GetMovementInput()
        {
            float moveX = 0f;
            float moveZ = 0f;

            if (IsAnyKeyPressed(upKeys)) moveZ += 1f;
            if (IsAnyKeyPressed(downKeys)) moveZ -= 1f;
            if (IsAnyKeyPressed(rightKeys)) moveX += 1f;
            if (IsAnyKeyPressed(leftKeys)) moveX -= 1f;

            return new Vector3(moveX, 0, moveZ).normalized;
        }

        /// <summary>
        /// Checks if any key in a list of keys is pressed.
        /// </summary>
        private bool IsAnyKeyPressed(List<Key> keys)
        {
            foreach (var key in keys)
            {
                if (Keyboard.current[key].isPressed)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the character's aim direction based on mouse position.
        /// </summary>
        private void UpdateAimDirection()
        {
            Vector2 aimDirection = GetMouseAimDirection();
           // characterEntity.UpdateDirectionalAim(aimDirection);
        }

        /// <summary>
        /// Calculates the aim direction from the character to the mouse position.
        /// </summary>
        /// <returns>Normalized direction vector.</returns>
        private Vector2 GetMouseAimDirection()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, characterEntity.transform.position);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 direction = point - characterEntity.transform.position;
                return new Vector2(direction.x, direction.z).normalized;
            }
            return Vector2.zero;
        }
    }
}
