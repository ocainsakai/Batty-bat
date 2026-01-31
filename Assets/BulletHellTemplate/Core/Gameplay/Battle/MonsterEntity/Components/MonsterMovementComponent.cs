using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Handles all locomotion for an entity that owns a <see cref="NavMeshAgent"/>.
    /// * Receives its configuration via <see cref="Configure"/>.
    /// * Keeps itself paused when the game is paused.
    /// * Exposes coroutines for slow, stun and knock-back.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public sealed class MonsterMovementComponent : MonoBehaviour
    {
        private float orbitAngularSpeed = 720f;
        private NavMeshAgent agent;
        private float baseSpeed;
        private float currentSpeed;
        private bool isPaused;  
        private CancellationTokenSource slowCts, stunCts, dashCts;
        private bool stunned;

        private Transform lookTarget;

        public void SetLookTarget(Transform t) => lookTarget = t;
        public void ClearLookTarget() => lookTarget = null;
        public void SetStunned() => stunned = true;
        private bool AgentReady =>
            agent != null && agent.enabled && agent.isActiveAndEnabled && agent.isOnNavMesh;
        public Vector3 WorldVelocity => AgentReady ? agent.velocity : Vector3.zero;

        private void Awake() => agent = GetComponent<NavMeshAgent>();
        /// <summary>Sets the base speed and any agent parameters decided by the owner.</summary>
        /// <summary>Initial agent setup called by owner.</summary>
        public void Configure(float moveSpeed)
        {
            agent ??= GetComponent<NavMeshAgent>();
            baseSpeed = moveSpeed;
            currentSpeed = moveSpeed;

            agent.speed = moveSpeed;
            agent.updateRotation = false; 
            agent.updateUpAxis = false;   
            agent.stoppingDistance = 0f;
        }

#if FUSION2
        public void DisableAgentForReplica()
        {
            if (agent) agent.enabled = false;
        }
#endif


        private void LateUpdate()
        {
            bool paused = GameplayManager.Singleton.IsPaused();

            if (AgentReady)
            {
                if (paused && !agent.isStopped) agent.isStopped = true;
                else if (!paused && agent.isStopped && !stunned) agent.isStopped = false;

                if (lookTarget)
                {
                    Vector3 dir = (lookTarget.position - transform.position);
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.001f)
                        transform.forward = dir.normalized;
                }
                else if (agent.velocity.sqrMagnitude > 0.01f)
                {
                    transform.forward = agent.velocity.normalized;
                }

            }
        }

        /* ───── public API ───────────── */

        /// <summary>Path-finds directly to a world-space point.</summary>
        public void Follow(Vector3 worldPos)
        {
            if (!IsAbleToMove() || !AgentReady) return;
            agent.stoppingDistance = 0f;
            agent.SetDestination(worldPos);
        }


        public void Flee(Vector3 from)
        {
            if (!IsAbleToMove()) return;
            Vector3 dir = (transform.position - from).normalized;
            agent.stoppingDistance = 0f;
            SetImmediate(dir, currentSpeed);
        }

        /// <summary>
        /// Orbits around <paramref name="tgt"/> trying to keep <paramref name="radius"/>.
        /// </summary>
        public void StrafeAround(Transform tgt, float radius)
        {
            if (!IsAbleToMove() || !tgt) return;

            Vector3 toTarget = transform.position - tgt.position;
            toTarget.y = 0f;

            float dist = toTarget.magnitude;
            float delta = dist - radius;

            Vector3 radial = delta > .2f ? -toTarget.normalized
                              : delta < -.2f ? toTarget.normalized
                              : Vector3.zero;

            Vector3 tangent = Quaternion.Euler(0f, orbitAngularSpeed * Time.deltaTime, 0f)
                              * toTarget;
            tangent = Vector3.Cross(Vector3.up, tangent).normalized;

            Vector3 moveDir = (tangent + radial).normalized;
            SetImmediate(moveDir, currentSpeed);
        }   

        public void ApplySlow(float pct, float dur)
        {
            slowCts?.Cancel();
            slowCts = new CancellationTokenSource();
            SlowRoutine(pct, dur, slowCts.Token).Forget();
        }

        public void ApplyStun(float dur)
        {
            stunCts?.Cancel();
            stunCts = new CancellationTokenSource();
            StunRoutine(dur, stunCts.Token).Forget();
        }

        public void ApplyKnockback(Vector3 from, float dist, float dur)
        {
            Vector3 dir = (transform.position - from).normalized;
            KnockRoutine(dir, dist, dur).Forget();
        }

        public void ApplyDash(Vector3 dir, float speed, float dur)
        {
            dashCts?.Cancel();
            dashCts = new CancellationTokenSource();
            DashRoutine(dir.normalized, speed, dur, dashCts.Token).Forget();
        }


        /* ───── coroutines (UniTask) ───────────────────────────────────── */

        private async UniTaskVoid SlowRoutine(float pct, float dur, CancellationToken token)
        {
            currentSpeed = baseSpeed * (1f - pct);
            agent.speed = currentSpeed;
            try { await UniTask.Delay(TimeSpan.FromSeconds(dur), cancellationToken: token); }
            catch (OperationCanceledException) { }

            currentSpeed = baseSpeed;
            agent.speed = currentSpeed;
        }

        private async UniTaskVoid StunRoutine(float dur, CancellationToken token)
        {
            stunned = true;
            if (AgentReady) agent.isStopped = true;
            try { await UniTask.Delay(TimeSpan.FromSeconds(dur), cancellationToken: token); }
            catch (OperationCanceledException) { }

            stunned = false;
            if (AgentReady) agent.isStopped = GameplayManager.Singleton.IsPaused();
        }

        private async UniTaskVoid KnockRoutine(Vector3 dir, float dist, float dur)
        {
            var token = this.GetCancellationTokenOnDestroy();

            if (AgentReady) agent.isStopped = true;

            float elapsed = 0f;
            try
            {
                while (elapsed < dur && !token.IsCancellationRequested)
                {
                    transform.position += dir * (dist / dur) * Time.deltaTime;
                    elapsed += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { }

            if (token.IsCancellationRequested || this == null) return;

            if (AgentReady)
            {
                agent.isStopped = GameplayManager.Singleton.IsPaused() || stunned;
                agent.ResetPath();
            }
        }

        private async UniTaskVoid DashRoutine(Vector3 dir, float spd, float dur, CancellationToken token)
        {
            if (AgentReady) agent.isStopped = true;
            float t = 0f;
            try
            {
                while (t < dur && !token.IsCancellationRequested)
                {
                    transform.position += dir * spd * Time.deltaTime;
                    t += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException) { }
            if (AgentReady)
            {
                agent.isStopped = GameplayManager.Singleton.IsPaused() || stunned;
                agent.ResetPath();
            }
        }

        /* ───── helpers ──────────────────────────────────────────────── */

        private bool IsAbleToMove() => agent && agent.isActiveAndEnabled && !stunned;

        /// <summary>1-frame impulse ignoring NavMesh path-planning.</summary>
        private void SetImmediate(Vector3 dir, float speed)
        {
            Vector3 next = transform.position + dir * speed * Time.deltaTime;
            agent.Warp(next);   
        }

        public void Shutdown()
        {
            if (AgentReady) agent.isStopped = true;
            slowCts?.Cancel(); stunCts?.Cancel(); dashCts?.Cancel();
        }
    }
}
