using System;
using UnityEngine;

namespace BulletHellTemplate
{
#if FUSION2
    using Fusion;
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
#endif
    [RequireComponent(typeof(CharacterStatsComponent))]
    [RequireComponent(typeof(CharacterAttackComponent))]
    [RequireComponent(typeof(CharacterBuffsComponent))]
    [RequireComponent(typeof(CharacterUIHandlerComponent))]
    public abstract class BaseCharacterEntity :
#if FUSION2
        NetworkBehaviour
#else
        MonoBehaviour
#endif
    {
        // Components
        protected CharacterStatsComponent characterStatsComponent;
        protected CharacterControllerComponent characterControllerComponent;
        protected CharacterAttackComponent characterAttackComponent;
        protected CharacterBuffsComponent characterBuffsComponent;
        protected CharacterUIHandlerComponent characterUIHandlerComponent;

        public CharacterAttackComponent CharacterAttackComponent => characterAttackComponent;
        public CharacterControllerComponent CharacterControllerComponent => characterControllerComponent;

        protected CharacterData characterData;
        protected int skinIndex;
        protected Animator animator;
        protected CharacterModel characterModel;
        protected bool canAutoAttack = true;
        protected bool isInvincible = false;
        protected int activeBuffCount = 0;
        protected bool isMovementStopped = false;
        protected bool isDebuffActive = false;
        protected bool isGameStarted = false;

        /// <summary>
        /// True when this character has already died (OnDeath executed).
        /// </summary>
        public bool IsDead { get; protected set; } = false;

        protected bool IsBuffActive => activeBuffCount > 0;

        protected virtual void Awake()
        {
            characterStatsComponent = GetComponent<CharacterStatsComponent>();
            characterControllerComponent = GetComponent<CharacterControllerComponent>();
            characterAttackComponent = GetComponent<CharacterAttackComponent>();
            characterUIHandlerComponent = GetComponent<CharacterUIHandlerComponent>();
        }

        protected virtual void Start() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() { }

        public virtual void Move(Vector3 direction) { }
        public virtual void UseSkill(int index, Vector2 inputDir) { }
        public virtual void Attack() { }
        public virtual void ReceiveDamage(float amount) { }
        public virtual void ApplyInvincible(float duration) { }

        /// <summary>
        /// Called once when character HP reaches 0 (or below). Must be idempotent.
        /// </summary>
        public virtual void OnDeath() { }

        public virtual void OnCharacterRevive() { }
    }
}
