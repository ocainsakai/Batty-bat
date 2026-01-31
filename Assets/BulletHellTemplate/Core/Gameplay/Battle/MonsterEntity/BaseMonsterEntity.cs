using BulletHellTemplate.Core.FSM;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace BulletHellTemplate
{
    /// <summary>
    /// Core logic shared by every monster-type entity (drops, death, XP, gold).
    /// </summary>
    /// 
#if FUSION2
    using Fusion;
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkTransform))]
#endif
    [RequireComponent(typeof(MonsterHealth))]
    [RequireComponent(typeof(MonsterMovementComponent))]
    [RequireComponent(typeof(MonsterSkillRunner))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterAnimationFSM))]
    public abstract partial class BaseMonsterEntity :
#if FUSION2
        NetworkBehaviour   
#else
        MonoBehaviour    
#endif
    {
        [Header("Settings on death")]
        protected int goldValue = 1;
        protected int xpValue = 1;
        [SerializeField] protected float destroyDelay = 0f;
        [SerializeField] protected bool dropGold = true;
        [SerializeField] protected bool dropExp = true;
        [SerializeField] protected DropEntity goldDropPrefab;
        [SerializeField] protected DropEntity expDropPrefab;

        public DropEntity GoldDropPrefab => goldDropPrefab;
        public DropEntity ExpDropPrefab => expDropPrefab;

        const float SCATTER_RADIUS = 1.2f;

        private CancellationTokenSource _deathCts;
        protected MonsterHealth HealthComponent { get; private set; }
        protected MonsterMovementComponent MovementComponent { get; private set; }
        protected MonsterSkillRunner MonsterSkillRunner { get; private set; }

        protected virtual void Awake()
        {
            HealthComponent = GetComponent<MonsterHealth>();
            MovementComponent = GetComponent<MonsterMovementComponent>();
            MonsterSkillRunner = GetComponent<MonsterSkillRunner>();
        }

        protected virtual void OnEnable()
        {
            if (HealthComponent != null) HealthComponent.OnDeath += HandleDeath;
        }

        protected virtual void OnDisable()
        {
            if (HealthComponent != null) HealthComponent.OnDeath -= HandleDeath;
        }

        protected virtual void StartConfigureMonster(int gold, int exp)
        {
           if (HealthComponent == null) HealthComponent = GetComponent<MonsterHealth>();
           if (!MovementComponent) MovementComponent = GetComponent<MonsterMovementComponent>();
           if (!MonsterSkillRunner) MonsterSkillRunner = GetComponent<MonsterSkillRunner>();
            goldValue = gold;
            xpValue = exp;
        }

        Vector3 ScatterAround(Vector3 center)
        {
            Vector2 rnd = UnityEngine.Random.insideUnitCircle * SCATTER_RADIUS;
            return new Vector3(center.x + rnd.x,
                               center.y,        
                               center.z + rnd.y);
        }

        /// <summary>
        /// Handles monster death both offline and online.
        /// </summary>
        protected virtual void HandleDeath()
        {
            _deathCts?.Cancel();
            MovementComponent.Shutdown();

#if FUSION2
            // ─── Check if a NetworkRunner is running ───────────────────
            NetworkRunner runner = NetworkRunner.GetRunnerForGameObject(gameObject);
            bool isNetSession    = runner && runner.IsRunning && runner.State == NetworkRunner.States.Running;

            if (isNetSession && Object.HasStateAuthority)
            {
                HandleNetworkedDrops();     // EXP / GOLD via RPC
                ScheduleDespawn();          // Runner.Despawn dentro de MonsterPool
                return;
            }
#endif
            // ─── Offline  ─────────────────────
            HandleLocalDrops();              // EXP / GOLD locais
            ScheduleDespawn();           
        }

        /*────────────────────────── Helpers  ───────────────────────────*/

        /// <summary>
        /// Spawns EXP / GOLD drops locally and updates gameplay counters.
        /// </summary>
        private void HandleLocalDrops()
        {
            Vector3 dropPos;

            // EXP
            if (dropExp && expDropPrefab)
            {
                dropPos = ScatterAround(transform.position);
                DropPool.Spawn(expDropPrefab, dropPos).SetValue(xpValue);
            }
            else
            {
                GameplayManager.Singleton.IncrementGainXP(xpValue);
            }

            // GOLD
            if (dropGold && goldDropPrefab)
            {
                dropPos = ScatterAround(transform.position);
                DropPool.Spawn(goldDropPrefab, dropPos).SetValue(goldValue);
            }
            else
            {
                GameplayManager.Singleton.IncrementGainGold(goldValue);
            }

            GameplayManager.Singleton.IncrementMonstersKilled();
        }

#if FUSION2
        /// <summary>
        /// Spawns EXP / GOLD drops in a network-safe way (host RPC).
        /// </summary>
        private void HandleNetworkedDrops()
        {
            Vector3 pos;

            // EXP
            if (dropExp && expDropPrefab)
            {
                pos = ScatterAround(transform.position);
                GameplaySync.Instance?.SyncSpawnDrop(this, false, pos, xpValue);
            }
            else
            {
                GameplayManager.Singleton.IncrementGainXP(xpValue);
            }

            // GOLD
            if (dropGold && goldDropPrefab)
            {
                pos = ScatterAround(transform.position);
                GameplaySync.Instance?.SyncSpawnDrop(this, true, pos, goldValue);
            }
            else
            {
                GameplayManager.Singleton.IncrementGainGold(goldValue);
            }

            GameplayManager.Singleton.IncrementMonstersKilled();
        }
#endif

        /// <summary>
        /// Waits for <c>destroyDelay</c> and then returns the instance to the pool
        /// (or lets Fusion despawn it when online).
        /// </summary>
        private void ScheduleDespawn()
        {
            UniTask.Void(async () =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(destroyDelay));

#if FUSION2
                if (NetworkRunner.GetRunnerForGameObject(gameObject))
                {
                    MonsterPool.Instance.Release(gameObject); // chama Despawn
                    return;
                }
#endif
                MonsterPool.Instance.Release(gameObject);     // queue local
            });
        }
    }
}
