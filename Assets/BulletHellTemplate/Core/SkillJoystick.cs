using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles skill activation using a joystick for mobile platforms.
    /// </summary>
    public class SkillJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform joystickTransform;
        [SerializeField] private RectTransform backgroundTransform;
        [SerializeField] private Image cooldownImage;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Image skillIcon;

        public bool hideCooldownImage;

        protected Vector2 inputVector;
        protected float handleLimit = 1.0f;
        protected float cooldownTime;
        protected float currentCooldown;
        protected bool isOnCooldown = false;
        protected CharacterEntity characterEntity;
        public int skillIndex;

        public float Horizontal => inputVector.x;
        public float Vertical => inputVector.y;

        private void Start()
        {
            if (backgroundTransform == null)
                backgroundTransform = GetComponent<RectTransform>();

            joystickTransform.gameObject.SetActive(true);
            skillIcon.gameObject.SetActive(true);
            cooldownImage.fillAmount = 0;
            cooldownText.text = "";
        }

        /// <summary>
        /// Configures the joystick with a character entity and initializes the skill icon and cooldown.
        /// </summary>
        /// <param name="entity">Character entity to link with the joystick.</param>
        public void Setup(CharacterEntity entity)
        {
            characterEntity = entity;
            cooldownTime = characterEntity.GetSkillCooldown(skillIndex);
            skillIcon.sprite = characterEntity.GetSkillIcon(skillIndex);
        }

        /// <summary>
        /// Returns the current cooldown time.
        /// </summary>
        public float GetCurrentCooldown() => currentCooldown;

        /// <summary>
        /// Returns the initial cooldown time.
        /// </summary>
        public float GetCooldownTime() => cooldownTime;

        /// <summary>
        /// Handles the drag event for the joystick.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (isOnCooldown) return;

            Vector2 direction = eventData.position - RectTransformUtility.WorldToScreenPoint(null, backgroundTransform.position);
            inputVector = (direction.magnitude > backgroundTransform.sizeDelta.x / 2.0f) ? direction.normalized : direction / (backgroundTransform.sizeDelta.x / 2.0f);
            joystickTransform.anchoredPosition = (inputVector * backgroundTransform.sizeDelta.x / 2.0f) * handleLimit;

            // Update the directional aim while the joystick is being dragged
            characterEntity.UpdateDirectionalAim(inputVector, skillIndex);
        }

        /// <summary>
        /// Handles the pointer down event for the joystick.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (GameplayManager.Singleton != null && GameplayManager.Singleton.IsPaused())
                return;

            bool hasEnoughMP =  characterEntity.GetCurrentMP() > characterEntity.GetSkillData(skillIndex).manaCost;

            if (!isOnCooldown && hasEnoughMP)
            {
                joystickTransform.gameObject.SetActive(true);

                // Activate the directional aim when the joystick is touched
                characterEntity.SetDirectionalAimActive(true, skillIndex,false);
            }
            OnDrag(eventData);
        }

        /// <summary>
        /// Handles the pointer up event for the joystick.
        /// </summary>
        /// <param name="eventData">Pointer event data.</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (GameplayManager.Singleton != null && GameplayManager.Singleton.IsPaused())
                return;

            if (isOnCooldown) return;

            if(characterEntity.GetSkillData(skillIndex).manaCost > characterEntity.GetCurrentMP())
            {
                characterEntity.DisplayMPInsufficientMessage("Not enough MP to use skill");
                UIGameplay.Singleton.DisplayMPInsufficientMessage("Not enough MP to use skill");
                inputVector = Vector2.zero;
                joystickTransform.anchoredPosition = Vector2.zero;
                return;
            }

            if (characterEntity != null)
            {
                characterEntity.UseSkill(skillIndex, inputVector);
            }

            inputVector = Vector2.zero;
            joystickTransform.anchoredPosition = Vector2.zero;

            // Deactivate the directional aim when the joystick is released
            characterEntity.SetDirectionalAimActive(false, skillIndex,true);

            StartCooldown();
        }

        /// <summary>
        /// Initiates the cooldown for the skill, adjusting for any cooldown reduction.
        /// </summary>
        public void StartCooldown()
        {
            isOnCooldown = true;
            float cooldownReduction = characterEntity.GetCurrentCooldownReduction() / 100f;
            currentCooldown = cooldownTime * (1 - cooldownReduction);
            cooldownImage.fillAmount = 1;
            cooldownImage.gameObject.SetActive(true);
            skillIcon.gameObject.SetActive(true);
            StartCoroutine(CooldownRoutine());
        }

        /// <summary>
        /// Manages the cooldown period, updating the UI accordingly.
        /// </summary>
        private IEnumerator CooldownRoutine()
        {
            while (currentCooldown > 0)
            {
                if (GameplayManager.Singleton != null && !GameplayManager.Singleton.IsPaused())
                {
                    currentCooldown -= Time.deltaTime;
                    cooldownImage.fillAmount = currentCooldown / (cooldownTime * (1 - characterEntity.GetCurrentCooldownReduction() / 100f));
                    cooldownText.text = Mathf.Ceil(currentCooldown).ToString();
                }
                yield return null;
            }

            cooldownImage.fillAmount = 0;
            cooldownText.text = "";
            isOnCooldown = false;
            if(hideCooldownImage)
            cooldownImage.gameObject.SetActive(false);
        }

    }
}
