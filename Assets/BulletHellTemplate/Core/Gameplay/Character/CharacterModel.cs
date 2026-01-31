using UnityEngine;
using BulletHellTemplate.Core.FSM;
using System.Collections.Generic;
using System;
using BulletHellTemplate.Core.Events;
using UnityEngine.Events;

namespace BulletHellTemplate
{
    /// <summary>
    /// MonoBehaviour placed on the character passEntryPrefab. It exposes animation data to the FSM
    /// and keeps backward-compatibility with traditional AnimatorController workflows.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterAnimationFSM))]
    public sealed class CharacterModel : MonoBehaviour, ICharacterAnimationContext
    {
        [Header("Animator References")]
        [SerializeField] private Animator animator;

        [Header("Mode Selection")]
        [Tooltip("If true, use HFSM (CharacterAnimationFSM). Otherwise keep AnimatorController workflow.")]
        [SerializeField] private bool useHFSM = true;

        [Header("Directional Sets (8-Way)")]
        public DirectionalAnimSet idleSet;
        public DirectionalAnimSet moveSet;

        [Header("Combat Clips")]
        public SkillAnimData attack;
        public AnimClipData receiveDamage;
        public AnimClipData death;

        [Header("Skill Clips")]
        public SkillAnimData[] skillAnimations;

        private CharacterAnimationFSM fsm;

        public UnityEvent OnEnter;
        public UnityEvent OnExit;
        public UnityEvent OnUpgrade;

        #region Unity lifecycle
        private void OnEnable()
        {
            EventBus.Subscribe<AnimationOnRunEvent>(OnRun);
            EventBus.Subscribe<AnimationOnIdleEvent>(OnIdle);
            EventBus.Subscribe<AnimationOnActionEvent>(OnAction);
            EventBus.Subscribe<AnimationOnActionEndEvent>(OnActionEnd);
            EventBus.Subscribe<AnimationOnDiedEvent>(OnDeath);
            OnEnter?.Invoke();
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AnimationOnRunEvent>(OnRun);
            EventBus.Unsubscribe<AnimationOnIdleEvent>(OnIdle);
            EventBus.Unsubscribe<AnimationOnActionEvent>(OnAction);
            EventBus.Unsubscribe<AnimationOnActionEndEvent>(OnActionEnd);
            EventBus.Unsubscribe<AnimationOnDiedEvent>(OnDeath);

            OnExit?.Invoke();
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();

            // ---------- 1. AnimatorOverrideController ----------
            var baseCtrl = animator.runtimeAnimatorController;
            var aoc = new AnimatorOverrideController(baseCtrl);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            void Override(string state, AnimationClip clip)
            {
                if (clip == null) return;
                var original = Array.Find(aoc.animationClips, c => c.name == state);
                if (original != null)
                    overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(original, clip));
            }

            // Helper
            string[] dirSuffix = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

            AnimClipData[] idleClips = {
                idleSet.Forward,       idleSet.ForwardRight, idleSet.Right,      idleSet.BackRight,
                idleSet.Back,          idleSet.BackLeft,     idleSet.Left,       idleSet.ForwardLeft
            };

            AnimClipData[] moveClips = {
                moveSet.Forward,       moveSet.ForwardRight, moveSet.Right,      moveSet.BackRight,
                moveSet.Back,          moveSet.BackLeft,     moveSet.Left,       moveSet.ForwardLeft
            };

            // Para Idle_Blend
            for (int i = 0; i < dirSuffix.Length; i++)
            {
                var clip = idleClips[i].Clip ?? idleSet.Forward.Clip;
                Override($"Idle_{dirSuffix[i]}", clip);
            }

            // Para Run_Blend
            for (int i = 0; i < dirSuffix.Length; i++)
            {
                var clip = moveClips[i].Clip ?? moveSet.Forward.Clip;
                Override($"Run_{dirSuffix[i]}", clip);
            }

            // Ataque / Hit / Death
            Override("Attack", attack.Clip);
            Override("ReceiveDamage", receiveDamage.Clip);
            Override("Death", death.Clip);

            // Skills
            for (int i = 0; i < skillAnimations.Length; ++i)
                Override($"Skill_{i}", skillAnimations[i].Clip);

            aoc.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = aoc;

            // ---------- 2. FSM ----------
            fsm = GetComponent<CharacterAnimationFSM>();
            if (fsm == null) fsm = gameObject.AddComponent<CharacterAnimationFSM>();
            fsm.enabled = useHFSM;
        }

        #endregion

        #region ------------- Events -------------

        private void OnRun(AnimationOnRunEvent evt)
        {
            if (evt.Target != this) return;
            fsm.SetMove(evt.dir);
        }

        private void OnIdle(AnimationOnIdleEvent evt)
        {
            if (evt.Target != this) return;
            fsm.SetMove(Vector2.zero);
        }

        private void OnDeath(AnimationOnDiedEvent evt)
        {
            if (evt.Target != this) return;
            fsm.PlayDeath();
        }

        private void OnAction(AnimationOnActionEvent evt)
        {
            if (evt.Target != this) return;
            if (evt.isAttack)
                fsm.PlayAttack();
            else
                fsm.PlaySkill(evt.skillIndex);
        }

        private void OnActionEnd(AnimationOnActionEndEvent evt)
        {
            if (evt.Target != this) return;
            EventBus.Publish(new AnimationOnRunEvent(this, Vector2.zero));
        }
        #endregion
        #region ICharacterAnimationContext implementation

        public Animator Animator => animator;
        public DirectionalAnimSet IdleSet => idleSet;
        public DirectionalAnimSet MoveSet => moveSet;
        public SkillAnimData Attack => attack;
        public AnimClipData ReceiveDamage => receiveDamage;
        public AnimClipData Death => death;

        public bool TryGetSkill(int index, out SkillAnimData data)
        {
            if (skillAnimations != null && index >= 0 && index < skillAnimations.Length)
            {
                data = skillAnimations[index];
                return true;
            }
            data = default;
            return false;
        }

        #endregion       
    }
}
