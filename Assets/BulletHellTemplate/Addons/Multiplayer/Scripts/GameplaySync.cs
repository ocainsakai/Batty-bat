using BulletHellTemplate.Core.Events;
using BulletHellTemplate.VFX;
using Cysharp.Threading.Tasks;
#if FUSION2
using Fusion;
#endif
using UnityEngine;

namespace BulletHellTemplate
{
    /// <summary>
    /// Networked singleton object that replicates global gameplay counters.
    /// Place it in the scene or let the Host spawn it via SpawnAsync.
    /// </summary>
#if FUSION2
    [RequireComponent(typeof(NetworkObject))]
#endif
    public sealed class GameplaySync :
#if FUSION2
        NetworkBehaviour   
#else
        MonoBehaviour
#endif
    {
        public static GameplaySync Instance { get; private set; }
        private GameplayManager GM => GameplayManager.Singleton;
        public enum PerkKind : byte { SkillPerk = 0, StatPerk = 1, BaseSkill = 2 }
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else Destroy(gameObject);
        }

        private void OnDestroy()
        {       
            if (Instance == this) Instance = null;
        }

#if FUSION2
        // ───── Utility Properties ─────
        [Networked, OnChangedRender(nameof(OnMatchStarted))]
        public NetworkBool MatchStarted { get; set; }
        [Networked, OnChangedRender(nameof(OnTimerChanged))]
        public int TimerSecs { get; set; }
        private float _secAcc = 0f;
        public bool RunnerActive => Runner && Runner.IsRunning;
        public bool IsHost => RunnerActive && HasStateAuthority;


        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !MatchStarted)
                return;

            _secAcc += Runner.DeltaTime;
            if (_secAcc >= 1f)
            {
                _secAcc -= 1f;

                if (TimerSecs > 0)
                {
                    TimerSecs--;
                }
            }
        }

        // ───── RPCs called on all clients ─────
        private void OnMatchStarted()
        {
            if (!MatchStarted) return;

            if (GameplayManager.Singleton)
                GameplayManager.Singleton.upgradeMode = UpgradeMode.UpgradeOnButtonClick;

            GameplayManager.Singleton?.StartGameplay();

            if (HasStateAuthority)
                TimerSecs = GameplayManager.Singleton.GetSurvivalTime();
        }

        private void OnTimerChanged()
        {
            if (GameplayManager.Singleton)
                GameplayManager.Singleton.SetTimeRemaining(TimerSecs);

            EventBus.Publish(new GameTimerTickEvent(TimerSecs));
        }

        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
        private void RPC_PlayerDied(NetworkId playerId)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            var ce = nobj.GetComponent<CharacterEntity>();
            if (ce && !ce.IsDead) ce.OnDeath();
        }

        public void SyncPlayerDied(CharacterEntity ce)
        {
            if (!RunnerActive) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;
            RPC_PlayerDied(nob.Id);
        }

        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
        private void RPC_PlayerRevived(NetworkId playerId)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            var ce = nobj.GetComponent<CharacterEntity>();
            if (ce && ce.IsDead) ce.CharacterRevive();

            // fechar UI de revive no dono (se for local e é ele)
            if (nobj.HasInputAuthority) UIGameplay.Singleton?.HideRevivePanel();
        }

        public void SyncPlayerRevived(CharacterEntity ce)
        {
            if (!RunnerActive) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;

            RPC_PlayerRevived(nob.Id);
            if (HasStateAuthority) RPC_DeathEnd(nob.Id);
        }

        // UI de revive para dono do personagem
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_StartReviveCountdown(NetworkId playerId, int seconds)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            if (nobj.HasInputAuthority)
                UIGameplay.Singleton?.ShowRevivePanel(seconds);
        }
        public void NotifyStartReviveCountdown(CharacterEntity ce, int seconds)
        {
            if (!RunnerActive || !HasStateAuthority) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;

            RPC_StartReviveCountdown(nob.Id, seconds);
            RPC_DeathStart(nob.Id, seconds);         
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ReviveTick(NetworkId playerId, int remain)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            if (nobj.HasInputAuthority)
                UIGameplay.Singleton?.UpdateReviveCountdown(remain);
        }
        public void NotifyReviveTick(CharacterEntity ce, int remain)
        {
            if (!RunnerActive || !HasStateAuthority) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;

            RPC_ReviveTick(nob.Id, remain);   
            RPC_DeathTick(nob.Id, remain);   
        }

        /// <summary>
        /// Broadcast a team defeat (no revive online). Called only by StateAuthority.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_TeamDefeat()
        {
            GameplayManager.Singleton.SetMatchFinished();
            GameplayManager.Singleton.CancelAllRevives();

            GameplayManager.Singleton.PauseGame();
            UIGameplay.Singleton.DisplayEndGameScreen(false);
            UIGameplay.Singleton.ClearAllDeathEntries();
        }

        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
        private void RPC_ApplyPerk(NetworkId playerId, PerkKind kind, int index)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            var ce = nobj.GetComponent<CharacterEntity>();
            if (!ce) return;

            var gm = GameplayManager.Singleton;

            switch (kind)
            {
                case PerkKind.SkillPerk:
                    var sp = gm.skillPerkData[index];
                    gm.SetPerkLevel(sp);
                    ce.GetComponent<CharacterAttackComponent>()?.ApplySkillPerk(sp, /*isReplica*/ true);
                    break;

                case PerkKind.StatPerk:
                    var st = gm.statPerkData[index];
                    gm.SetPerkLevel(st);
                    ce.ApplyStatPerk(st, gm.GetPerkLevel(st)); 
                    break;

                case PerkKind.BaseSkill:
                    var bs = ce.GetCharacterData().skills[index];
                    gm.LevelUpBaseSkill(bs);
                    break;
            }

            UIGameplay.Singleton?.UpdateSkillPerkUI();
            UIGameplay.Singleton?.UpdateStatPerkUI();
        }

        // ── Death feed  ─────────────────────────────────
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DeathStart(NetworkId playerId, int seconds)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            var ce = nobj.GetComponent<CharacterEntity>();
            UIGameplay.Singleton?.ShowDeathEntry(nobj, ce, seconds); 
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DeathTick(NetworkId playerId, int remain)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            UIGameplay.Singleton?.UpdateDeathCountdown(nobj, remain);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_DeathEnd(NetworkId playerId)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;
            UIGameplay.Singleton?.HideDeathEntry(nobj);
        }

        public void SyncPerkChosen(CharacterEntity ce, PerkKind kind, int index)
        {
            if (!RunnerActive) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;
            RPC_ApplyPerk(nob.Id, kind, index);
        }

        /// <summary>
        /// Host/leader calls this when he detects a full team wipe.
        /// </summary>
        public void SyncTeamDefeat()
        {
            if (!RunnerActive || !HasStateAuthority)
                return;

            RPC_TeamDefeat();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_SetGold(int total)
        {
            if (!HasStateAuthority)
                GM.ForceSetGold(total);
        }
           

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void RPC_SetXP(int total)
        {
            if (!HasStateAuthority)
                GM.ForceSetXP(total);
        }
           
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SetMonstersKilled(int total)
        {
            if (!HasStateAuthority)
                GM.ForceSetMonstersKilled(total);
        }
           

        // ───── APIs called by GameplayManager ─────
        public void SyncGold(int total)
        {
            if (!RunnerActive)
                GM.ForceSetGold(total);         // offline mode
            else if (IsHost)
                RPC_SetGold(total);             // host sends
        }

        public void SyncXP(int total)
        {
            if (!RunnerActive)
                GM.ForceSetXP(total);
            else if (IsHost)
                RPC_SetXP(total);
        }

        public void SyncMonsters(int total)
        {
            if (!RunnerActive)
                GM.ForceSetMonstersKilled(total);
            else if (IsHost)
                RPC_SetMonstersKilled(total);
        }
        /// <summary>
        /// RPC that tells _all_ clients to spawn a drop for the given monster.
        /// </summary>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnDropForMonster(NetworkId monsterId, bool isGold, Vector3 position, int amount)
        {

            if (!Runner.TryFindObject(monsterId, out var nobj))
                return;

            var me = nobj.GetComponent<BaseMonsterEntity>();
            if (me == null)
                return;

            var prefab = isGold ? me.GoldDropPrefab : me.ExpDropPrefab;
            if (prefab == null)
                return;

            var drop = DropPool.Spawn(prefab, position);
            drop.SetValue(amount);
        }

        /// <summary>
        /// Called by Host to sync a drop on all clients.
        /// </summary>
        public void SyncSpawnDrop(BaseMonsterEntity monster, bool isGold, Vector3 position, int amount)
        {
            if (!RunnerActive || !HasStateAuthority)
                return;

            var nob = monster.GetComponent<NetworkObject>();
            if (nob == null)
                return;

            RPC_SpawnDropForMonster(nob.Id, isGold, position, amount);
        }


        /// <summary>
        /// RPC to configure monster values after it has been spawned.
        /// Called on all clients. The target monster is identified by its NetworkId.
        /// </summary>
        /// <param name="netId">Network ID of the monster</param>
        /// <param name="gold">Gold reward value</param>
        /// <param name="xp">XP reward value</param>
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ConfigMonster(NetworkId netId, int gold, int xp)
        {
            if (Runner.TryFindObject(netId, out var nobj))
            {
                var me = nobj.GetComponent<MonsterEntity>();
                me?.ConfigureMonster(gold, xp);
            }
        }

        /// <summary>
        /// Called by the Host after SpawnAsync to synchronize monster configuration.
        /// Does nothing in offline mode. Only the Host should call this.
        /// </summary>
        /// <param name="nobj">Spawned NetworkObject</param>
        /// <param name="gold">Gold reward value</param>
        /// <param name="xp">XP reward value</param>
        public void SyncConfigureMonster(NetworkObject nobj, int gold, int xp)
        {
            if (!RunnerActive) return; 
            if (IsHost)
                RPC_ConfigMonster(nobj.Id, gold, xp);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_MonsterSkillCast(NetworkId monsterId, int skillIndex, Vector3 dir)
        {
            if (Runner.TryFindObject(monsterId, out var nobj))
            {
                var me = nobj.GetComponent<MonsterEntity>();
                me?.SkillRunner?.ExecuteSkill_Remote(skillIndex, dir);

                me?.PlaySkillAnimNet(skillIndex); 
            }
        }

        public void SyncMonsterSkillCast(MonsterEntity monster, int skillIndex, Vector3 dir)
        {
            if (!RunnerActive || !IsHost) return;
            var nob = monster.GetComponent<NetworkObject>();
            if (!nob) return;
            RPC_MonsterSkillCast(nob.Id, skillIndex, dir);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_MonsterHp(NetworkId monsterId, float hp)
        {
            if (!Runner.TryFindObject(monsterId, out var nobj))
                return;

            if (nobj.HasStateAuthority)
                return;

            var mh = nobj.GetComponent<MonsterHealth>();
            mh?.ApplyRemoteHp(hp);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BossSpawned(NetworkId bossId)
        {
            if (Runner.TryFindObject(bossId, out var nobj))
            {
                var me = nobj.GetComponent<MonsterEntity>();
                UIGameplay.Singleton?.SetFinalBoss(me);
                UIGameplay.Singleton?.ShowFinalBossMessage();
            }
        }

        public void SyncBossSpawned(MonsterEntity boss)
        {
            if (!RunnerActive || !HasStateAuthority) return;
            var nob = boss.GetComponent<NetworkObject>();
            if (!nob) return;
            RPC_BossSpawned(nob.Id);
        }

        // ─── RPC for auto‐attack replication ───────────────────────────────────

        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
        private void RPC_PlayerAttack(NetworkId playerId, Vector3 dir)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;

            var comp = nobj.GetComponent<CharacterAttackComponent>();
            if (comp == null) return;

            bool replica = !nobj.HasStateAuthority; 
            comp.AttackInternal(dir, replica);
        }

        public void SyncPlayerAttack(CharacterEntity ce, Vector3 dir)
        {
            if (!RunnerActive) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;
            RPC_PlayerAttack(nob.Id, dir);
        }

        // ───────── SKILL ───────────────────────────────────────────────
        [Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = false)]
        private void RPC_PlayerUseSkill(NetworkId playerId, int idx, Vector2 input)
        {
            if (!Runner.TryFindObject(playerId, out var nobj)) return;

            var atk = nobj.GetComponent<CharacterAttackComponent>();
            if (atk == null) return;

            var skill = atk.CharacterOwner.GetCharacterData().skills[idx];
            bool replica = !nobj.HasStateAuthority; // host
            atk.UseSkillInternal(idx, skill, input, replica);
        }

        public void SyncPlayerUseSkill(CharacterEntity ce, int idx, Vector2 input)
        {
            if (!RunnerActive) return;
            var nob = ce.GetComponent<NetworkObject>();
            if (!nob) return;
            RPC_PlayerUseSkill(nob.Id, idx, input);
        }


        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_EndGame()
        {
            if (!HasStateAuthority)
                GameplayManager.Singleton.EndGame();
        }

        /// <summary>
        /// API pública: apenas o Host Shared‑Mode deve chamar para encerrar o jogo.
        /// </summary>
        public void SyncEndGame()
        {
            if (!Runner || !Runner.IsRunning || !HasStateAuthority)
                return;

            RPC_EndGame();
        }
#endif
    }
}
