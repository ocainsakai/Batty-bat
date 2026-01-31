using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles skill activation for PC using keyboard and mouse inputs.
    /// Manages skill activation, aiming, cooldowns, and prevents skill spamming by enforcing a delay between skill uses.
    /// </summary>
    public class PCSkillController : MonoBehaviour
    {
        [Tooltip("Mappings for each skill's keyboard and mouse buttons")]
        public List<SkillKeyMapping> skillKeyMappings;

        [Tooltip("Delay before allowing the next skill to be used to prevent spamming.")]
        public float delayToNextSkill = 0.3f;

        private bool[] isSkillActive;    // Tracks whether a skill is being held
        private bool[] isOnCooldown;     // Tracks whether a skill is on cooldown
        private float[] cooldownTime;    // Initial cooldown times per skill
        private float[] currentCooldown; // Current cooldowns per skill
        private CharacterEntity characterEntity; // Reference to the character entity

        private bool canUseSkill = true; // Flag to determine if a new skill can be used

        private void Awake()
        {
            int skillCount = skillKeyMappings.Count;
            isSkillActive = new bool[skillCount];
            isOnCooldown = new bool[skillCount];
            cooldownTime = new float[skillCount];
            currentCooldown = new float[skillCount];
        }

        private void Start()
        {
            characterEntity = UIGameplay.Singleton.GetCharacterEntity();
            if (characterEntity == null)
            {
                Debug.LogError("CharacterEntity is not assigned or missing.");
            }

            for (int i = 0; i < skillKeyMappings.Count; i++)
            {
                int skillIndex = skillKeyMappings[i].skillIndex;
                cooldownTime[i] = characterEntity.GetSkillCooldown(skillIndex);
                currentCooldown[i] = 0f;
                isOnCooldown[i] = false;
            }
        }

        private void Update()
        {
            if (characterEntity == null) return;

            for (int i = 0; i < skillKeyMappings.Count; i++)
            {
                UpdateSkillCooldown(i);
                CheckInputForSkill(i);
            }
        }

        /// <summary>
        /// Sets up the character entity and initializes skill cooldown data.
        /// </summary>
        /// <param name="entity">The character entity to associate with this controller.</param>
        public void Setup(CharacterEntity entity)
        {
            characterEntity = entity;

            for (int i = 0; i < skillKeyMappings.Count; i++)
            {
                int skillIndex = skillKeyMappings[i].skillIndex;
                cooldownTime[i] = characterEntity.GetSkillCooldown(skillIndex);
                currentCooldown[i] = 0f;
                isOnCooldown[i] = false;
            }
        }

        /// <summary>
        /// Retrieves the current cooldown for a specified skill.
        /// </summary>
        /// <param name="skillIndex">The skill index to query.</param>
        /// <returns>The current cooldown time remaining for the skill.</returns>
        public float GetCurrentCooldown(int skillIndex)
        {
            int mappingIndex = skillKeyMappings.FindIndex(mapping => mapping.skillIndex == skillIndex);
            if (mappingIndex >= 0)
            {
                return currentCooldown[mappingIndex];
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Retrieves the initial cooldown time for a specified skill.
        /// </summary>
        /// <param name="skillIndex">The skill index to query.</param>
        /// <returns>The initial cooldown time of the skill.</returns>
        public float GetCooldownTime(int skillIndex)
        {
            int mappingIndex = skillKeyMappings.FindIndex(mapping => mapping.skillIndex == skillIndex);
            if (mappingIndex >= 0)
            {
                return cooldownTime[mappingIndex];
            }
            else
            {
                return 0f;
            }
        }

        /// <summary>
        /// Checks input for skill activation, holding, and release events.
        /// </summary>
        private void CheckInputForSkill(int index)
        {
            if (GameplayManager.Singleton != null && GameplayManager.Singleton.IsPaused())
                return;

            SkillKeyMapping skillMapping = skillKeyMappings[index];

            foreach (Key key in skillMapping.keyboardKeys)
            {
                if (key == Key.None) continue;

                if (Keyboard.current[key].wasPressedThisFrame)
                    ActivateSkill(index);
                else if (Keyboard.current[key].isPressed)
                    UpdateSkillAim(index);
                else if (Keyboard.current[key].wasReleasedThisFrame)
                    ReleaseSkill(index);
            }

            foreach (MouseButton mouseButton in skillMapping.mouseButtons)
            {
                if (IsMouseButtonPressed(mouseButton))
                    ActivateSkill(index);
                else if (IsMouseButtonHeld(mouseButton))
                    UpdateSkillAim(index);
                else if (IsMouseButtonReleased(mouseButton))
                    ReleaseSkill(index);
            }
        }

        /// <summary>
        /// Activates the skill if not on cooldown, if a skill can be used, and enables directional aiming.
        /// </summary>
        private void ActivateSkill(int index)
        {
            if (!isSkillActive[index] && !isOnCooldown[index] && canUseSkill)
            {
                int skillIndex = skillKeyMappings[index].skillIndex;
                if (characterEntity.GetSkillData(skillIndex).manaCost > characterEntity.GetCurrentMP())
                {
                    characterEntity.DisplayMPInsufficientMessage("Not enough MP to use skill");
                    UIGameplay.Singleton.DisplayMPInsufficientMessage("Not enough MP to use skill");
                    return;
                }

                isSkillActive[index] = true;
                characterEntity.SetDirectionalAimActive(true, index,true); // Enable directional aiming
                canUseSkill = false; // Prevent other skills from being activated immediately
                StartCoroutine(SkillDelay()); // Start the delay coroutine
            }
        }

        /// <summary>
        /// Coroutine to handle the delay before allowing the next skill to be used.
        /// </summary>
        private IEnumerator SkillDelay()
        {
            yield return new WaitForSeconds(delayToNextSkill);
            canUseSkill = true;
        }

        /// <summary>
        /// Updates the aiming direction while a skill is being held.
        /// </summary>
        private void UpdateSkillAim(int index)
        {
            if (isSkillActive[index])
            {
                Vector2 direction = GetMouseDirection();
                characterEntity.UpdateDirectionalAim(direction, index);
            }
        }

        /// <summary>
        /// Releases the skill, triggers its use, starts cooldown, and disables directional aiming.
        /// </summary>
        private void ReleaseSkill(int index)
        {
            if (isSkillActive[index])
            {
                Vector2 direction = GetMouseDirection();
                int skillMappingIndex = skillKeyMappings[index].skillIndex;

                characterEntity.UseSkill(skillMappingIndex, direction);
                StartCooldown(index);

                isSkillActive[index] = false;
                characterEntity.SetDirectionalAimActive(false, index, true); // Disable directional aiming
            }
        }

        /// <summary>
        /// Starts the cooldown for a specific skill, adjusting for any cooldown reduction.
        /// </summary>
        private void StartCooldown(int index)
        {
            isOnCooldown[index] = true;
            float cooldownReduction = characterEntity.GetCurrentCooldownReduction() / 100f;
            currentCooldown[index] = cooldownTime[index] * (1 - cooldownReduction);
        }

        /// <summary>
        /// Updates the cooldown timer for a specific skill.
        /// </summary>
        private void UpdateSkillCooldown(int index)
        {
            if (GameplayManager.Singleton != null && GameplayManager.Singleton.IsPaused())
                return;

            if (isOnCooldown[index])
            {
                currentCooldown[index] -= Time.deltaTime;

                if (currentCooldown[index] <= 0f)
                {
                    currentCooldown[index] = 0f;
                    isOnCooldown[index] = false;
                }
            }
        }

        /// <summary>
        /// Calculates the aiming direction based on the mouse position.
        /// </summary>
        private Vector2 GetMouseDirection()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane plane = new Plane(Vector3.up, characterEntity.transform.position);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 direction = point - characterEntity.transform.position;
                return new Vector2(direction.x, direction.z).normalized;
            }
            return Vector2.zero;
        }

        private bool IsMouseButtonPressed(MouseButton mouseButton)
        {
            return mouseButton switch
            {
                MouseButton.Left => Mouse.current.leftButton.wasPressedThisFrame,
                MouseButton.Right => Mouse.current.rightButton.wasPressedThisFrame,
                MouseButton.Middle => Mouse.current.middleButton.wasPressedThisFrame,
                _ => false,
            };
        }

        private bool IsMouseButtonHeld(MouseButton mouseButton)
        {
            return mouseButton switch
            {
                MouseButton.Left => Mouse.current.leftButton.isPressed,
                MouseButton.Right => Mouse.current.rightButton.isPressed,
                MouseButton.Middle => Mouse.current.middleButton.isPressed,
                _ => false,
            };
        }

        private bool IsMouseButtonReleased(MouseButton mouseButton)
        {
            return mouseButton switch
            {
                MouseButton.Left => Mouse.current.leftButton.wasReleasedThisFrame,
                MouseButton.Right => Mouse.current.rightButton.wasReleasedThisFrame,
                MouseButton.Middle => Mouse.current.middleButton.wasReleasedThisFrame,
                _ => false,
            };
        }
    }

    [System.Serializable]
    public class SkillKeyMapping
    {
        public int skillIndex;
        public List<Key> keyboardKeys = new List<Key> { Key.None };
        public List<MouseButton> mouseButtons = new List<MouseButton>();
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }
}
