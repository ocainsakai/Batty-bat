using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using HFSM;
using static BulletHellTemplate.Core.FSM.AnimationEvent;
using static BulletHellTemplate.Core.FSM.AnimationState;
using System;

namespace BulletHellTemplate.Core.FSM
{
    /// <summary>
    /// High-level FSM that drives character animations using UnityHFSM.
    /// No external triggers are required for Locomotion; it evaluates
    /// movement each frame via a dynamic condition.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CharacterAnimationFSM : MonoBehaviour
    {
        #region Inspector
        [Tooltip("Component that implements ICharacterAnimationContext.")]
        private MonoBehaviour contextProvider;

        [Tooltip("Animator layer index for upper-body (masked) skills.")]
        [SerializeField] private int skillLayerIndex = 1;
        #endregion

        private ICharacterAnimationContext ctx;

        private StateMachine<AnimationState, AnimationEvent> root;
        private StateMachine<
            AnimationState, LocomotionSubState, AnimationEvent> locomotion;

        private Coroutine _actionRoutine;

        private Vector2 currentDir;
        private int pendingSkillIdx;
        private bool _isMoving;
        private bool _actionDone;
        private float _lastSkillLen;

        /*── health tracking ──────────────────*/
        private Func<bool> _isAlive;   // returns true while HP > 0
        private bool _prevAlive; // cached previous value

        /*──────────────────────────────────────────────*/
        #region Unity lifecycle
        private void Awake()
        {
            ctx = contextProvider != null
            ? contextProvider as ICharacterAnimationContext
            : GetComponent<ICharacterAnimationContext>();

            /* 2. Health checker ------------------------------------------*/
            Assert.IsNotNull(ctx, "No component implementing ICharacterAnimationContext was found on this GameObject.");

            if (TryGetComponent(out MonsterHealth mh))
                _isAlive = () => mh.CurrentHp > 0;
            else if (TryGetComponent(out CharacterStatsComponent ch))
                _isAlive = () => ch.CurrentHP > 0;
            else
                _isAlive = () => true;   // fallback: always alive

            _prevAlive = _isAlive();

            BuildFsm();
            root.Init();
        }

        /// <summary>
        /// Tick the FSM every frame so timers/conditions are processed
        /// even when the character is idle.
        /// </summary>
        private void Update()
        {
            bool alive = _isAlive();
            if (alive != _prevAlive)
            {
                if (!alive)              
                {
                    root.Trigger(OnDie);
                }
                else                     
                {
                    root.RequestStateChange(Locomotion);
                    ctx.Animator.SetLayerWeight(skillLayerIndex, 0);
                    _actionDone = false;
                }
                _prevAlive = alive;
            }
            root.OnLogic();
        }
        #endregion

        /*──────────────────────────────────────────────*/
        #region Public API
        /// <summary>Called every frame by gameplay layer.</summary>
        public void SetMove(Vector2 dir)
        {
            currentDir = dir;
            bool wasMoving = _isMoving;
            _isMoving = dir.sqrMagnitude >= 0.01f;
           
            if (root.ActiveStateName.Equals(Locomotion) &&
                locomotion.ActiveStateName.Equals(LocomotionSubState.Move))
            {
                UpdateBlend(currentDir);
            }
        }

        public void PlayAttack()
        {           
            root.Trigger(OnAttack);
        }
            
        public void PlaySkill(int idx)
        {    
            pendingSkillIdx = idx; 
            root.Trigger(OnSkill); 
        }
        public void PlayReceiveDamage()
        {
            root.Trigger(OnReceiveDamage);
        }
           
        public void PlayDeath()
        {
            
            root.Trigger(OnDie);
        }
            
        #endregion

        /*──────────────────────────────────────────────*/
        #region FSM setup
        private void BuildFsm()
        {
            root = new();

            /*──────────────── Locomotion (sub-FSM) ────────────────*/
            BuildLocomotionSubFsm();
            root.AddState(Locomotion, locomotion);

            /*──────────────── ATTACK ──────────────────────────*/
            root.AddState(Attack,
                onEnter: s =>
                {
                    PlaySkill(ctx.Attack, "Attack");
                    _actionDone = false;
                    StartActionTimer(ClipLength(ctx.Attack)); 
                },
                onExit: s =>
                {
                    ctx.Animator.SetLayerWeight(skillLayerIndex, 0);
                    StopActionTimer();
                });

            root.AddTransition(Attack, Locomotion, _ => _actionDone);

            /*──────────────── SKILL ───────────────────────────*/
            root.AddState(Skill,
                onEnter: s =>
                {
                    PlaySkillInternal();                    
                    _actionDone = false;
                    StartActionTimer(_lastSkillLen);            
                },
                onExit: s =>
                {
                    ctx.Animator.SetLayerWeight(skillLayerIndex, 0);
                    StopActionTimer();
                });

            root.AddTransition(Skill, Locomotion, _ => _actionDone);
            /*───── Receive Damage & Death ─────────────────────────*/
            root.AddState(ReceiveDamage,
                onEnter: s => Play("ReceiveDamage", ctx.ReceiveDamage));
            float dmgLen = ClipLength(ctx.ReceiveDamage);
            root.AddTransition(
                new TransitionAfter<AnimationState>(ReceiveDamage,
                                                    Locomotion, dmgLen));

            root.AddState(Dead,
                onEnter: s => Play("Death", ctx.Death),
                isGhostState: true);

            /*───── Global triggers (Attack overrides Skill etc.) ──*/
            AddGlobal(OnAttack, Attack);
            AddGlobal(OnSkill, Skill);
            AddGlobal(OnReceiveDamage, ReceiveDamage);
            AddGlobal(OnDie, Dead);

            /*───── Start ──────────────────────────────────────────*/
            root.SetStartState(Locomotion);
        }

        private void BuildLocomotionSubFsm()
        {
            locomotion = new();

            locomotion.AddState(
                LocomotionSubState.Idle,
                onEnter: s => PlayDirectional("Idle_Blend",
                                              ctx.IdleSet.Forward.Speed));

            locomotion.AddState(
                LocomotionSubState.Move,
                onEnter: s => PlayDirectional("Run_Blend",
                                              ctx.MoveSet.Forward.Speed));

            /* Dynamic transitions evaluated every OnLogic() */
            locomotion.AddTransition(
                LocomotionSubState.Idle,
                LocomotionSubState.Move,
                _ => _isMoving);

            locomotion.AddTransition(
                LocomotionSubState.Move,
                LocomotionSubState.Idle,
                _ => !_isMoving);

            locomotion.SetStartState(LocomotionSubState.Idle);
        }

        private void AddGlobal(AnimationEvent trig, AnimationState to)
        {
            root.AddTriggerTransitionFromAny(trig,
                new Transition<AnimationState>(default, to, _ => true));
        }
        #endregion

        /*──────────────────────────────────────────────*/
        #region Animation helpers
        private const string X = "MoveX", Y = "MoveY";

        private static float ClipLength(AnimClipData d) =>
            d.Clip ? d.Clip.length / Mathf.Max(0.1f, d.Speed) : 0.5f;

        private static float ClipLength(SkillAnimData d) =>
            d.Clip ? d.Clip.length / Mathf.Max(0.1f, d.Speed) : 0.5f;

        private void Play(string state, AnimClipData d, int layer = 0)
        {
            if (state == null) return;
            ctx.Animator.speed = d.Speed <= 0 ? 1 : d.Speed;
            ctx.Animator.CrossFadeInFixedTime(state, d.Transition, layer);
        }

        private void PlayDirectional(string state, float speed)
        {
            ctx.Animator.speed = speed <= 0 ? 1 : speed;
            ctx.Animator.CrossFadeInFixedTime(state, 0.1f, 0);
        }

        private void UpdateBlend(Vector2 dir)
        {
            ctx.Animator.SetFloat(X, dir.x, 0.1f, Time.deltaTime);
            ctx.Animator.SetFloat(Y, dir.y, 0.1f, Time.deltaTime);
        }

        private void PlaySkill(SkillAnimData d, string state)
        {
            int layer = (d.UseAvatarMask && d.Mask) ? skillLayerIndex : 0;
            ctx.Animator.SetLayerWeight(skillLayerIndex,
                                        layer == skillLayerIndex ? 1 : 0);
            ctx.Animator.speed = d.Speed <= 0 ? 1 : d.Speed;
            ctx.Animator.CrossFadeInFixedTime(state, d.Transition, layer);
        }

        private void PlaySkillInternal()
        {
            if (!ctx.TryGetSkill(pendingSkillIdx, out var d))
                return;

            PlaySkill(d, $"Skill_{pendingSkillIdx}");
            _lastSkillLen = ClipLength(d);
        }

        /// <summary>Starts a one-shot timer that will set _actionDone = true.</summary>
        private void StartActionTimer(float seconds)
        {
            StopActionTimer();
            _actionRoutine = StartCoroutine(ActionTimerRoutine(seconds));
        }

        private void StopActionTimer()
        {
            if (_actionRoutine != null)
                StopCoroutine(_actionRoutine);
            _actionRoutine = null;
        }

        private IEnumerator ActionTimerRoutine(float t)
        {
            yield return new WaitForSeconds(t);
            _actionDone = true;
        }
        #endregion
    }
}
